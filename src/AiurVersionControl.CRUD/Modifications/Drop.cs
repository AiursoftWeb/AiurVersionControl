using AiurVersionControl.Models;
using System.Linq;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Drop<T> : IModification<CollectionWorkSpace<T>>
    {
        private readonly string _propertyName;
        private readonly object _expectValue;

        public Drop(string propertyName, object expectValue)
        {
            _propertyName = propertyName;
            _expectValue = expectValue;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            var property = typeof(T).GetProperty(_propertyName);
            var toRemove = workspace.List.FirstOrDefault(t => property.GetValue(t, null)?.Equals(_expectValue) ?? false);
            if (toRemove is not null)
            {
                workspace.List.Remove(toRemove);
            }
        }
    }
}
