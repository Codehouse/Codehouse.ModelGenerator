﻿using System.Collections.Immutable;

namespace ModelGenerator.Framework.FileScanning
{
    public record FileSet
    {
        public string Id { get; init; }
        public IImmutableList<string> Files { get; init; }
        public string ItemPath { get; init; }
        public string ModelPath { get; init; }
        public string Name { get; init; }
    }
}