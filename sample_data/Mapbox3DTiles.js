const MERCATOR_A = 6378137.0;
const WORLD_SIZE = MERCATOR_A * Math.PI * 2;

ThreeboxConstants = {
  WORLD_SIZE: WORLD_SIZE,
  PROJECTION_WORLD_SIZE: WORLD_SIZE / (MERCATOR_A * Math.PI * 2),
  MERCATOR_A: MERCATOR_A,
  DEG2RAD: Math.PI / 180,
  RAD2DEG: 180 / Math.PI,
  EARTH_CIRCUMFERENCE: 40075000, // In meters
}
  
/* 
  mapbox-gl uses a camera fixed at the orgin (the middle of the canvas) The camera is only updated when rotated (bearing angle), 
  pitched or when the map view is resized.
  When panning and zooming the map, the desired part of the world is translated and zoomed in front of the camera. The world is only updated when
  the map is panned or zoomed.

  The mapbox-gl internal coordinate system has origin (0,0) located at longitude -180 degrees and latitude 0 degrees. 
  The scaling is 2^map.getZoom() * 512/EARTH_CIRCUMFERENCE_IN_METERS. At zoom=0 (scale=2^0=1), the whole world fits in 512 units.
*/
class CameraSync {
  constructor (map, camera, world) {
    this.map = map;
    this.camera = camera;
    this.active = true;
    this.updateCallback = ()=>{};
    
    this.camera.matrixAutoUpdate = false;   // We're in charge of the camera now!
  
    // Postion and configure the world group so we can scale it appropriately when the camera zooms
    this.world = world || new THREE.Group();
    this.world.position.x = this.world.position.y = ThreeboxConstants.WORLD_SIZE/2;
    this.world.matrixAutoUpdate = false;
  
    //set up basic camera state
    this.state = {
      fov: 0.6435011087932844, // Math.atan(0.75);
      translateCenter: new THREE.Matrix4(),
      worldSizeRatio: 512/ThreeboxConstants.WORLD_SIZE
    };
  
    this.state.translateCenter.makeTranslation(ThreeboxConstants.WORLD_SIZE/2, -ThreeboxConstants.WORLD_SIZE / 2, 0);
  
    // Listen for move events from the map and update the Three.js camera. Some attributes only change when viewport resizes, so update those accordingly
    this.map.on('move', ()=>this.updateCamera());
    this.map.on('resize', ()=>this.setupCamera());
    //this.map.on('moveend', ()=>this.updateCallback())

    this.setupCamera();
  }
  setupCamera() {
    var t = this.map.transform
    const halfFov = this.state.fov / 2;
    var cameraToCenterDistance = 0.5 / Math.tan(halfFov) * t.height;
    
    this.state.cameraToCenterDistance = cameraToCenterDistance;
    this.state.cameraTranslateZ = new THREE.Matrix4().makeTranslation(0,0,cameraToCenterDistance);
  
    this.updateCamera();
  }  
  updateCamera(ev) {
  
    if(!this.camera) {
      console.log('nocamera')
      return;
    }
  
    var t = this.map.transform
  
    var halfFov = this.state.fov / 2;
    const groundAngle = Math.PI / 2 + t._pitch;
    this.state.topHalfSurfaceDistance = Math.sin(halfFov) * this.state.cameraToCenterDistance / Math.sin(Math.PI - groundAngle - halfFov);
  
    // Calculate z distance of the farthest fragment that should be rendered.
    const furthestDistance = Math.cos(Math.PI / 2 - t._pitch) * this.state.topHalfSurfaceDistance + this.state.cameraToCenterDistance;
  
    // Add a bit extra to avoid precision problems when a fragment's distance is exactly `furthestDistance`
    const farZ = furthestDistance * 1.01;    
  
    this.camera.projectionMatrix = this.makePerspectiveMatrix(this.state.fov, t.width / t.height, 1, farZ);
    
  
    var cameraWorldMatrix = new THREE.Matrix4();
    var rotatePitch = new THREE.Matrix4().makeRotationX(t._pitch);
    var rotateBearing = new THREE.Matrix4().makeRotationZ(t.angle);
  
    // Unlike the Mapbox GL JS camera, separate camera translation and rotation out into its world matrix
    // If this is applied directly to the projection matrix, it will work OK but break raycasting
  
    cameraWorldMatrix
      .premultiply(this.state.cameraTranslateZ)
      .premultiply(rotatePitch)
      .premultiply(rotateBearing);
    
  
    this.camera.matrixWorld.copy(cameraWorldMatrix);
    
    // Handle scaling and translation of objects in the map in the world's matrix transform, not the camera
    let zoomPow = t.scale * this.state.worldSizeRatio;
    let scale = new THREE.Matrix4();
    scale.makeScale( zoomPow, zoomPow, zoomPow );
  
    let translateMap = new THREE.Matrix4();
    
    let x = -this.map.transform.x || -this.map.transform.point.x;
    let y = this.map.transform.y || this.map.transform.point.y;
    
    translateMap.makeTranslation(x, y, 0);
    
    this.world.matrix = new THREE.Matrix4;
    this.world.matrix
      //.premultiply(rotateMap)
      .premultiply(this.state.translateCenter)
      .premultiply(scale)
      .premultiply(translateMap);
    let matrixWorldInverse = new THREE.Matrix4();
    matrixWorldInverse.getInverse(this.world.matrix);

    this.camera.projectionMatrixInverse.getInverse(this.camera.projectionMatrix);
    this.camera.matrixWorldInverse.getInverse(this.camera.matrixWorld);
    this.frustum = new THREE.Frustum();
    this.frustum.setFromProjectionMatrix(new THREE.Matrix4().multiplyMatrices(this.camera.projectionMatrix, this.camera.matrixWorldInverse));
    
    this.cameraPosition = new THREE.Vector3(0,0,0).unproject(this.camera).applyMatrix4(matrixWorldInverse);

    this.updateCallback();
  }
  makePerspectiveMatrix(fovy, aspect, near, far) {
  
    let out = new THREE.Matrix4();
    let f = 1.0 / Math.tan(fovy / 2),
    nf = 1 / (near - far);
  
    let newMatrix = [
      f / aspect, 0, 0, 0,
      0, f, 0, 0,
      0, 0, (far + near) * nf, -1,
      0, 0, (2 * far * near) * nf, 0
    ]
  
    out.elements = newMatrix
    return out;
  }
}

class TileSet {
  constructor(){
    this.url = null;
    this.version = null;
    this.gltfUpAxis = 'Z';
    this.geometricError = null;
    this.root = null;
  }
  // TileSet.load
  async load(url, styleParams) {
    this.url = url;
    let resourcePath = THREE.LoaderUtils.extractUrlBase(url);
    
    let response = await fetch(this.url);
    if (!response.ok) {
      throw new Error(`HTTP ${response.status} - ${response.statusText}`);
    }
    let json = await response.json();    
    this.version = json.asset.version;
    this.geometricError = json.geometricError;
    this.refine = json.refine ? json.refine.toUpperCase() : 'ADD';
    this.root = new ThreeDeeTile(json.root, resourcePath, styleParams, this.refine);
    return;
  }
}

class ThreeDeeTile {
  constructor(json, resourcePath, styleParams, parentRefine, parentTransform) {
    this.loaded = false;
    this.styleParams = styleParams;
    this.resourcePath = resourcePath;
    this.totalContent = new THREE.Group();  // Three JS Object3D Group for this tile and all its children
    this.totalContent.name = `totalContent`;
    this.tileContent = new THREE.Group();    // Three JS Object3D Group for this tile's content
    this.tileContent.name = `tileContent  ${json.content?json.content.uri:'no content'}`;
    this.childContent = new THREE.Group();    // Three JS Object3D Group for this tile's children
    this.childContent.name = "childContent";
    this.totalContent.add(this.tileContent);
    this.totalContent.add(this.childContent);
    this.boundingVolume = json.boundingVolume;
    if (this.boundingVolume && this.boundingVolume.box) {
      let b = this.boundingVolume.box;
      let extent = [b[0] - b[3], b[1] - b[7], b[0] + b[3], b[1] + b[7]];
      let sw = new THREE.Vector3(extent[0], extent[1], b[2] - b[11]);
      let ne = new THREE.Vector3(extent[2], extent[3], b[2] + b[11]);
      this.box = new THREE.Box3(sw, ne);
      if (Mapbox3DTiles.DEBUG) {
        let geom = new THREE.BoxGeometry(b[3] * 2, b[7] * 2, b[11] * 2);
        let edges = new THREE.EdgesGeometry( geom );
        this.debugColor = new THREE.Color( 0xffffff );
        this.debugColor.setHex( Math.random() * 0xffffff );
        let line = new THREE.LineSegments( edges, new THREE.LineBasicMaterial( {color:this.debugColor }) );
        let trans = new THREE.Matrix4().makeTranslation(b[0], b[1], b[2]);
        line.applyMatrix4(trans);
        this.debugLine = line;
      }
    } else {
      this.extent = null;
      this.sw = null;
      this.ne = null;
      this.box = null;
      this.center = null;
    }
    this.refine = json.refine ? json.refine.toUpperCase() : parentRefine;
    this.geometricError = json.geometricError;
    this.worldTransform = parentTransform ? parentTransform.clone() : new THREE.Matrix4();
    this.transform = json.transform;
    if (this.transform) 
    { 
      let tileMatrix = new THREE.Matrix4().fromArray(this.transform);
      this.totalContent.applyMatrix4(tileMatrix);
      this.worldTransform.multiply(tileMatrix);
    }
    this.content = json.content;
    this.children = [];
    if (json.children) {
      for (let i=0; i<json.children.length; i++){
        let child = new ThreeDeeTile(json.children[i], resourcePath, styleParams, this.refine, this.worldTransform);
        this.childContent.add(child.totalContent);
        this.children.push(child);
      }
    }
  }
  //ThreeDeeTile.load
  async load() {
    if (this.unloadedTileContent) {
      this.totalContent.add(this.tileContent);
      this.unloadedTileContent = false;
    }
    if (this.unloadedChildContent) {
      this.totalContent.add(this.childContent);
      this.unloadedChildContent = false;
    }
    if (this.unloadedDebugContent) {
      this.totalContent.add(this.debugLine);
      this.unloadedDebugContent = false;
    }
    if (this.loaded) {
      return;
    }
    this.loaded = true;
    if (this.debugLine) {        
      this.totalContent.add(this.debugLine);
    }
    if (this.content) {
      let url = this.content.uri ? this.content.uri : this.content.url;
      if (!url) return;
      if (url.substr(0, 4) != 'http')
        url = this.resourcePath + url;
      let type = url.slice(-4);
      switch (type) {
        case 'json':
          // child is a tileset json
          try {
            let tileset = new TileSet();
            await tileset.load(url, this.styleParams);
            this.children.push(tileset.root);
            if (tileset.root) {
              if (tileset.root.transform) {
                tileset.root.totalContent.applyMatrix4(new THREE.Matrix4().fromArray(tileset.root.transform));
              }
              this.childContent.add(tileset.root.totalContent);
            }
          } catch (error) {
            console.error(error);
          }
          break;
        case 'b3dm':
          try {
            let loader = new THREE.GLTFLoader();
            let b3dm = new B3DM(url);
            let rotateX = new THREE.Matrix4().makeRotationAxis(new THREE.Vector3(1, 0, 0), Math.PI / 2);
            this.tileContent.applyMatrix4(rotateX); // convert from GLTF Y-up to Z-up
            let b3dmData = await b3dm.load();
            loader.parse(b3dmData.glbData, this.resourcePath, (gltf) => {
                //Add the batchtable to the userData since gltLoader doesn't deal with it
                gltf.scene.children[0].userData = b3dmData.batchTableJson;
                
                gltf.scene.traverse(child => {
                  if (child instanceof THREE.Mesh) {
                    // some gltf has wrong bounding data, recompute here
                    child.geometry.computeBoundingBox();
                    child.geometry.computeBoundingSphere();
                    child.material.depthWrite = true; // necessary for Velsen dataset?
                  }
                });
                if (this.styleParams.color != null || this.styleParams.opacity != null) {
                  let color = new THREE.Color(this.styleParams.color);
                  gltf.scene.traverse(child => {
                    if (child instanceof THREE.Mesh) {
                      if (this.styleParams.color != null) 
                        child.material.color = color;
                      if (this.styleParams.opacity != null) {
                        child.material.opacity = this.styleParams.opacity;
                        child.material.transparent = this.styleParams.opacity < 1.0 ? true : false;
                      }
                    }
                  });
                }
                if (this.debugColor) {
                  gltf.scene.traverse(child => {
                    if (child instanceof THREE.Mesh) {
                      child.material.color = this.debugColor;
                    }
                  })
                }
                this.tileContent.add(gltf.scene);
              }, (error) => {
                throw new Error('error parsing gltf: ' + error);
              }
            );
          } catch (error) {
            console.error(error);
          }
          break;
        case 'pnts':
          try {
            let pnts = new PNTS(url);
            let pointData = await pnts.load();            
            let geometry = new THREE.BufferGeometry();
            geometry.setAttribute('position', new THREE.Float32BufferAttribute(pointData.points, 3));
            let material = new THREE.PointsMaterial();
            material.size = this.styleParams.pointsize != null ? this.styleParams.pointsize : 1.0;
            if (this.styleParams.color) {
              material.vertexColors = THREE.NoColors;
              material.color = new THREE.Color(this.styleParams.color);
              material.opacity = this.styleParams.opacity != null ? this.styleParams.opacity : 1.0;
            } else if (pointData.rgba) {
              geometry.setAttribute('color', new THREE.Float32BufferAttribute(pointData.rgba, 4));
              material.vertexColors = THREE.VertexColors;
            } else if (pointData.rgb) {
              geometry.setAttribute('color', new THREE.Float32BufferAttribute(pointData.rgb, 3));
              material.vertexColors = THREE.VertexColors;
            }
            this.tileContent.add(new THREE.Points( geometry, material ));
            if (pointData.rtc_center) {
              let c = pointData.rtc_center;
              this.tileContent.applyMatrix4(new THREE.Matrix4().makeTranslation(c[0], c[1], c[2]));
            }
            this.tileContent.add(new THREE.Points( geometry, material ));
          } catch (error) {
            console.error(error);
          }
          break;
        case 'i3dm':
          throw new Error('i3dm tiles not yet implemented');
          break;
        case 'cmpt':
          throw new Error('cmpt tiles not yet implemented');
          break;
        default:
          throw new Error('invalid tile type: ' + type);
      }
    }
  }
  unload(includeChildren) {
    this.unloadedTileContent = true;
    this.totalContent.remove(this.tileContent);

    //this.tileContent.visible = false;
    if (includeChildren) {
      this.unloadedChildContent = true;
      this.totalContent.remove(this.childContent);
      //this.childContent.visible = false;
    } else  {
      this.childContent.visible = true;
    }
    if (this.debugLine) {
      this.totalContent.remove(this.debugLine);
      this.unloadedDebugContent = true;
    }
    // TODO: should we also free up memory?
  }
  checkLoad(frustum, cameraPosition) {

    /*this.load();
    for (let i=0; i<this.children.length;i++) {
      this.children[i].checkLoad(frustum, cameraPosition);
    }
    return;
    */

    /*if (this.totalContent.parent.name === "world") {
      this.totalContent.parent.updateMatrixWorld();
    }*/
    let transformedBox = this.box.clone();
    transformedBox.applyMatrix4(this.totalContent.matrixWorld);
    
    // is this tile visible?
    if (!frustum.intersectsBox(transformedBox)) {
      this.unload(true);
      return;
    }
    
    let worldBox = this.box.clone().applyMatrix4(this.worldTransform);
    let dist = worldBox.distanceToPoint(cameraPosition);
    
    
    //let dist = transformedBox.distanceToPoint(cameraPosition);

    //console.log(`dist: ${dist}, geometricError: ${this.geometricError}`);
    // are we too far to render this tile?
    if (this.geometricError > 0.0 && dist > this.geometricError * 50.0) {
      //console.log(`${dist} > ${this.geometricError}`)
      this.unload(true);
      return;
    }
    
    // should we load this tile?
    if (this.refine == 'REPLACE' && dist < this.geometricError * 20.0) {
      this.unload(false);
    } else {
      if (this.content) {
        //console.log(`loading ${this.content.uri}`);
      } else {
        //console.log(`loading ${this.resourcePath}`);
      }
      this.load();
    }
    
    
    // should we load its children?
    for (let i=0; i<this.children.length; i++) {
      if (dist < this.geometricError * 20.0) {
        this.children[i].checkLoad(frustum, cameraPosition);
      } else {
        this.children[i].unload(true);
      }
    }

    /*
    // below code loads tiles based on screenspace instead of geometricError,
    // not sure yet which algorith is better so i'm leaving this code here for now
    let sw = this.box.min.clone().project(camera);
    let ne = this.box.max.clone().project(camera);      
    let x1 = sw.x, x2 = ne.x;
    let y1 = sw.y, y2 = ne.y;
    let tilespace = Math.sqrt((x2 - x1)*(x2 - x1) + (y2 - y1)*(y2 - y1)); // distance in screen space
    
    if (tilespace < 0.2) {
      this.unload();
    }
    // do nothing between 0.2 and 0.25 to avoid excessive tile loading/unloading
    else if (tilespace > 0.25) {
      this.load();
      this.children.forEach(child => {
        child.checkLoad(camera);
      });
    }*/
    
  }
}

class TileLoader {
  // This class contains the common code to load tile content, such as b3dm and pnts files.
  // It is not to be used directly. Instead, subclasses are used to implement specific 
  // content loaders for different tile types.
  constructor(url) {
    this.url = url;
    this.type = url.slice(-4);
    this.version = null;
    this.byteLength = null;
    this.featureTableJSON = null;
    this.featureTableBinary = null;
    this.batchTableJson = null;
    this.batchTableBinary = null;
    this.binaryData = null;
  }
  // TileLoader.load
  async load() {
    let response = await fetch(this.url)            
    if (!response.ok) {
      throw new Error(`HTTP ${response.status} - ${response.statusText}`);
    }
    let buffer = await response.arrayBuffer();
    let res = this.parseResponse(buffer);
    return res;
  }
  parseResponse(buffer) {
    let header = new Uint32Array(buffer.slice(0, 28));
    let decoder = new TextDecoder();
    let magic = decoder.decode(new Uint8Array(buffer.slice(0, 4)));
    if (magic != this.type) {
      throw new Error(`Invalid magic string, expected '${this.type}', got '${this.magic}'`);
    }
    this.version = header[1];
    this.byteLength = header[2];
    let featureTableJSONByteLength = header[3];
    let featureTableBinaryByteLength = header[4];
    let batchTableJsonByteLength = header[5];
    let batchTableBinaryByteLength = header[6];
    
    /*
    console.log('magic: ' + magic);
    console.log('version: ' + this.version);
    console.log('featureTableJSONByteLength: ' + featureTableJSONByteLength);
    console.log('featureTableBinaryByteLength: ' + featureTableBinaryByteLength);
    console.log('batchTableJsonByteLength: ' + batchTableJsonByteLength);
    console.log('batchTableBinaryByteLength: ' + batchTableBinaryByteLength);
    */
    
    let pos = 28; // header length
    if (featureTableJSONByteLength > 0) {
      this.featureTableJSON = JSON.parse(decoder.decode(new Uint8Array(buffer.slice(pos, pos+featureTableJSONByteLength))));
      pos += featureTableJSONByteLength;
    } else {
      this.featureTableJSON = {};
    }
    this.featureTableBinary = buffer.slice(pos, pos+featureTableBinaryByteLength);
    pos += featureTableBinaryByteLength;
    if (batchTableJsonByteLength > 0) {
      this.batchTableJson = JSON.parse(decoder.decode(new Uint8Array(buffer.slice(pos, pos+batchTableJsonByteLength))));
      pos += batchTableJsonByteLength;
    } else {
      this.batchTableJson = {};
    }
    this.batchTableBinary = buffer.slice(pos, pos+batchTableBinaryByteLength);
    pos += batchTableBinaryByteLength;
    this.binaryData = buffer.slice(pos);
    return this;
  }
}
  
class B3DM extends TileLoader {
  constructor(url) {
    super(url);
    this.glbData = null;
  }
  parseResponse(buffer) {
    super.parseResponse(buffer);
    this.glbData = this.binaryData;
    return this;
  }
}

class PNTS extends TileLoader{
  constructor(url) {
    super(url);
    this.points = new Float32Array();
    this.rgba = null;
    this.rgb = null;
  }
  parseResponse(buffer) {
    super.parseResponse(buffer);
    if (this.featureTableJSON.POINTS_LENGTH && this.featureTableJSON.POSITION) {
      let len = this.featureTableJSON.POINTS_LENGTH;
      let pos = this.featureTableJSON.POSITION.byteOffset;
      this.points = new Float32Array(this.featureTableBinary.slice(pos, pos + len * Float32Array.BYTES_PER_ELEMENT * 3));
      this.rtc_center = this.featureTableJSON.RTC_CENTER;
      if (this.featureTableJSON.RGBA) {
        pos = this.featureTableJSON.RGBA.byteOffset;
        let colorInts = new Uint8Array(this.featureTableBinary.slice(pos, pos + len * Uint8Array.BYTES_PER_ELEMENT * 4));
        let rgba = new Float32Array(colorInts.length);
        for (let i=0; i<colorInts.length; i++) {
          rgba[i] = colorInts[i] / 255.0;
        }
        this.rgba = rgba;
      } else if (this.featureTableJSON.RGB) {
        pos = this.featureTableJSON.RGB.byteOffset;
        let colorInts = new Uint8Array(this.featureTableBinary.slice(pos, pos + len * Uint8Array.BYTES_PER_ELEMENT * 3));
        let rgb = new Float32Array(colorInts.length);
        for (let i=0; i<colorInts.length; i++) {
          rgb[i] = colorInts[i] / 255.0;
        }
        this.rgb = rgb;
      } else if (this.featureTableJSON.RGB565) {
        console.error('RGB565 is currently not supported in pointcloud tiles.')
      }
    }
    return this;
  }
}
  
class Layer {
  constructor (params) {
    if (!params) throw new Error('parameters missing for mapbox 3D tiles layer');
    if (!params.id) throw new Error('id parameter missing for mapbox 3D tiles layer');
    if (!params.url) throw new Error('url parameter missing for mapbox 3D tiles layer');
    
    this.id = params.id,
    this.url = params.url;
    this.styleParams = {};
    if ('color' in params) this.styleParams.color = params.color;
    if ('opacity' in params) this.styleParams.opacity = params.opacity;
    if ('pointsize' in params) this.styleParams.pointsize = params.pointsize;

    this.loadStatus = 0;
    this.viewProjectionMatrix = null;
    
    this.type = 'custom';
    this.renderingMode = '3d';
  }
  LightsArray() {
    const arr = [];
    let directionalLight1 = new THREE.DirectionalLight(0xffffff);
    directionalLight1.position.set(0.5, 1, 0.5).normalize();
    let target = directionalLight1.target.position.set(100000000, 1000000000, 0).normalize();
    arr.push(directionalLight1);

    let directionalLight2 = new THREE.DirectionalLight(0xffffff);
    //directionalLight2.position.set(0, 70, 100).normalize();
    directionalLight2.position.set(0.3, 0.3, 1).normalize();
    arr.push(directionalLight2);

    //arr.push(new THREE.DirectionalLightHelper( directionalLight1, 500));
    //arr.push(new THREE.DirectionalLightHelper( directionalLight2, 500));     

          //this.scene.background = new THREE.Color( 0xaaaaaa );
          //this.scene.add( new THREE.DirectionalLight() );
          //this.scene.add( new THREE.HemisphereLight() );
    return arr;
  }
  loadVisibleTiles() {
    if (this.tileset.root) {
      this.tileset.root.checkLoad(this.cameraSync.frustum, this.cameraSync.cameraPosition);
    }
  }
  onAdd(map, gl) {
    this.map = map;
    const fov = 28;
    const aspect = map.getCanvas().width/map.getCanvas().height;
    const near = 0.000000000001;
    const far = Infinity;

    this.mapQueryRenderedFeatures = map.queryRenderedFeatures.bind(this.map);
    this.map.queryRenderedFeatures = this.queryRenderedFeatures.bind(this);
          
    this.camera = new THREE.PerspectiveCamera(fov, aspect, near, far);
    this.scene = new THREE.Scene();
    this.rootTransform = [1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1];
    let lightsarray = this.LightsArray();
    lightsarray.forEach(light=>{
      this.scene.add(light);
    });
    this.world = new THREE.Group();
    this.world.name = 'world';
    this.scene.add(this.world);

    this.renderer = new THREE.WebGLRenderer({
      alpha: true, 
      antialias: true, 
      canvas: map.getCanvas(),
      context: gl
    });
    this.renderer.shadowMap.enabled = true;
    this.renderer.autoClear = false;

    this.cameraSync = new CameraSync(this.map, this.camera, this.world);
    this.cameraSync.updateCallback = ()=>this.loadVisibleTiles();
    
    //raycaster for mouse events
    this.raycaster = new THREE.Raycaster();
    this.tileset = new TileSet();
    this.tileset.load(this.url, this.styleParams).then(()=>{
      if (this.tileset.root) {
        this.world.add(this.tileset.root.totalContent);
        this.world.updateMatrixWorld();
        this.loadStatus = 1;
        this.loadVisibleTiles();
      }
    }).catch(error=>{
      console.error(`${error} (${this.url})`);
    })
  }
  onRemove(map, gl) {
    // todo: (much) more cleanup?
    this.map.queryRenderedFeatures = this.mapQueryRenderedFeatures;
    this.cameraSync = null;
  }
  queryRenderedFeatures(geometry, options){
    let result = this.mapQueryRenderedFeatures(geometry, options);
    if (!this.map || !this.map.transform) {
      return result;
    }
    if (!(options && options.layers && !options.layers.includes(this.id))) {
      if (geometry && geometry.x && geometry.y) {     
        var mouse = new THREE.Vector2();
        
        // // scale mouse pixel position to a percentage of the screen's width and height
        mouse.x = ( geometry.x / this.map.transform.width ) * 2 - 1;
        mouse.y = 1 - ( geometry.y / this.map.transform.height ) * 2;

        this.raycaster.setFromCamera(mouse, this.camera);

        // calculate objects intersecting the picking ray
        let intersects = this.raycaster.intersectObjects(this.world.children, true);
        if (intersects.length) {
          let feature = {
            "type": "Feature",
            "properties" : {},
            "geometry" :{},
            "layer": {"id": this.id, "type": "custom 3d"},
            "source": this.url,
            "source-layer": null,
            "state": {}
          }
          let propertyIndex;
          let intersect = intersects[0];
          if (intersect.object && intersect.object.geometry && 
              intersect.object.geometry.attributes && 
              intersect.object.geometry.attributes._batchid) {
            let geometry = intersect.object.geometry;
            let vertexIdx = intersect.faceIndex;
            if (geometry.index) {
              // indexed BufferGeometry
              vertexIdx = geometry.index.array[intersect.faceIndex*3];
              propertyIndex = geometry.attributes._batchid.data.array[vertexIdx*7+6]
            } else {
              // un-indexed BufferGeometry
              propertyIndex = geometry.attributes._batchid.array[vertexIdx*3];
            }            
            let keys = Object.keys(intersect.object.parent.userData);
            if (keys.length) {
              for (let propertyName of keys) {
                feature.properties[propertyName] = intersect.object.parent.userData[propertyName][propertyIndex];
              }
            } else {
              feature.properties.batchId = propertyIndex;
            }
          } else {
            if (intersect.index != null) {
              feature.properties.index = intersect.index;
            } else {
              feature.properties.name = this.id;
            }
          }
          if (options.outline != false && (intersect.object !== this.outlinedObject || 
              (propertyIndex != null && propertyIndex !== this.outlinePropertyIndex) 
                || (propertyIndex == null && intersect.index !== this.outlineIndex))) {
            // update outline
            if (this.outlineMesh) {
              let parent = this.outlineMesh.parent;
              parent.remove(this.outlineMesh);
              this.outlineMesh = null;
            }
            this.outlinePropertyIndex = propertyIndex;
            this.outlineIndex = intersect.index;
            if (intersect.object instanceof THREE.Mesh) {
              this.outlinedObject = intersect.object;
              let outlineMaterial = new THREE.MeshBasicMaterial({color: options.outlineColor? options.outlineColor : 0xff0000, wireframe: true});
              let outlineMesh;
              if (intersect.object && 
                  intersect.object.geometry && 
                  intersect.object.geometry.attributes && 
                  intersect.object.geometry.attributes._batchid) {
                // create new geometry from faces that have same _batchid
                let geometry = intersect.object.geometry;
                if (geometry.index) {
                  let ip1 = geometry.index.array[intersect.faceIndex*3];
                  let idx = geometry.attributes._batchid.data.array[ip1*7+6];
                  let blockFaces = [];
                  for (let faceIndex = 0; faceIndex < geometry.index.array.length; faceIndex += 3) {
                    let p1 = geometry.index.array[faceIndex];
                    if (geometry.attributes._batchid.data.array[p1*7+6] === idx) {
                      let p2 = geometry.index.array[faceIndex+1];
                      if (geometry.attributes._batchid.data.array[p2*7+6] === idx) {
                        let p3 = geometry.index.array[faceIndex+2];
                        if (geometry.attributes._batchid.data.array[p3*7+6] === idx) {
                          blockFaces.push(faceIndex);
                        }
                      }
                    }
                  }  
                  let highLightGeometry = new THREE.Geometry(); 
                  for (let vertexCount = 0, face = 0; face < blockFaces.length; face++) {
                    let faceIndex = blockFaces[face];
                    let p1 = geometry.index.array[faceIndex];
                    let p2 = geometry.index.array[faceIndex+1];
                    let p3 = geometry.index.array[faceIndex+2];
                    let positions = geometry.attributes.position.data.array;
                    highLightGeometry.vertices.push(
                      new THREE.Vector3(positions[p1*7], positions[p1*7+1], positions[p1*7+2]),
                      new THREE.Vector3(positions[p2*7], positions[p2*7+1], positions[p2*7+2]),
                      new THREE.Vector3(positions[p3*7], positions[p3*7+1], positions[p3*7+2]),
                    )
                    highLightGeometry.faces.push(new THREE.Face3(vertexCount, vertexCount+1, vertexCount+2));
                    vertexCount += 3;
                  }
                  highLightGeometry.computeBoundingSphere();
                  outlineMesh = new THREE.Mesh(highLightGeometry, outlineMaterial);
                } else {
                  let ip1 = intersect.faceIndex*3;
                  let idx = geometry.attributes._batchid.array[ip1];
                  let blockFaces = [];
                  for (let faceIndex = 0; faceIndex < geometry.attributes._batchid.array.length; faceIndex += 3) {
                    let p1 = faceIndex;
                    if (geometry.attributes._batchid.array[p1] === idx) {
                      let p2 = faceIndex + 1;
                      if (geometry.attributes._batchid.array[p2] === idx) {
                        let p3 = faceIndex + 2;
                        if (geometry.attributes._batchid.array[p3] === idx) {
                          blockFaces.push(faceIndex);
                        }
                      }
                    }
                  }
                  let highLightGeometry = new THREE.Geometry(); 
                  for (let vertexCount = 0, face = 0; face < blockFaces.length; face++) {
                    let faceIndex = blockFaces[face] * 3;
                    let positions = geometry.attributes.position.array;
                    highLightGeometry.vertices.push(
                      new THREE.Vector3(positions[faceIndex], positions[faceIndex+1], positions[faceIndex+2]),
                      new THREE.Vector3(positions[faceIndex+3], positions[faceIndex+4], positions[faceIndex+5]),
                      new THREE.Vector3(positions[faceIndex+6], positions[faceIndex+7], positions[faceIndex+8]),
                    )
                    highLightGeometry.faces.push(new THREE.Face3(vertexCount, vertexCount+1, vertexCount+2));
                    vertexCount += 3;
                  }
                  highLightGeometry.computeBoundingSphere();   
                  outlineMesh = new THREE.Mesh(highLightGeometry, outlineMaterial);
                }
              } else {
                outlineMesh = new THREE.Mesh(this.outlinedObject.geometry, outlineMaterial);
              }
              outlineMesh.position.x = this.outlinedObject.position.x+0.1;
              outlineMesh.position.y = this.outlinedObject.position.y+0.1;
              outlineMesh.position.z = this.outlinedObject.position.z+0.1;
              outlineMesh.quaternion.copy(this.outlinedObject.quaternion);
              outlineMesh.scale.copy(this.outlinedObject.scale);
              outlineMesh.matrix.copy(this.outlinedObject.matrix);
              outlineMesh.raycast = () =>{};
              outlineMesh.name = "outline";
              outlineMesh.wireframe = true;
              this.outlinedObject.parent.add(outlineMesh);
              this.outlineMesh = outlineMesh;
            }
          }
          result.unshift(feature);
        } else {
          this.outlinedObject = null;
          if (this.outlineMesh) {
            let parent = this.outlineMesh.parent;
            parent.remove(this.outlineMesh);
            this.outlineMesh = null;
          }
        }
      }
    }
    return result;
  }
  _update() {
    this.renderer.state.reset();
    this.renderer.render (this.scene, this.camera);
    
    
    /*if (this.loadStatus == 1) { // first render after root tile is loaded
      this.loadStatus = 2;
      let frustum = new THREE.Frustum();
      frustum.setFromProjectionMatrix(new THREE.Matrix4().multiplyMatrices(this.camera.projectionMatrix, this.camera.matrixWorldInverse));
      if (this.tileset.root) {
        this.tileset.root.checkLoad(frustum, this.getCameraPosition());
      }
    }*/
  }
  update() {
    requestAnimationFrame(()=>this._update());
  }
  render(gl, viewProjectionMatrix) {
    this._update();
  }
}

class Mapbox3DTiles {
  
  static projectedUnitsPerMeter(latitude) {
    let c = ThreeboxConstants;
    return Math.abs( c.WORLD_SIZE / Math.cos( c.DEG2RAD * latitude ) / c.EARTH_CIRCUMFERENCE );
  }
  static projectToWorld(coords) {
    // Spherical mercator forward projection, re-scaling to WORLD_SIZE
    let c = ThreeboxConstants;
    var projected = [
        c.MERCATOR_A * c.DEG2RAD * coords[0] * c.PROJECTION_WORLD_SIZE,
        c.MERCATOR_A * Math.log(Math.tan((Math.PI*0.25) + (0.5 * c.DEG2RAD * coords[1]) )) * c.PROJECTION_WORLD_SIZE
    ];
 
    //z dimension, defaulting to 0 if not provided
    if (!coords[2]) {
      projected.push(0)
    } else {
        var pixelsPerMeter = projectedUnitsPerMeter(coords[1]);
        projected.push( coords[2] * pixelsPerMeter );
    }

    var result = new THREE.Vector3(projected[0], projected[1], projected[2]);

    return result;
  }
}

Mapbox3DTiles.Layer = Layer;