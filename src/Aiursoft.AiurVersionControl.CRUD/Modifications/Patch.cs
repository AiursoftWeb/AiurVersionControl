using Aiursoft.AiurVersionControl.Models;

namespace Aiursoft.AiurVersionControl.CRUD.Modifications
{
    public class Patch<T, TSearch, TPatch> : IModification<CollectionWorkSpace<T>>
    {
        public string SearchPropertyName { get; set; }
        public TSearch ExpectValue { get; set; }
        public string PatchPropertyName { get; set; }
        public TPatch NewValue { get; set; }

        [Obsolete(error: true, message: "This message is only for Newtonsoft.Json")]
        public Patch() { }

        public Patch(
            string searchPropertyName,
            TSearch expectValue,
            string patchPropertyName,
            TPatch newValue)
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
            var toPatch = workspace.List.FirstOrDefault(t => property?.GetValue(t, null)?.Equals(ExpectValue) ?? false);
            if (toPatch is not null)
            {
                patchProperty?.SetValue(toPatch, NewValue);
            }
        }

        public override string ToString()
        {
            return $"Patch a {typeof(T).Name}";
        }
    }
}
