using System;
using System.Collections.Concurrent;
using System.Linq;
using AiurEventSyncer.Models;

namespace AiurVersionControl.Models
{
    public class RepositoryPool<T>
    {
        protected ConcurrentDictionary<string, T> Repos = null;
        
        public void CreatePool() 
        {
            lock (this)
            {
                if (Repos != null) {
                    return;
                }
                
                Repos = new ConcurrentDictionary<string, T>();
            }
        }

        public void AddRepository(string id, T repo)
        {
            lock (this)
            {
                if (Repos == null)
                {
                    throw new InvalidOperationException("Pool hasn't been created.");
                }

                Repos.TryAdd(id, repo);
            }
        }

        public bool RemoveRepository(string id)
        {
            lock (this)
            {
                if (Repos == null)
                {
                    throw new InvalidOperationException("Pool hasn't been created.");
                }

                return Repos.TryRemove(id, out _);
            }
        }

        public T GetRepository(string id)
        {
            lock (this)
            {
                if (Repos == null)
                {
                    throw new InvalidOperationException("Pool hasn't been created.");
                }
                
                return Repos.FirstOrDefault(x => x.Key == id).Value;
            }
        }

        public void ClosePool()
        {
            lock (this)
            {
                if (Repos == null)
                {
                    throw new InvalidOperationException("Pool hasn't been created.");
                }
                
                Repos.Clear();
                Repos = null;
            }
        }
    }
}