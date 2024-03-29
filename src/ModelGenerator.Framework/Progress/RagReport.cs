﻿using System.Collections.Generic;
using System.Linq;
using ModelGenerator.Framework.Configuration;
using Spectre.Console;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// A RAG (red-amber-green) report, which can be generated from a
    /// <see cref="RagBuilder{T}"/>.
    /// </summary>
    /// <typeparam name="TResult">The result of the activity</typeparam>
    /// <typeparam name="TItem">The type of the scopes</typeparam>
    public class RagReport<TResult, TItem> : IReport<TResult>
    {
        public ICollection<RagStatus<TItem>> Fails { get; }
        public string Name { get; }
        public ICollection<RagStatus<TItem>> Passes { get; }
        public TResult Result { get; }
        public ICollection<RagStatus<TItem>> Warns { get; }

        public RagReport(string name, RagBuilder<TItem> builder, TResult result)
        {
            Passes = builder.GetPasses();
            Warns = builder.GetWarns();
            Fails = builder.GetFails();

            Name = name;
            Result = result;
        }

        public void Print(Verbosities verbosity)
        {
            AnsiConsole.MarkupLine($"[underline]{Name} results:[/]");

            PrintGroup("Pass", "green", Passes, verbosity, Verbosities.VeryVerbose);
            PrintGroup("Warn", "olive", Warns, verbosity, Verbosities.Verbose);
            PrintGroup("Fail", "maroon", Fails, verbosity, Verbosities.Normal);

            AnsiConsole.WriteLine();
        }

        private void PrintGroup(string title, string colour, ICollection<RagStatus<TItem>> statusCollection, Verbosities verbosity, Verbosities itemisationThreshold)
        {
            var valueGroups = statusCollection.GroupBy(f => f.Value).ToArray();
            AnsiConsole.MarkupLine($"  {title}: [{colour}]{valueGroups.Length:N0}[/]");

            if (verbosity >= itemisationThreshold)
            {
                foreach (var valueGroup in valueGroups)
                {
                    PrintGroupItems(valueGroup, verbosity);
                }
            }
        }

        private void PrintGroupItems(IGrouping<TItem, RagStatus<TItem>> valueGroup, Verbosities verbosity)
        {
            AnsiConsole.MarkupLine($"    [underline]{valueGroup.Key}[/]");
            foreach (var value in valueGroup)
            {
                value.Print(verbosity);
            }
        }
    }
}