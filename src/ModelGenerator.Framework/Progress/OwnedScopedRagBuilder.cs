using System;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// A version of the <see cref="ScopedRagBuilder{T}"/> that, upon
    /// disposal, will automatically merge back into the owner
    /// <see cref="RagBuilder{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of scope</typeparam>
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