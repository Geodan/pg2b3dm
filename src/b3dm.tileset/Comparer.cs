namespace B3dm.Tileset;

public static class Comparer
{
    public static bool IsSimilar(double first, double second)
    {
        var delta = 0.1;
        return (second > first - delta) && (second < first + delta);
    }

}
