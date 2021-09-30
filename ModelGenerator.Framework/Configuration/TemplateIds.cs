﻿using System;

namespace ModelGenerator.Framework.FileParsing
{
    public record TemplateIds : ITemplateIds
    {
        public Guid Template { get; init; }
        public Guid TemplateField { get; init; }
        public Guid TemplateSection { get; init; }
    }
}