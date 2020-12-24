using AiurStore.Abstracts;
using AiurStore.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiurStore.Providers.FileProvider
{
    public class FileStoreProvider<T> : IStoreProvider<T>
    {
        private string _path;

        public FileStoreProvider(string path)
        {
            _path = path;
        }

        public IEnumerable<T> GetAll()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
            return File.ReadLines(_path).Select(t => JsonTools.Deserialize<T>(t));
        }

        public void Add(T newItem)
        {
            using var fileSteam = File.AppendText(_path);
            fileSteam.WriteLine(JsonTools.Serialize(newItem));
        }

        public void Clear()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        public void Insert(int index, T newItem)
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
            var txtLines = File.ReadAllLines(_path).ToList();
            txtLines.Insert(index, JsonTools.Serialize(newItem));
            File.WriteAllLines(_path, txtLines);
        }
    }
}
