using AiurStore.Models;
using System.Collections.Generic;
using AiurStore.Tools;
using System.IO;
using System.Linq;

namespace AiurStore.Providers
{
    public class FileAiurStoreDb<T> : InOutDatabase<T>
    {
        private readonly string _path;

        public FileAiurStoreDb(string path)
        {
            _path = path;

        }

        public override IEnumerable<T> GetAll()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
            return File.ReadLines(_path).Select(t => JsonTools.Deserialize<T>(t));
        }

        public override void Add(T newItem)
        {
            using var fileSteam = File.AppendText(_path);
            fileSteam.WriteLine(JsonTools.Serialize(newItem));
        }

        public override void Clear()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        public override void Insert(int index, T newItem)
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
