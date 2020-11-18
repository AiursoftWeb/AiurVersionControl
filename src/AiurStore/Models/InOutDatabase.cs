using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AiurStore.Models
{
    public abstract class InOutDatabase<T> : IEnumerable<T>
    {
        private readonly InOutDbOptions _options;
        public InOutDatabase()
        {
            _options = new InOutDbOptions();
            this.OnConfiguring(_options);
            if (_options.Provider == null)
            {
                throw new InvalidOperationException("No store service configured!");
            }
        }

        protected abstract void OnConfiguring(InOutDbOptions options);

        public void Add(T newObject)
        {
            _options.Provider.Add(JsonSerializer.Serialize(newObject));
        }

        public void Clear()
        {
            _options.Provider.Clear();
        }

        public void Insert(int index, T newObject)
        {
            _options.Provider.Insert(index, JsonSerializer.Serialize(newObject));
        }

        public void InsertAfter(Func<T, bool> predicate, T newObject)
        {
            if (GetInsertIndex(predicate, out int index))
            {
                Insert(index, newObject);
            }
        }

        public IEnumerable<T> After(Func<T, bool> func)
        {
            var yielding = false;
            foreach (var item in this)
            {
                if (yielding)
                {
                    yield return item;
                }
                if (func(item))
                {
                    yielding = true;
                }
            }
        }

        private bool GetInsertIndex(Func<T, bool> predicate, out int index)
        {
            index = 1;
            foreach (var item in this)
            {
                if (predicate(item))
                {
                    return true;
                }
                index++;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _options.Provider.GetAll().Select(t => JsonSerializer.Deserialize<T>(t)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
