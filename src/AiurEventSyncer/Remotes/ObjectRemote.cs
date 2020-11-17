using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _localRepository;

        public ObjectRemote(Repository<T> localRepository)
        {
            _localRepository = localRepository;
        }

        public Commit<T> LocalPointerPosition { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string sourcePointerPosition)
        {
            return _localRepository.Commits.AfterCommitId(sourcePointerPosition);
        }

        public string GetRemotePointerPositionId()
        {
            return _localRepository.Commits.Last().Id;
        }
    }

    public static class ListExtends
    {
        public static IEnumerable<Commit<T>> AfterCommitId<T>(this IEnumerable<Commit<T>> source, string sourcePointerPosition)
        {
            if (string.IsNullOrWhiteSpace(sourcePointerPosition))
            {
                return source;
            }
            return source.After(t => t.Id == sourcePointerPosition);
        }

        public static IEnumerable<T> After<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            bool yielding = false;
            foreach(var item in source)
            {
                if (yielding)
                {
                    yield return item;
                }
                if (func(item))
                {
                    yielding = true;
                }
            }
        }
    }
}
