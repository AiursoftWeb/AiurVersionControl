using AiurStore.Abstracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiurStore.Providers.FileProvider
{
    public class FileStoreProvider : IStoreProvider
    {
        private string _path;

        public FileStoreProvider(string path)
        {
            _path = path;
        }

        public IEnumerable<string> GetAll()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
            return File.ReadLines(_path);
        }

        public void Add(string newItem)
        {
            using var fileSteam = File.AppendText(_path);
            fileSteam.WriteLine(newItem);
        }

        public void Clear()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        public void Insert(int index, string newItem)
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
            var txtLines = File.ReadAllLines(_path).ToList();
            txtLines.Insert(index, newItem);
            File.WriteAllLines(_path, txtLines);
        }
    }
}
