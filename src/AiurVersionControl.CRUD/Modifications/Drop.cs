using AiurVersionControl.Models;
using System;
using System.Linq;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Drop<T, D> : IModification<CollectionWorkSpace<T>>
    {
        public string PropertyName { get; set; }
        public D ExpectValue { get; set; }

        [Obsolete(error: true, message: "This message is only for Newtonsoft.Json")]
        public Drop() { }

        public Drop(string propertyName, D expectValue)
        {
            PropertyName = propertyName;
            ExpectValue = expectValue;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            var property = typeof(T).GetProperty(PropertyName);
            var toRemove = workspace.List.FirstOrDefault(t => property.GetValue(t, null)?.Equals(ExpectValue) ?? false);
            if (toRemove is not null)
            {
                workspace.List.Remove(toRemove);
            }
        }
    }
}
