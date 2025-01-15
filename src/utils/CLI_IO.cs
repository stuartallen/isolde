using Isolde;

public enum TextSpeed
{
    Slow,
    Normal,
    Fast,
}

public static class CLI_IO
{
    private static readonly int[] TextSpeeds = { 250, 100, 25 };
    private static readonly int CheckForTabEveryMs = 25;
    private static readonly Dictionary<string, ConsoleColor> TextColors = new()
    {
        { ValidColors.Red, ConsoleColor.Red },
        { ValidColors.Blue, ConsoleColor.Blue },
        { ValidColors.Green, ConsoleColor.Green },
        { ValidColors.Yellow, ConsoleColor.Yellow },
        { ValidColors.White, ConsoleColor.White },
        { ValidColors.Cyan, ConsoleColor.Cyan },
        { ValidColors.Magenta, ConsoleColor.Magenta },
        { ValidColors.DarkYellow, ConsoleColor.DarkYellow },
        { ValidColors.Gray, ConsoleColor.Gray },
    };

    private static int currentTextSpeedIndex = 1;

    private static ConsoleColor defaultColor = ConsoleColor.White;

    // Set the text speed
    public static void SetTextSpeed(int index)
    {
        currentTextSpeedIndex = index;
    }

    // Clear the console and reset the color
    public static void Clear()
    {
        Console.Clear();
        Console.ForegroundColor = defaultColor;
    }

    public static void RenderRooms(Room[,] rooms, Player player)
    {
        for (int y = 0; y < rooms.GetLength(1); y++)
        {
            for (int x = 0; x < rooms.GetLength(0); x++)
            {
                var room = rooms[x, y];
                string symbol = room.IsWall ? "██" :
                        room.HasMonster ? "👿" :
                        room.IsOpening ? "🚪" :
                        room.HasTreasure ? "💰" :
                        "  ";

                symbol = room.IsDiscovered ? symbol : "▒▒";
                symbol = player.X == x && player.Y == y ? "🦸" : symbol;

                Console.Write($"|{symbol}");
            }

            Console.Write("|\n");
        }

        // Add key
        Console.WriteLine("\nKey:");
        Console.WriteLine("🦸 - Player    👿 - Monster    💰 - Treasure    🚪 - Opening/Exit");
        Console.WriteLine("██ - Wall      ▒▒ - Undiscovered");
    }

    // Render one text block
    public static void RenderText(string prompt, bool awaitReturn = true, bool clear = true)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        // Ready the console
        if (clear)
        {
            Clear();
        }

        // Normalize line endings to Environment.NewLine
        prompt = prompt.Replace("\r\n", "\n").Replace("\r", "\n");

        int speedDelay = TextSpeeds[currentTextSpeedIndex];
        bool tabHasBeenPressed = false;

        string[] promptLines = prompt.Split('\n');

        // Render line by line
        foreach (string line in promptLines)
        {
            string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Render word by word
            foreach (string word in words)
            {
                RenderWord(word);

                if (!tabHasBeenPressed)
                {
                    for (int i = 0; i < speedDelay / CheckForTabEveryMs; i++)
                    {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Tab)
                            {
                                tabHasBeenPressed = true;
                            }
                        }

                        if (tabHasBeenPressed)
                        {
                            break;
                        }
                        else
                        {
                            Thread.Sleep(CheckForTabEveryMs);
                        }
                    }
                }
            }

            Console.WriteLine();
        }

        if (awaitReturn)
        {
            Console.WriteLine(">>>");
            Console.WriteLine("[Press Enter to continue]");

            bool hasPressedReturn = false;

            while (!hasPressedReturn)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        hasPressedReturn = true;
                    }
                }
            }
        }
    }

    // Render a whole list of text blocks
    public static void RenderTextList(string[] prompts, bool awaitReturn = true, bool clear = true)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        foreach (string prompt in prompts)
        {
            ArgumentNullException.ThrowIfNull(prompt);
            RenderText(prompt, awaitReturn, clear);
        }
    }

    // Render a generic set of option menu, and return the 0-based index of the selected option
    public static int PresentOptionMenu(string prompt, string[] options, bool clear = true, int[]? disabledOptions = null, int lastChosenOption = 0)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        ArgumentNullException.ThrowIfNull(options);

        RenderText(prompt, false, clear);
        return PresentOptionMenuHelper(options, lastChosenOption, disabledOptions);
    }

    // Render a generic set of option menu, and return the 0-based index of the selected option
    public static int PresentOptionMenu(string[] prompts, string[] options, bool clear = true, int[]? disabledOptions = null, int lastChosenOption = 0)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        ArgumentNullException.ThrowIfNull(options);

        RenderTextList(prompts, false, clear);
        return PresentOptionMenuHelper(options, lastChosenOption, disabledOptions);
    }

    private static int PresentOptionMenuHelper(string[] options, int lastChosenOption, int[]? disabledOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        disabledOptions ??= [];

        int selectedOption = lastChosenOption;
        int lastLineLength = RenderOptions(options, selectedOption, disabledOptions);

        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // left arrow or shift + tab
                if (keyInfo.Key == ConsoleKey.LeftArrow || (keyInfo.Key == ConsoleKey.Tab && keyInfo.Modifiers == ConsoleModifiers.Shift))
                {
                    selectedOption = Math.Max(0, selectedOption - 1);
                }

                // right arrow or tab
                else if (keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.Tab)
                {
                    selectedOption = Math.Min(options.Length - 1, selectedOption + 1);
                }

                // up arrow (go to first non-disabled option)
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedOption = Array.FindIndex(options, o => !disabledOptions.Contains(Array.IndexOf(options, o)));
                }

                // down arrow (go to last non-disabled option)
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedOption = Array.FindLastIndex(options, o => !disabledOptions.Contains(Array.IndexOf(options, o)));
                }

                // number keys
                else if (keyInfo.Key >= ConsoleKey.D1 && keyInfo.Key <= ConsoleKey.D9)
                {
                    int optionIndex = keyInfo.Key - ConsoleKey.D0;
                    if (optionIndex >= 1 && optionIndex <= options.Length)
                    {
                        selectedOption = optionIndex - 1;
                    }
                }

                // enter (exit if not disabled)
                else if (keyInfo.Key == ConsoleKey.Enter && !disabledOptions.Contains(selectedOption))
                {
                    Console.WriteLine(string.Empty);
                    break;
                }

                ClearLastLine(lastLineLength);
                lastLineLength = RenderOptions(options, selectedOption, disabledOptions);
            }
        }

        return selectedOption;
    }

    private static void ClearLastLine(int lastLineLength)
    {
        // Move back to the start of the line
        Console.Write(new string('\b', lastLineLength));

        // Fill the line with spaces
        Console.Write(new string(' ', lastLineLength));

        // Move back to the start of the line
        Console.Write(new string('\b', lastLineLength));
    }

    private static int RenderOptions(string[] options, int selectedOption, int[] disabledOptions)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(disabledOptions);

        int lastLineLength = 0;

        for (int i = 0; i < options.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(options[i]);

            string[] words = options[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int wordLength = 0;
            foreach (string word in words)
            {
                if (i == selectedOption && wordLength == 0)
                {
                    lastLineLength += 1;
                    if (disabledOptions.Contains(i))
                    {
                        RenderTextWithColor("·", ConsoleColor.Blue);
                    }
                    else
                    {
                        RenderTextWithColor(">", defaultColor);
                    }
                }

                if (disabledOptions.Contains(i))
                {
                    // Remove existing color if disabled
                    if (word.Contains('_'))
                    {
                        string[] parts = word.Split('_', 2);
                        RenderWord($"{ValidColors.BlueDelimiter}{parts[1]}");
                        wordLength += parts[1].Length + 1;
                    }
                    else
                    {
                        wordLength += RenderWord($"{ValidColors.BlueDelimiter}{word}");
                    }
                }
                else
                {
                    wordLength += RenderWord(word);
                }
            }

            lastLineLength += wordLength;
        }

        return lastLineLength;
    }

    // Render a single word
    private static int RenderWord(string word)
    {
        int length;
        ArgumentNullException.ThrowIfNull(word);

        // If the word fits the format "Color_Text"
        if (word.Contains('_') && !word.EndsWith('_'))
        {
            string[] parts = word.Split('_', 2);

            string color = parts[0];
            string text = parts[1];
            if (TextColors.TryGetValue(color, out ConsoleColor textColor))
            {
                RenderTextWithColor($"{text} ", textColor);
            }
            else
            {
                RenderTextWithColor($"{text} ", defaultColor);
            }

            length = text.Length + 1;
        }
        else
        {
            RenderTextWithColor($"{word} ", defaultColor);
            length = word.Length + 1;
        }

        return length;
    }

    private static void RenderTextWithColor(string text, ConsoleColor color)
    {
        ArgumentNullException.ThrowIfNull(text);

        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = defaultColor;
    }
}