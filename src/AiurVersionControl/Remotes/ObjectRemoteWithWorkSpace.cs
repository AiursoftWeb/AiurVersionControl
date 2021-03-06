﻿using AiurEventSyncer.ConnectionProviders;
using AiurVersionControl.Models;

namespace AiurVersionControl.Remotes
{
    public class ObjectRemoteWithWorkSpace<T> : RemoteWithWorkSpace<T> where T : WorkSpace, new()
    {
        public ObjectRemoteWithWorkSpace(ControlledRepository<T> localRepository, bool autoPush = false, bool autoPull = false)
            : base(new FakeConnection<IModification<T>>(localRepository), autoPush, autoPull)
        {

        }
    }
}
