using AiurVersionControl.Models;
using System;
using System.Linq;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Patch<T, D1, D2> : IModification<CollectionWorkSpace<T>>
    {
        public string SearchPropertyName { get; set; }
        public D1 ExpectValue { get; set; }
        public string PatchPropertyName { get; set; }
        public D2 NewValue { get; set; }

        [Obsolete(error: true, message: "This message is only for Newtonsoft.Json")]
        public Patch() { }

        public Patch(
            string searchPropertyName,
            D1 expectValue,
            string patchPropertyName,
            D2 newValue)
        {
            SearchPropertyName = searchPropertyName;
            ExpectValue = expectValue;
            PatchPropertyName = patchPropertyName;
            NewValue = newValue;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            var property = typeof(T).GetProperty(SearchPropertyName);
            var patchProperty = typeof(T).GetProperty(PatchPropertyName);
            var toPatch = workspace.List.FirstOrDefault(t => property.GetValue(t, null)?.Equals(ExpectValue) ?? false);
            if (toPatch is not null)
            {
                patchProperty.SetValue(toPatch, NewValue);
            }
        }
    }
}
