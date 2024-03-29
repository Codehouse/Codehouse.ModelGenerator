﻿using System;
using System.IO;

namespace ModelGenerator.Tds
{
    public class TdsSettings
    {
        public string Root
        {
            get => _root ?? Directory.GetCurrentDirectory();
            set => _root = string.IsNullOrEmpty(value) ? null : value;
        }

        public string[] Sources { get; set; } = Array.Empty<string>();

        private string? _root;
    }
}