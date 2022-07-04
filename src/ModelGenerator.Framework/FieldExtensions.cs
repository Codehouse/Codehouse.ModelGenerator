using System;
using System.Linq;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework
{
    public static class FieldExtensions
    {
        public static Guid[] GetMultiReferenceValue(this Field? field)
        {
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return new Guid[0];
            }

            try
            {
                return field.Value.Split(new[] {'|', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(Guid.Parse)
                            .ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not parse multiref field value of field {field.Name}.", ex);
            }
        }
    }
}