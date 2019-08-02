using System.Collections.Generic;

namespace MMaster
{
    internal class FileID
    {
        public int ID { get; private set; }

        public string Path { get; private set; }

        public List<string> Types { get; set; }

        public List<string> Methods { get; set; }

        internal FileID(int id, string path)
        {
            this.ID = id;
            this.Path = path;
            this.Types = new List<string>();
            this.Methods = new List<string>();
        }
    }
}