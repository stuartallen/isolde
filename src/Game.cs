namespace Isolde;

public enum GameState
{
    Exposition,
    Dungeon,
    Success,
    Slain,
    Escape,
}

public class Game
{
    private readonly int starterItemsAmountToChoose = 3;
    private readonly string[] starterItems = [
        string.Join(" ", "Cleaver of Goblin Snot".Split(' ').Select(word => ValidColors.GreenDelimiter + word)),
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
        this.Dungeon = new Dungeon(this.Player);
    }

    public GameState State { get; private set; }

    public Player Player { get; private set; }

    public Dungeon Dungeon { get; private set; }

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

        this.State = GameState.Dungeon;
    }

    public void RunDungeon()
    {
        this.State = GameState.Dungeon;
        bool isFirstTurn = true;

        while (this.State == GameState.Dungeon)
        {
            CLI_IO.Clear();

            Room[,] rooms = this.Dungeon.GetRooms();
            var (x, y) = this.Player.GetPosition();
            this.Dungeon.DiscoverAdjacentRooms(x, y);

            CLI_IO.RenderRooms(rooms, this.Player);
            Console.WriteLine();

            this.Dungeon.RunRoom(
                this.Player,
                () => this.State = GameState.Success,
                () => this.State = GameState.Slain,
                () => this.State = GameState.Escape,
                isFirstTurn);

            isFirstTurn = false;
        }
    }

    public void RunEnding()
    {
        if (this.State == GameState.Success)
        {
            List<string> successText = ParseText.ParseFile("text/ending/success.txt");
            foreach (string dialogue in successText)
            {
                CLI_IO.RenderText(dialogue);
            }
        }
        else if (this.State == GameState.Slain)
        {
            List<string> slainText = ParseText.ParseFile("text/ending/slain.txt");
            foreach (string dialogue in slainText)
            {
                CLI_IO.RenderText(dialogue);
            }
        }
        else if (this.State == GameState.Escape)
        {
            List<string> escapeText = ParseText.ParseFile("text/ending/escape.txt");
            foreach (string dialogue in escapeText)
            {
                CLI_IO.RenderText(dialogue);
            }
        }
    }
}