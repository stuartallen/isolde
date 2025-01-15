namespace Isolde;

public class Dungeon
{
    private const int SIZE = 10;
    private const int NumMonsters = 3;
    private readonly Room[,] rooms;
    private readonly Random random;

    private readonly List<string> monsters = [
        string.Join(" ", "Dragon".Split(' ').Select(word => ValidColors.RedDelimiter + word)),
        string.Join(" ", "Goblin".Split(' ').Select(word => ValidColors.GreenDelimiter + word)),
        string.Join(" ", "Rather Large Rabbit".Split(' ').Select(word => ValidColors.CyanDelimiter + word)),
    ];

    private int lastChosenDirection = 0;

    public Dungeon(Player player)
    {
        this.random = new Random();
        this.rooms = new Room[SIZE, SIZE];

        // Initialize rooms
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                this.rooms[x, y] = new Room(x, y);
            }
        }

        this.GenerateMaze();
        this.PlaceFeatures(player);
    }

    public Room[,] GetRooms()
    {
        return this.rooms;
    }

    // Public method to access a room
    public Room GetRoom(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds");
        }

        return this.rooms[x, y];
    }

    public void RunRoom(Player player, Action onWin, Action onSlain, Action onEscape, bool isFirstTurn)
    {
        var (x, y) = player.GetPosition();
        var currentRoom = this.GetRoom(x, y);

        // Check for special room features
        if (currentRoom.HasMonster)
        {
            bool hasBeenSlain = this.RunMonsterEvent(player, currentRoom);
            if (hasBeenSlain)
            {
                onSlain();
                return;
            }
        }

        if (currentRoom.HasTreasure)
        {
            this.RunTreasureEvent(player, currentRoom);
        }

        if (currentRoom.IsOpening && !isFirstTurn)
        {
            bool hasLeft = this.RunOpeningEvent(player, onWin, onEscape);

            // Skip direction handling if player has left the dungeon
            if (hasLeft)
            {
                return;
            }
        }

        this.HandlePlayerMovement(player, onSlain);
    }

    public void DiscoverAdjacentRooms(int x, int y)
    {
        var directions = new (int Dx, int Dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0), (-1, -1), (1, -1), (-1, 1), (1, 1) };

        foreach (var (dx, dy) in directions)
        {
            int newX = x + dx;
            int newY = y + dy;

            if (IsInBounds(newX, newY))
            {
                this.rooms[newX, newY].IsDiscovered = true;
            }
        }
    }

    private static bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < SIZE && y >= 0 && y < SIZE;
    }

    private static string GetHealthStatus(int health, bool isPlayer)
    {
        return health >= 8 ? $"{(isPlayer ? ValidColors.GreenDelimiter : ValidColors.RedDelimiter)}strong" :
                health >= 5 ? $"{ValidColors.RedDelimiter}weak" :
                $"{(isPlayer ? ValidColors.RedDelimiter : ValidColors.GreenDelimiter)}badly {(isPlayer ? ValidColors.RedDelimiter : ValidColors.GreenDelimiter)}hurt";
    }

    // Returns true if the player has left the dungeon
    private bool RunOpeningEvent(Player player, Action onWin, Action onEscape)
    {
        CLI_IO.Clear();
        string[] options = { "Leave the dungeon", "Stay and explore" };
        int choice = CLI_IO.PresentOptionMenu("You stand before the dungeon entrance. What will you do?", options, false);

        if (choice == 0)
        {
            if (player.HasTreasure)
            {
                CLI_IO.RenderText($"You leave the dungeon with the {ValidColors.RedDelimiter}Enchanted {ValidColors.RedDelimiter}Ruby in hand!");
                onWin();
            }
            else
            {
                CLI_IO.RenderText("You escape with your life, but without the treasure...");
                onEscape();
            }

            return true;
        }
        else
        {
            CLI_IO.RenderText("You decide to continue exploring the dungeon.");
            CLI_IO.Clear();
            CLI_IO.RenderRooms(this.rooms, player);

            return false;
        }
    }

    private void RunTreasureEvent(Player player, Room room)
    {
        CLI_IO.Clear();
        string[] options = { $"Take the {ValidColors.RedDelimiter}Enchanted {ValidColors.RedDelimiter}Ruby", "Leave it be" };
        int choice = CLI_IO.PresentOptionMenu($"The brilliant {ValidColors.RedDelimiter}Enchanted {ValidColors.RedDelimiter}Ruby lies before you. What do you do?", options, false);

        string turnClosingText =
            choice == 0 ? $"You carefully pocket the {ValidColors.RedDelimiter}Enchanted {ValidColors.RedDelimiter}Ruby." :
            $"You decide to leave the {ValidColors.RedDelimiter}Enchanted {ValidColors.RedDelimiter}Ruby where it lies.";

        if (choice == 0)
        {
            player.AddItem($"{ValidColors.RedDelimiter}Enchanted {ValidColors.RedDelimiter}Ruby");
            player.SetHasTreasure();
            room.HasTreasure = false;
        }

        CLI_IO.RenderText(turnClosingText);
        CLI_IO.Clear();
        CLI_IO.RenderRooms(this.rooms, player);
    }

    private void HandlePlayerMovement(Player player, Action onSlain)
    {
        var (x, y) = player.GetPosition();
        var validMoves = this.GetValidMoves(x, y);

        if (validMoves.Count == 0)
        {
            Console.WriteLine("You are trapped!");
            onSlain();
            return;
        }

        var disabledMovesIdx = validMoves.Where(m => m.Status == "disabled").Select(m => validMoves.IndexOf(m)).ToArray();

        // This was such a silly idea, why did I do the menu this way before lol
        // // Present only valid movement options
        // string[] options = validMoves.Select(m => m.Option).ToArray();
        // int choice = CLI_IO.PresentOptionMenu("Which direction would you like to move?", options, false, disabledMovesIdx, this.lastChosenDirection);

        // Update player position based on choice
        // var (dx, dy) = validMoves[choice].Direction;

        // // this.lastChosenDirection = choice;

        // player.SetPosition(x + dx, y + dy);

        // Wait for valid key press
        while (true)
        {
            var key = Console.ReadKey(true).Key;
            var direction = key switch
            {
                ConsoleKey.UpArrow => (0, -1),
                ConsoleKey.RightArrow => (1, 0),
                ConsoleKey.DownArrow => (0, 1),
                ConsoleKey.LeftArrow => (-1, 0),
                _ => (0, 0),
            };

            // Check if move is valid
            int newX = x + direction.Item1;
            int newY = y + direction.Item2;
            if (IsInBounds(newX, newY) && !this.rooms[newX, newY].IsWall)
            {
                player.SetPosition(newX, newY);
                break;
            }
        }
    }

    private List<(string Option, (int X, int Y) Direction, string Status)> GetValidMoves(int x, int y)
    {
        // Define all possible directions
        var allDirections = new[]
        {
            ("Up", (0, -1), "enabled"),
            ("Right", (1, 0), "enabled"),
            ("Down", (0, 1), "enabled"),
            ("Left", (-1, 0), "enabled"),
        };

        for (int i = 0; i < allDirections.Length; i++)
        {
            var (dx, dy) = allDirections[i].Item2;
            int newX = x + dx;
            int newY = y + dy;
            allDirections[i].Item3 = IsInBounds(newX, newY) && !this.rooms[newX, newY].IsWall ? "enabled" : "disabled";
        }

        return allDirections.ToList();
    }

    private void GenerateMaze()
    {
        // Start from a random position
        int startX = this.random.Next(SIZE);
        int startY = this.random.Next(SIZE);

        this.CarvePathFrom(startX, startY);
    }

    private void CarvePathFrom(int x, int y)
    {
        this.rooms[x, y].IsWall = false;

        // Define possible directions (up, right, down, left)
        var directions = new (int Dx, int Dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };

        // Shuffle directions
        directions = directions.OrderBy(_ => this.random.Next()).ToArray();

        foreach (var (dx, dy) in directions)
        {
            int newX = x + (dx * 2); // Move two steps to skip over walls
            int newY = y + (dy * 2);

            if (IsInBounds(newX, newY) && this.rooms[newX, newY].IsWall)
            {
                // Carve through the wall between current and new position
                this.rooms[x + dx, y + dy].IsWall = false;
                this.CarvePathFrom(newX, newY);
            }
        }
    }

    // Place features and set player position to the opening
    private void PlaceFeatures(Player player)
    {
        // Place opening (entrance/exit)
        var nonWallRooms = this.GetNonWallRooms().ToList();
        var openingRoom = nonWallRooms[this.random.Next(nonWallRooms.Count)];
        openingRoom.IsOpening = true;
        openingRoom.IsDiscovered = true;

        player.SetPosition(openingRoom.X, openingRoom.Y);

        // Discover adjacent rooms to opening
        this.DiscoverAdjacentRooms(openingRoom.X, openingRoom.Y);

        // Place treasure
        nonWallRooms = nonWallRooms.Where(r => !r.IsOpening).ToList();
        var treasureRoom = nonWallRooms[this.random.Next(nonWallRooms.Count)];
        treasureRoom.HasTreasure = true;

        // Place monsters
        nonWallRooms = nonWallRooms.Where(r => !r.HasTreasure).ToList();
        for (int i = 0; i < NumMonsters && nonWallRooms.Count > 0; i++)
        {
            int index = this.random.Next(nonWallRooms.Count);
            nonWallRooms[index].HasMonster = true;
            nonWallRooms.RemoveAt(index);
        }
    }

    private IEnumerable<Room> GetNonWallRooms()
    {
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                if (!this.rooms[x, y].IsWall)
                {
                    yield return this.rooms[x, y];
                }
            }
        }
    }

    private bool RunMonsterEvent(Player player, Room room)
    {
        // Choose a random monster
        string monster = this.monsters[this.random.Next(this.monsters.Count)];

        // Generate random health and get description
        int health = this.random.Next(5, 16); // 5 to 15
        string healthDesc = health > 12 ? $"{ValidColors.RedDelimiter}bulky" :
                            health >= 8 ? $"{ValidColors.YellowDelimiter}athletic" :
                            $"{ValidColors.GreenDelimiter}scrawny";

        // Generate random attack and get description
        int attack = this.random.Next(1, 7); // 1 to 6
        string attackDesc = attack >= 5 ? $"{ValidColors.RedDelimiter}aggressive" :
                            attack >= 3 ? $"{ValidColors.YellowDelimiter}lively" :
                            $"{ValidColors.GreenDelimiter}weak";

        // Display the monster encounter
        CLI_IO.Clear();
        CLI_IO.RenderText($"A {monster} blocks your path. It appears {healthDesc} ({health}) and {attackDesc} ({attack})!");

        while (health > 0 && player.Health > 0)
        {
            this.RunFight(player, monster, ref health, attack);
        }

        if (health <= 0)
        {
            CLI_IO.RenderText($"The {monster} is slain!");
            CLI_IO.RenderText($"Isolde finished the encounter looking {GetHealthStatus(player.Health, true)} ({player.Health}).");
        }
        else
        {
            CLI_IO.RenderText($"Isolde is slain!");
            CLI_IO.RenderText($"The {monster} finished the encounter looking {GetHealthStatus(health, false)} ({health}). It laughs as Isolde falls!");
            return true;
        }

        // Re-render the dungeon
        CLI_IO.Clear();
        CLI_IO.RenderRooms(this.rooms, player);
        Console.WriteLine();
        room.HasMonster = false;

        return false;
    }

    private void RunFight(Player player, string monster, ref int monsterHealth, int monsterAttack)
    {
        // Present combat options
        string[] options = { $"{ValidColors.RedDelimiter}Calculated {ValidColors.RedDelimiter}Attack", $"{ValidColors.GreenDelimiter}Wild {ValidColors.GreenDelimiter}Frenzy" };
        int choice = CLI_IO.PresentOptionMenu("How do you wish to attack?", options);

        // Player's turn
        this.ProcessPlayerAttack(player, monster, ref monsterHealth, choice);

        // Show monster status
        string monsterStatus = GetHealthStatus(monsterHealth, false);
        CLI_IO.RenderText($"The {monster} is looking {monsterStatus} ({monsterHealth}).");

        // Monster's turn if still alive
        if (monsterHealth > 0)
        {
            this.ProcessMonsterAttack(player, monster, monsterAttack);

            // Show player status
            string playerStatus = GetHealthStatus(player.Health, true);
            CLI_IO.RenderText($"Isolde is looking {playerStatus} ({player.Health}).");
        }
    }

    private void ProcessPlayerAttack(Player player, string monster, ref int monsterHealth, int attackChoice)
    {
        bool hit = this.random.Next(100) < 90; // 90% hit chance
        if (!hit)
        {
            CLI_IO.RenderText($"Isolde misses the {monster}!");
            return;
        }

        // Calculate damage based on attack type
        int damage;

        // Calculated attack
        if (attackChoice == 0)
        {
            damage = this.random.Next(1, 5) + this.random.Next(1, 5); // 2d4
        }

        // Wild frenzy
        else
        {
            damage = this.random.Next(1, 13); // 1d12
        }

        // Get random item for flavor
        string weapon;
        var items = player.GetItems();
        if (items.Count > 0)
        {
            weapon = items[this.random.Next(items.Count)];
        }
        else
        {
            weapon = $"{ValidColors.YellowDelimiter}bare {ValidColors.YellowDelimiter}hands"; // Fallback weapon
        }

        // Get damage description
        string hitDesc = damage > 10 ? $"{ValidColors.CyanDelimiter}legendary {ValidColors.CyanDelimiter}hit" :
                        damage > 7 ? $"{ValidColors.GreenDelimiter}crushing {ValidColors.GreenDelimiter}blow" :
                        damage > 4 ? $"{ValidColors.YellowDelimiter}solid {ValidColors.YellowDelimiter}strike" :
                        $"{ValidColors.RedDelimiter}glancing {ValidColors.RedDelimiter}hit";

        CLI_IO.RenderText($"Isolde strikes with her {weapon} scoring a {hitDesc} ({damage})!");
        monsterHealth -= damage;
    }

    private void ProcessMonsterAttack(Player player, string monster, int monsterAttack)
    {
        bool hit = this.random.Next(100) < 75; // 75% hit chance
        if (!hit)
        {
            CLI_IO.RenderText($"The {monster} misses Isolde!");
            return;
        }

        player.DecreaseHealth(monsterAttack);
        CLI_IO.RenderText($"The {monster} hits Isolde for {monsterAttack} damage!");
    }
}
