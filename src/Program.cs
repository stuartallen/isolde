using Isolde;

public class Program
{
    public static void Main(string[] args)
    {
        CLI_IO.Clear();

        string[] speeds = [
            $"{ValidColors.RedDelimiter}Slow",
            $"{ValidColors.BlueDelimiter}Normal",
            $"{ValidColors.GreenDelimiter}Fast"
        ];

        int selectedOption = CLI_IO.PresentOptionMenu(
            ["Select a text speed for your game\nKeep in mind, this text came at you at normal speed"],
            speeds);

        CLI_IO.SetTextSpeed(selectedOption);
        CLI_IO.RenderText($"You selected {speeds[selectedOption]}\nGreat choice!");
        CLI_IO.RenderText("Now, let's begin the Yellow_Adventure Yellow_of Yellow_Isolde!");
        Game game = new();

        game.RunExposition();
        game.RunDungeon();
        game.RunEnding();

        CLI_IO.Clear();
    }
}