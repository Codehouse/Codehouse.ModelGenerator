using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FieldTypeResolver
    {
        private readonly IDictionary<string, string> _fieldParameterLookup;
        private readonly IDictionary<string, string> _fieldTypeLookup;
        private readonly IDictionary<string, string> _fieldValueLookup;
        private readonly FortisSettings.FieldTypeMappingSettings _settings;

        public FieldTypeResolver(FortisSettings settings)
        {
            _settings = settings.FieldTypeMappings;
            _fieldParameterLookup = _settings.FieldParameterMappings.ToImmutableDictionary();
            _fieldTypeLookup = _settings.ConcreteFieldTypes;
            _fieldValueLookup = _settings.FieldValueMappings;
        }

        public string GetFieldConcreteType(TemplateField field)
        {
            if (_fieldTypeLookup.TryGetValue(field.FieldType, out string fieldType))
            {
                return fieldType;
            }

            return _settings.FallBackFieldType;
        }

        public string GetFieldInterfaceType(TemplateField field)
        {
            return "I" + GetFieldConcreteType(field);
        }

        public string GetFieldParameterType(TemplateField field)
        {
            var concreteFieldType = GetFieldConcreteType(field);
            if (_fieldParameterLookup.TryGetValue(concreteFieldType, out string fieldType))
            {
                return fieldType;
            }

            throw new NotSupportedException($"Field type '{field.FieldType}' cannot be used in rendering parameters.");
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