using System;
using System.Collections.Generic;

namespace MMaster
{
    internal class FileID
    {
        public int ID { get; }

        public string Path { get; }

        public bool LoadedAsdependency { get; }

        public List<string> LibraryCallNames { get; set; } = new List<string>();

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }

        internal FileID(int id, string path, bool loadedAsdependency = false)
        {
            this.ID = id;
            this.Path = path;
            LoadedAsdependency = loadedAsdependency;
        }
    }
}