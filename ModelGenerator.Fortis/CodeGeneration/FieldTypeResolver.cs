using System;
using System.Collections.Immutable;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FieldTypeResolver : IFieldTypeResolver
    {
        private readonly FortisSettings.FieldTypeMappingSettings _settings;
        private readonly IImmutableDictionary<string, string> _fieldTypeLookup;
        private readonly IImmutableDictionary<string, string> _fieldValueLookup;

        public FieldTypeResolver(FortisSettings settings)
        {
            _settings = settings.FieldTypeMappings;
            _fieldTypeLookup = MappingInverter.InvertMapping(_settings.ConcreteFieldTypes, StringComparer.OrdinalIgnoreCase);
            _fieldValueLookup = MappingInverter.InvertMapping(_settings.FieldValueMappings, StringComparer.OrdinalIgnoreCase);
        }
        
        // TODO: Resolve field types for rendering parameters
        public string GetFieldInterfaceType(TemplateField field)
        {
            return "I" + GetFieldConcreteType(field);
        }

        public string GetFieldConcreteType(TemplateField field)
        {
            if (_fieldTypeLookup.TryGetValue(field.FieldType, out string fieldType))
            {
                return fieldType;
            }

            return _settings.FallBackFieldType;
        }

        public string? GetFieldValueType(TemplateField field)
        {
            var concreteFieldType = GetFieldConcreteType(field);
            if (_fieldValueLookup.TryGetValue(concreteFieldType, out string fieldType))
            {
                return fieldType;
            }

            return null;
        }
    }
}