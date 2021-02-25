using AiurVersionControl.Models;
using System;
using System.Linq;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Patch<T> : IModification<CollectionWorkSpace<T>>
    {
        private readonly string _searchPropertyName;
        private readonly object _expectValue;
        private readonly string _patchPropertyName;
        private readonly object _newValue;

        public Patch(
            string searchPropertyName, 
            object expectValue,
            string patchPropertyName,
            object newValue)
        {
            _searchPropertyName = searchPropertyName;
            _expectValue = expectValue;
            _patchPropertyName = patchPropertyName;
            _newValue = newValue;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            var property = typeof(T).GetProperty(_searchPropertyName);
            var patchProperty = typeof(T).GetProperty(_patchPropertyName);
            var toPatch = workspace.List.FirstOrDefault(t => property.GetValue(t, null)?.Equals(_expectValue) ?? false);
            if (toPatch is not null)
            {
                patchProperty.SetValue(toPatch, _newValue);
            }
        }
    }
}
