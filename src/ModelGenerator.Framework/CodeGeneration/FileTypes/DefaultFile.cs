using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration.FileTypes
{
    public class DefaultFile : IFileType
    {
        public GenerationContext Context { get; }
        public ModelFile Model { get; }
        public ScopedRagBuilder<string> ScopedRagBuilder { get; }

        public DefaultFile(GenerationContext context, ModelFile model, ScopedRagBuilder<string> scopedRagBuilder)
        {
            Context = context;
            Model = model;
            ScopedRagBuilder = scopedRagBuilder;
        }
    }
}