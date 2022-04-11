using static System.Console;
namespace LicenseGenerator;

public static class ConsoleHelpers
{
    public record Option<T>(string Shortcut, string Name, T Value);

    public record Option(string Shortcut, string Name, string Value) : Option<string>(Shortcut, Name, Value);
    
    private class ColourSwitcher : IDisposable
    {
        private readonly ConsoleColor? _originalForeground;
        private readonly ConsoleColor? _originalBackground;

        public ColourSwitcher(ConsoleColor? foreground, ConsoleColor? background)
        {
            if (foreground.HasValue)
            {
                _originalForeground = ForegroundColor;
                ForegroundColor = foreground.Value;
            }

            if (background.HasValue)
            {
                _originalBackground = BackgroundColor;
                BackgroundColor = background.Value;
            }
        }

        public void Dispose()
        {
            if (_originalForeground.HasValue)
            {
                ForegroundColor = _originalForeground.Value;
            }

            if (_originalBackground.HasValue)
            {
                BackgroundColor = _originalBackground.Value;
            }
        }
    }

    public static string Choose(string promptText, params string[] optionValues)
    {
        var options = optionValues
            .Select((s, i) => new Option((i+1).ToString(), s, s))
            .ToArray();
        return Choose(promptText, options).Value;
    }

    public static Option<T> Choose<T>(string promptText, params Option<T>[] options)
    {
        WriteLine(promptText + ":");
        for (int i = 0; i < options.Length; i++)
        {
            WriteLine($"\t[{options[i].Shortcut}] - {options[i].Name}");
        }

        Option<T>? selection = null;
        while (selection == null)
        {
            var selectedValue = Prompt("Choose an option");
            selection = options.SingleOrDefault(o => o.Shortcut.Equals(selectedValue, StringComparison.OrdinalIgnoreCase));
        }

        return selection;
    }

    public static bool Confirm(string promptText)
    {
        var value = Prompt(promptText + " [y/n]");
        return string.Equals("y", value, StringComparison.OrdinalIgnoreCase);
    }
    
    public static string Prompt(string promptText)
    {
        Write(promptText);
        Write(": ");
        return ReadLine()?.Trim() ?? string.Empty;
    }
    
    public static T Prompt<T>(string promptText, Func<string, T> converter)
    {
        try
        {
            return converter.Invoke(Prompt(promptText));
        }
        catch (Exception)
        {
            WriteLine("Invalid input.");
            return Prompt(promptText, converter);
        }
    }

    public static IDisposable SwitchColours(ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        return new ColourSwitcher(foreground, background);
    }

    public static void WrapAndIndent(string value, int indent = 2, int width = 32)
    {
        var range = value.AsMemory();
        for (int i = 0; i < value.Length; i += width)
        {
            Write(new string(' ', 2));
            if (i + width >= value.Length)
            {
                WriteLine(range.Slice(i));
            }
            else
            {
                WriteLine(range.Slice(i, width));
            }
        }
    }
}