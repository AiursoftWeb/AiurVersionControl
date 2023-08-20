using Aiursoft.AiurVersionControl.Models;
using System;

namespace Aiursoft.AiurVersionControl.CRUD.Modifications
{
    public class Add<T> : IModification<CollectionWorkSpace<T>>
    {
        public T Item { get; set; }

        [Obsolete(error: true, message: "This message is only for Newtonsoft.Json")]
        public Add() { }

        public Add(T item)
        {
            Item = item;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            workspace.List.Add(Item);
        }

        public override string ToString()
        {
            return $"Add a new {typeof(T).Name}";
        }
    }
}
