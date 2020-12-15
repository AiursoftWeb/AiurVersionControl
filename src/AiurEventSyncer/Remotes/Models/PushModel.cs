using AiurEventSyncer.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Remotes.Models
{
    public class PushModel<T>
    {
        public List<Commit<T>> Commits { get; set; }
        public string Start { get; set; }
    }
}
