using System;
using ModelGenerator.Framework.Configuration;
using Spectre.Console;

namespace ModelGenerator.Framework.Progress
{
    public readonly struct RagStatus<T>
    {
        public Exception? Exception { get; }
        public string? Message { get; }
        public T Value { get; }

        public RagStatus(T value) => (Value, Message, Exception) = (value, null, null);
        public RagStatus(T value, string message) => (Value, Message, Exception) = (value, message, null);
        public RagStatus(T value, string message, Exception ex) => (Value, Message, Exception) = (value, message, ex);
        public RagStatus(T value, Exception ex) => (Value, Message, Exception) = (value, ex.Message, ex);

        public void Print(Verbosities verbosity)
        {
            if (Message != null)
            {
                AnsiConsole.WriteLine($"        {Message}");
            }

            switch (Exception)
            {
                case FrameworkException when verbosity == Verbosities.Verbose:
                    PrintFullException(Exception);
                    break;
                case FrameworkException:
                    AnsiConsole.WriteLine($"          {Exception.Message}");
                    break;
                case not null:
                    PrintFullException(Exception);
                    break;
                default:
                    break;
            }
        }

        private void PrintFullException(Exception exception)
        {
            var exceptionLines = exception.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var exceptionLine in exceptionLines)
            {
                AnsiConsole.WriteLine($"        {exceptionLine}");
            }
        }
    }
}