using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiurStore
{
    public class InOutDbOptions
    {
        public IStoreProvider Provider { get; set; }
        public void UseFileStore(string path)
        {
            Provider = new FileStoreProvider(path);
        }
    }

    public interface IStoreProvider
    {
        public IEnumerable<string> GetAll();
        public void Insert(string newItem);
        public void Drop();
    }

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

        public void Drop()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}