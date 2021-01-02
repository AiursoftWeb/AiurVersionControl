﻿using AiurStore.Models;
using AiurStore.Tools;
using System;
using System.Collections.Generic;

namespace AiurStore.Providers
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T> where T : class
    {
        private readonly LinkedList<T> _store = new LinkedList<T>();

        private LinkedListNode<T> SearchFromLast(Func<T, bool> prefix)
        {
            var start = _store.Last;
            while (start != null)
            {
                if (prefix(start.Value))
                {
                    return start;
                }
                start = start.Previous;
            }
            return null;
        }

        public override IEnumerable<T> GetAll()
        {
            return _store;
        }

        public override IEnumerable<T> GetAllAfter(Func<T, bool> prefix)
        {
            var node = SearchFromLast(prefix);
            if (node == null)
            {
                return _store;
            }
            else
            {
                return node.Next.YieldAfter();
            }
        }

        public override IEnumerable<T> GetAllAfter(T afterWhich)
        {
            if (afterWhich == null)
            {
                return _store;
            }
            else
            {
                var start = _store.FindLast(afterWhich);
                return start?.Next.YieldAfter();
            }
        }

        public override void Add(T newItem)
        {
            _store.AddLast(newItem);
        }

        public override void Clear()
        {
            _store.Clear();
        }

        public override void InsertAfter(T afterWhich, T newItem)
        {
            if (afterWhich == null)
            {
                _store.AddFirst(newItem);
            }
            else
            {
                var which = _store.FindLast(afterWhich);
                _store.AddAfter(which, newItem);
            }
        }
    }
}