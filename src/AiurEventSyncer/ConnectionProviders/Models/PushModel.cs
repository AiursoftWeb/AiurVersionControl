﻿using AiurEventSyncer.Abstract;
using System.Collections.Generic;

namespace AiurEventSyncer.ConnectionProviders.Models
{
    public class PushModel<T>
    {
        public List<Commit<T>> Commits { get; set; }
        public string Start { get; set; }
    }
}
