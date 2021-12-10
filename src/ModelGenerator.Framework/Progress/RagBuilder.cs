using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModelGenerator.Framework.Progress
{
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

        private ICollection<RagStatus<T>> GetCollection(ConcurrentBag<RagStatus<T>> collection)
        {
            return collection.OrderBy(s => s.Value).ToArray();
        }
    }
}