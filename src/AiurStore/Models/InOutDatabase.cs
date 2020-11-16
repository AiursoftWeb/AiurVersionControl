using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AiurStore.Models
{
    public abstract class InOutDatabase<T>
    {
        private readonly InOutDbOptions _options;
        public InOutDatabase()
        {
            _options = new InOutDbOptions();
            this.OnConfiguring(_options);
            if (_options.Provider == null)
            {
                throw new InvalidOperationException("No file store configured!");
            }
        }

        public IEnumerable<T> Query()
        {
            return _options.Provider.GetAll().Select(t => JsonSerializer.Deserialize<T>(t));
        }

        public void Insert(T newObject)
        {
            _options.Provider.Insert(JsonSerializer.Serialize(newObject));
        }

        public void Drop()
        {
            _options.Provider.Drop();
        }

        protected abstract void OnConfiguring(InOutDbOptions options);
    }
}
