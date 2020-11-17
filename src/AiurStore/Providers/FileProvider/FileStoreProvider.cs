using AiurStore.Abstracts;
using System.Collections.Generic;
using System.IO;

namespace AiurStore.Providers.FileProvider
{
    public class FileStoreProvider : IStoreProvider
    {
        private string path;

        public FileStoreProvider(string path)
        {
            this.path = path;
        }

        public IEnumerable<string> GetAll()
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            return File.ReadLines(path);
        }

        public void Insert(string newItem)
        {
            using var fileSteam = File.AppendText(path);
            fileSteam.WriteLine(newItem);
        }

        public void Clear()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
