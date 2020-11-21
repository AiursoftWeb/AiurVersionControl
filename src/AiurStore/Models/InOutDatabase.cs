using AiurStore.Abstracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AiurStore.Models
{
    public abstract class InOutDatabase<T> : IEnumerable<T>
    {
        public IStoreProvider Provider { get; set; }
        private object _obj = new object();
        public InOutDatabase()
        {
            var options = new InOutDbOptions();
            this.OnConfiguring(options);
            Provider = options.Provider;
        }

        protected abstract void OnConfiguring(InOutDbOptions options);

        public void Add(T newObject)
        {
            lock (_obj)
            {
                Provider.Add(JsonSerializer.Serialize(newObject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
        }

        public void Clear()
        {
            lock (_obj)
            {
                Provider.Clear();
            }
        }

        public void Insert(int index, T newObject)
        {
            lock (_obj)
            {
                Provider.Insert(index, JsonSerializer.Serialize(newObject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
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
            lock (_obj)
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
            return Provider.GetAll().Select(t => JsonSerializer.Deserialize<T>(t, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
