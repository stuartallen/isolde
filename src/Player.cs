// The players name is always Isolde
public class Player
{
    public Player()
    {
        this.Health = 10;
        this.Inventory = [];
    }

    public int Health { get; set; }

    public List<string> Inventory { get; private set; }

    public void AddItem(string item)
    {
        ArgumentNullException.ThrowIfNull(item);
        this.Inventory.Add(item);
        CLI_IO.RenderText($"Player obtained the {item} and added it to the inventory!");
    }
}
