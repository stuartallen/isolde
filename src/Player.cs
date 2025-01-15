// The players name is always Isolde
public class Player
{
    public Player()
    {
        this.Health = 10;
        this.Inventory = [];
    }

    public int X { get; set; }

    public int Y { get; set; }

    public int Health { get; set; }

    public bool HasTreasure { get; private set; }

    public List<string> Inventory { get; private set; }

    public void AddItem(string item)
    {
        ArgumentNullException.ThrowIfNull(item);
        this.Inventory.Add(item);
        CLI_IO.RenderText($"Player obtained the {item} and added it to the inventory!");
    }

    public (int X, int Y) GetPosition()
    {
        return (this.X, this.Y);
    }

    public void SetPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    // For now the player can only obtain the treasure
    public void SetHasTreasure()
    {
        this.HasTreasure = true;
    }

    public void DecreaseHealth(int amount)
    {
        this.Health -= amount;
    }

    public List<string> GetItems()
    {
        return this.Inventory;
    }
}
