using subtree;

namespace B3dm.Tileset;

public class Tile3DWithBounds
{
    public Tile3D Tile { get; set; }
    public BoundingBox3D BoundingBox { get; set; }

    public Tile3DWithBounds(Tile3D tile, BoundingBox3D boundingBox)
    {
        Tile = tile;
        BoundingBox = boundingBox;
    }
}
