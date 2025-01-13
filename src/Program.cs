public class Program
{
    public static void Main(string[] args)
    {
        CLI_IO.Clear();

        CLI_IO.RenderText("Hello World!, let's see these Blue_different speeds");

        int selectedOption = CLI_IO.PresentOptionMenu(["Select a speed\nIt'll help us later"],  ["Red_Slow", "Green_Normal", "Blue_Fast"]);

        CLI_IO.SetTextSpeed(selectedOption);
        CLI_IO.RenderText("You selected option " + (selectedOption + 1) + "\nGreat choice!");
    }
}