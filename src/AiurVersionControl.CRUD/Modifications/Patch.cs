using AiurVersionControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Patch<T> : IModification<CollectionWorkSpace<T>>
    {
        public Predicate<T> Searcher { get; set; }
        public Action<T> Action { get; set; }

        public Patch(Predicate<T> searcher, Action<T> action)
        {
            Searcher = searcher;
            Action = action;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            var found = workspace.List.Find(Searcher);
            if (found != null)
            {
                Action(found);
            }
        }
    }
}
