﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// A RAG (red-amber-green) builder, aligning to failures, warnings
    /// and passes.
    /// <para>Can be used to provide information for playback to users, in
    /// particular warnings if something wasn't right, or failures if something
    /// went wrong</para>
    /// </summary>
    /// <typeparam name="T">Scope type</typeparam>
    public class RagBuilder<T>
    {
        private readonly ConcurrentBag<RagStatus<T>> _fails = new();
        private readonly ConcurrentBag<RagStatus<T>> _passes = new();
        private readonly ConcurrentBag<RagStatus<T>> _warns = new();

        public void AddFail(RagStatus<T> value)
        {
            _fails.Add(value);
        }

        public void AddPass(T value)
        {
            _passes.Add(new RagStatus<T>(value));
        }

        public void AddWarn(RagStatus<T> value)
        {
            _warns.Add(value);
        }

        public OwnedScopedRagBuilder<T> CreateScope(T scope)
        {
            return new OwnedScopedRagBuilder<T>(this, scope);
        }

        public ICollection<RagStatus<T>> GetFails()
        {
            return GetCollection(_fails);
        }

        public ICollection<RagStatus<T>> GetPasses()
        {
            return GetCollection(_passes);
        }

        public ICollection<RagStatus<T>> GetWarns()
        {
            return GetCollection(_warns);
        }

        public void MergeBuilder(ScopedRagBuilder<T> builder)
        {
            var (fails, warns, passes) = builder.GetStatuses();
            AddRange(_fails, fails);
            AddRange(_warns, warns);
            AddRange(_passes, passes);
        }

        private void AddRange(ConcurrentBag<RagStatus<T>> target, IEnumerable<RagStatus<T>> range)
        {
            foreach (var item in range)
            {
                target.Add(item);
            }
        }

        private ICollection<RagStatus<T>> GetCollection(ConcurrentBag<RagStatus<T>> collection)
        {
            return collection.OrderBy(s => s.Value).ToArray();
        }
    }
}