CREATE SCHEMA IF NOT EXISTS citydb;

CREATE TABLE IF NOT EXISTS citydb.geometry_data
(
    id BIGINT PRIMARY KEY,
    geometry geometry(GeometryZ, 4326),
    geometry_properties jsonb
);

CREATE TABLE IF NOT EXISTS citydb.surface_data
(
    id BIGINT PRIMARY KEY,
    tex_image_id BIGINT
);

CREATE TABLE IF NOT EXISTS citydb.tex_image
(
    id BIGINT PRIMARY KEY,
    image_uri TEXT,
    mime_type TEXT,
    image_data BYTEA
);

CREATE TABLE IF NOT EXISTS citydb.surface_data_mapping
(
    id BIGSERIAL PRIMARY KEY,
    geometry_data_id BIGINT,
    surface_data_id BIGINT,
    texture_mapping jsonb
);

INSERT INTO citydb.geometry_data (id, geometry, geometry_properties)
VALUES
    (
        1,
        'SRID=4326;POLYGON Z ((0 0 0, 1 0 0, 0 1 0, 0 0 0))'::geometry,
        '{"type": 6, "children": [{"type": 3, "objectId": "surface_1", "geometryIndex": 0}]}'::jsonb
    ),
    (
        2,
        'SRID=4326;POLYGON Z ((10 10 0, 11 10 0, 10 11 0, 10 10 0))'::geometry,
        '{"type": 6, "children": [{"type": 3, "objectId": "surface_2", "geometryIndex": 0}]}'::jsonb
    ),
    (
        3,
        'SRID=4326;MULTIPOLYGON Z (((20 20 0,20 21 0,21 21 0,21 20 0,20 20 0)),((22 20 0,22 21 0,23 21 0,23 20 0,22 20 0)))'::geometry,
        '{"type": 6, "children": [{"type": 3, "objectId": "surface_3a", "geometryIndex": 0}, {"type": 3, "objectId": "surface_3b", "geometryIndex": 1}]}'::jsonb
    )
ON CONFLICT (id) DO NOTHING;

INSERT INTO citydb.tex_image (id, image_uri, mime_type, image_data)
VALUES
    (
        1,
        'tiny.png',
        'image/png',
        decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=', 'base64')
    ),
    (
        2,
        'tiny2.png',
        'image/png',
        decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=', 'base64')
    )
ON CONFLICT (id) DO NOTHING;

INSERT INTO citydb.surface_data (id, tex_image_id)
VALUES
    (1, 1),
    (2, 1),
    (3, 2)
ON CONFLICT (id) DO NOTHING;

INSERT INTO citydb.surface_data_mapping (geometry_data_id, surface_data_id, texture_mapping)
VALUES
    (
        1,
        1,
        '{"surface_1":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}'::jsonb
    ),
    (
        3,
        2,
        '{"surface_3a":[[[0.0,0.0],[0.0,1.0],[1.0,1.0],[1.0,0.0],[0.0,0.0]]]}'::jsonb
    ),
    (
        3,
        3,
        '{"surface_3b":[[[0.0,0.0],[0.0,1.0],[1.0,1.0],[1.0,0.0],[0.0,0.0]]]}'::jsonb
    )
ON CONFLICT DO NOTHING;

CREATE INDEX IF NOT EXISTS citydb_geometry_data_centroid_idx ON citydb.geometry_data USING gist(st_centroid(st_envelope(geometry)));
