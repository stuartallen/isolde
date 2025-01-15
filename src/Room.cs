namespace Isolde;

public class Room(int x, int y)
{
    public bool IsWall { get; set; } = true; // Default to wall, generation algorithm will carve paths

    public bool IsDiscovered { get; set; } = false;

    public bool HasMonster { get; set; } = false;

    public bool HasTreasure { get; set; } = false;

    public bool IsOpening { get; set; } = false;

    public int X { get; } = x;

    public int Y { get; } = y;
}