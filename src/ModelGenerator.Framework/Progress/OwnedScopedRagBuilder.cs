using System;

namespace ModelGenerator.Framework.Progress
{
    public sealed class OwnedScopedRagBuilder<T> : ScopedRagBuilder<T>, IDisposable
    {
        private readonly RagBuilder<T> _owner;

        public OwnedScopedRagBuilder(RagBuilder<T> owner, T scope) : base(scope)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner.MergeBuilder(this);
        }
    }
}