using static System.Console;
namespace LicenseGenerator;

public static class ConsoleHelpers
{
    public record Option(string Shortcut, string Name, string Value);
    
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

    public static Option Choose(string promptText, params Option[] options)
    {
        WriteLine(promptText + ":");
        for (int i = 0; i < options.Length; i++)
        {
            WriteLine($"\t[{options[i].Shortcut}] - {options[i].Name}");
        }

        Option? selection = null;
        while (selection == null)
        {
            var selectedValue = Prompt("Choose an option");
            selection = options.SingleOrDefault(o => o.Shortcut.Equals(selectedValue, StringComparison.OrdinalIgnoreCase));
        }

        return selection;
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
}