using AiurVersionControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Drop<T> : IModification<CollectionWorkSpace<T>>
    {
        public Predicate<T> Searcher { get; set; }

        public Drop(Predicate<T> searcher)
        {
            Searcher = searcher;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            workspace.List.Remove(workspace.List.Find(Searcher));
        }
    }
}
