// Decompiled with JetBrains decompiler
// Type: MMaster.MMasterCommand
// Assembly: MMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adf6cf49a94e58fe
// MVID: FAC110E1-835B-44B3-9078-25DCBA4F0789
// Assembly location: C:\Users\Matthieu\Documents\Programmes\MMaster\MMaster.exe

using System;

namespace MMaster
{
  [AttributeUsage(AttributeTargets.Method)]
  public class MMasterCommand : Attribute
  {
    public readonly bool RequireAdminRights;
    public readonly string HelpPrompt;

    public MMasterCommand(string helpPrompt = "", bool requireAdminRights = false)
    {
      this.RequireAdminRights = requireAdminRights;
      this.HelpPrompt = helpPrompt;
    }
  }
}
