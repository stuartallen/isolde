public enum TextSpeed
{
    Slow,
    Normal,
    Fast,
}

public static class CLI_IO
{
    private static readonly int[] TextSpeeds = { 500, 100, 25 };
    private static readonly int CheckForTabEveryMs = 25;
    private static readonly Dictionary<string, ConsoleColor> TextColors = new()
    {
        { "Red", ConsoleColor.Red },
        { "Blue", ConsoleColor.Blue },
        { "Green", ConsoleColor.Green },
        { "Yellow", ConsoleColor.Yellow },
        { "White", ConsoleColor.White },
        { "Cyan", ConsoleColor.Cyan },
        { "Magenta", ConsoleColor.Magenta },
    };

    private static int currentTextSpeedIndex = 0;

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

    // Render one text block
    public static void RenderText(string prompt, bool awaitReturn = true)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        // Ready the console
        Clear();

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
    public static void RenderTextList(string[] prompts, bool awaitReturn = true)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        foreach (string prompt in prompts)
        {
            ArgumentNullException.ThrowIfNull(prompt);
            RenderText(prompt, awaitReturn);
        }
    }

    // Render a generic set of option menu, and return the 0-based index of the selected option
    public static int PresentOptionMenu(string prompt, string[] options)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        ArgumentNullException.ThrowIfNull(options);

        RenderText(prompt, false);
        return PresentOptionMenuHelper(options);
    }

    // Render a generic set of option menu, and return the 0-based index of the selected option
    public static int PresentOptionMenu(string[] prompts, string[] options)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        ArgumentNullException.ThrowIfNull(options);

        RenderTextList(prompts, false);
        return PresentOptionMenuHelper(options);
    }

    private static int PresentOptionMenuHelper(string[] options)
    {
        ArgumentNullException.ThrowIfNull(options);

        int selectedOption = 0;
        int lastLineLength = RenderOptions(options, selectedOption);

        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    selectedOption = Math.Max(0, selectedOption - 1);
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    selectedOption = Math.Min(options.Length - 1, selectedOption + 1);
                }
                else if (keyInfo.Key >= ConsoleKey.D1 && keyInfo.Key <= ConsoleKey.D9)
                {
                    int optionIndex = keyInfo.Key - ConsoleKey.D0;
                    if (optionIndex >= 1 && optionIndex <= options.Length)
                    {
                        selectedOption = optionIndex - 1;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(string.Empty);
                    break;
                }

                ClearLastLine(lastLineLength);
                lastLineLength = RenderOptions(options, selectedOption);
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

    private static int RenderOptions(string[] options, int selectedOption)
    {
        ArgumentNullException.ThrowIfNull(options);

        int lastLineLength = 0;

        for (int i = 0; i < options.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(options[i]);
            string word = string.Empty;
            string selectedOptionPrefix = i == selectedOption ? ">" : string.Empty;
            ConsoleColor color = defaultColor;
            if (options[i].Contains('_'))
            {
                // Split the option into a color and text, and insert at the beginning
                // This way the entire option is rendered as the option color

                // Also render a > if the option is selected
                string[] parts = options[i].Split('_', 2);
                if (TextColors.TryGetValue(parts[0], out ConsoleColor textColor))
                {
                    color = textColor;
                }

                word += $"{selectedOptionPrefix}({i + 1}) {parts[1]} ";
            }
            else
            {
                word += $"{selectedOptionPrefix}({i + 1}) {options[i]} ";
            }

            // Plus 1 accounts for space from RenderWord
            lastLineLength += word.Length + 1;
            RenderTextWithColor(word, color);
        }

        return lastLineLength;
    }

    // Render a single word
    private static void RenderWord(string word)
    {
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
        }
        else
        {
            RenderTextWithColor($"{word} ", defaultColor);
        }

        // Reset the color
        Console.ForegroundColor = defaultColor;
    }

    private static void RenderTextWithColor(string text, ConsoleColor color)
    {
        ArgumentNullException.ThrowIfNull(text);

        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = defaultColor;
    }
}