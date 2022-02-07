using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ModelGenerator.Framework.Progress
{
    public class ScopedRagBuilder<T>
    {
        public bool CanPass => !HasFails && !HasWarns;
        public bool HasFails => !_fails.IsEmpty;
        public bool HasWarns => !_warns.IsEmpty;

        private readonly ConcurrentBag<RagStatus<T>> _fails = new();
        private readonly ConcurrentBag<RagStatus<T>> _passes = new();
        private readonly T _scope;
        private readonly ConcurrentBag<RagStatus<T>> _warns = new();

        public ScopedRagBuilder(T scope)
        {
            _scope = scope;
        }

        public void AddFail(string message)
        {
            AddFail(new RagStatus<T>(_scope, message));
        }

        public void AddFail(string message, Exception exception)
        {
            AddFail(new RagStatus<T>(_scope, message, exception));
        }

        public void AddPass()
        {
            if (HasFails || HasWarns)
            {
                throw new InvalidOperationException($"Cannot pass an item with {_fails.Count:N0} fails and {_warns.Count:N0} warnings.");
            }

            _passes.Add(new RagStatus<T>(_scope));
        }

        public void AddWarn(string message)
        {
            AddWarn(new RagStatus<T>(_scope, message));
        }

        public void AddWarn(string message, Exception exception)
        {
            AddWarn(new RagStatus<T>(_scope, message, exception));
        }

        public (IEnumerable<RagStatus<T>> fails, IEnumerable<RagStatus<T>> warns, IEnumerable<RagStatus<T>> passes) GetStatuses()
        {
            return (_fails, _warns, _passes);
        }

        private void AddFail(RagStatus<T> status)
        {
            if (!_passes.IsEmpty)
            {
                _passes.Clear();
            }

            _fails.Add(status);
        }

        private void AddWarn(RagStatus<T> status)
        {
            if (!_passes.IsEmpty)
            {
                _passes.Clear();
            }

            _warns.Add(status);
        }
    }
}