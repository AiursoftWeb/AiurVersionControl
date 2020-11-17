﻿using AiurEventSyncer.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public Commit<T> LocalPointerPosition { get; set; }
        public string GetRemotePointerPositionId();
        IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition);
    }
}