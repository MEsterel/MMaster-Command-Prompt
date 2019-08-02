// Decompiled with JetBrains decompiler
// Type: MMaster.FileID
// Assembly: MMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adf6cf49a94e58fe
// MVID: FAC110E1-835B-44B3-9078-25DCBA4F0789
// Assembly location: C:\Users\Matthieu\Documents\Programmes\MMaster\MMaster.exe

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
