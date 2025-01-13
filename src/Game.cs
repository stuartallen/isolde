public enum GameState
{
    Exposition,
    Dungeon,
    Success,
    Failure,
}

public class Game
{
    private readonly int starterItemsAmountToChoose = 3;
    private readonly string[] starterItems = [
        string.Join(" ", "Mysterious Health Potion".Split(' ').Select(word => ValidColors.GreenDelimiter + word)),
        string.Join(" ", "Sword of Might".Split(' ').Select(word => ValidColors.RedDelimiter + word)),
        string.Join(" ", "Old Boot".Split(' ').Select(word => ValidColors.DarkYellowDelimiter + word)),
        string.Join(" ", "Tears of a Great Warrior".Split(' ').Select(word => ValidColors.CyanDelimiter + word)),
        string.Join(" ", "Painted Duck".Split(' ').Select(word => ValidColors.YellowDelimiter + word)),
        string.Join(" ", "Iron Shield".Split(' ').Select(word => ValidColors.GrayDelimiter + word)),
        string.Join(" ", "Very Neat Yo-Yo".Split(' ').Select(word => ValidColors.MagentaDelimiter + word)),
    ];

    public Game()
    {
        this.State = GameState.Exposition;
        this.Player = new Player();
    }

    public GameState State { get; private set; }

    public Player Player { get; private set; }

    public void RunExposition()
    {
        this.State = GameState.Exposition;

        List<string> expositionText = ParseText.ParseFile("text/exposition/introduction.txt");
        foreach (string dialogue in expositionText)
        {
            CLI_IO.RenderText(dialogue);
        }

        string[] mutableStartItems = this.starterItems;
        for (int i = 0; i < this.starterItemsAmountToChoose; i++)
        {
            int playerChoiceIdx = CLI_IO.PresentOptionMenu($"You may choose from the following {mutableStartItems.Length} items:", mutableStartItems);
            this.Player.AddItem(mutableStartItems[playerChoiceIdx]);
            mutableStartItems = mutableStartItems.Where((_, idx) => idx != playerChoiceIdx).ToArray();
        }

        List<string> callToActionText = ParseText.ParseFile("text/exposition/callToAction.txt");
        foreach (string dialogue in callToActionText)
        {
            CLI_IO.RenderText(dialogue);
        }
    }
}