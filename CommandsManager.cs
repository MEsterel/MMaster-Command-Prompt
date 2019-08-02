// Decompiled with JetBrains decompiler
// Type: MMaster.CommandsManager
// Assembly: MMaster, Version=1.0.0.0, Culture=neutral, PublicKeyToken=adf6cf49a94e58fe
// MVID: FAC110E1-835B-44B3-9078-25DCBA4F0789
// Assembly location: C:\Users\Matthieu\Documents\Programmes\MMaster\MMaster.exe

using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MMaster
{
  internal static class CommandsManager
  {
    private static string _externalDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private static CSharpCodeProvider provider = new CSharpCodeProvider();
    private const string _commandNamespace = "MMaster.Commands";
    internal const string _externalExtension = "*.cs";
    private const string _refPattern = "// REF \\\"(.+)\\\"";

    internal static Dictionary<string, Type> _internalLibraryTypes { get; private set; }

    internal static Dictionary<Type, Dictionary<string, MethodInfo>> _internalCommandLibraries { get; private set; }

    internal static Dictionary<string, Type> _externalLibraryTypes { get; private set; }

    internal static Dictionary<Type, Dictionary<string, MethodInfo>> _externalCommandLibraries { get; private set; }

    internal static List<FileID> _listLoadedFiles { get; private set; } = new List<FileID>();

    static CommandsManager()
    {
      CommandsManager._internalLibraryTypes = new Dictionary<string, Type>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      CommandsManager._internalCommandLibraries = new Dictionary<Type, Dictionary<string, MethodInfo>>();
      CommandsManager._externalLibraryTypes = new Dictionary<string, Type>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      CommandsManager._externalCommandLibraries = new Dictionary<Type, Dictionary<string, MethodInfo>>();
    }

    internal static void LoadInternalCommands()
    {
      CFormat.WriteLine("[CommandManager] Loading internal commands...", ConsoleColor.Gray);
      foreach (Type key in ((IEnumerable<Type>) Assembly.GetExecutingAssembly().GetTypes()).Where<Type>((Func<Type, bool>) (t =>
      {
        if (t.IsClass)
          return t.Namespace == "MMaster.Commands";
        return false;
      })).ToList<Type>())
      {
        IEnumerable<MethodInfo> methodInfos = ((IEnumerable<MethodInfo>) key.GetMethods(BindingFlags.Static | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => ((IEnumerable<object>) m.GetCustomAttributes(false)).Where<object>((Func<object, bool>) (a => a.GetType().Name.Contains(typeof (MMasterCommand).Name))).Count<object>() > 0));
        Dictionary<string, MethodInfo> dictionary = new Dictionary<string, MethodInfo>();
        foreach (MethodInfo methodInfo in methodInfos)
        {
          string name = methodInfo.Name;
          if (dictionary.ContainsKey(name))
            CFormat.WriteLine(string.Format("[CommandManager] Could not load command named \"{0}.{1}\" because one already exists.", (object) key.Name, (object) name), ConsoleColor.Red);
          else
            dictionary.Add(name, methodInfo);
        }
        CommandsManager._internalLibraryTypes.Add(key.Name, key);
        CommandsManager._internalCommandLibraries.Add(key, dictionary);
      }
      CFormat.WriteLine("[CommandManager] Internal commands loaded.", ConsoleColor.Gray);
    }

    internal static void LoadExternalCommands()
    {
      CFormat.WriteLine("[CommandManager] Loading external commands in application's directory...", ConsoleColor.Gray);
      CommandsManager.LoadDirectory("", false);
    }

    private static void ReferenceAssemblies(
      ref CompilerParameters parameters,
      string fileCode,
      string fileName)
    {
      foreach (Match match in new Regex("// REF \\\"(.+)\\\"").Matches(fileCode))
      {
        try
        {
          parameters.ReferencedAssemblies.Add(match.Groups[1].Value);
          CFormat.WriteLine(string.Format("[CommandManager] Found referenced assembly in \"{0}\": \"{1}\"", (object) fileName, (object) match.Groups[1].Value), ConsoleColor.Gray);
        }
        catch (Exception ex)
        {
          CFormat.WriteLine(string.Format("[CommandManager] Could not load referenced assembly in {0}: \"{1}\". Details: {2}", (object) fileName, (object) match.Captures[3].Value, (object) ex.Message), ConsoleColor.Red);
        }
      }
    }

    internal static void LoadFile(string path, bool successMessage = true)
    {
      try
      {
        if (CommandsManager._listLoadedFiles.Any<FileID>((Func<FileID, bool>) (x => x.Path == Path.Combine(Path.GetDirectoryName(path) == "" ? AppDomain.CurrentDomain.BaseDirectory : "", path))))
        {
          CFormat.WriteLine("[CommandManager] Could not load file named \"" + Path.GetFileName(path) + "\" because it has already been loaded.", ConsoleColor.Red);
        }
        else
        {
          FileID fileId = new FileID(CommandsManager._listLoadedFiles.Count, path);
          CFormat.WriteLine("[CommandManager] Loading \"" + Path.GetFileName(path) + "\"...", ConsoleColor.Gray);
          string fileCode = "";
          using (StreamReader streamReader = new StreamReader(path))
            fileCode = streamReader.ReadToEnd();
          CompilerParameters parameters = new CompilerParameters()
          {
            GenerateInMemory = true
          };
          parameters.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);
          CommandsManager.ReferenceAssemblies(ref parameters, fileCode, Path.GetFileName(path));
          CompilerResults compilerResults = CommandsManager.provider.CompileAssemblyFromSource(parameters, fileCode);
          if ((uint) compilerResults.Errors.Count > 0U)
          {
            CFormat.WriteLine("[CommandManager] Could not load file named \"" + Path.GetFileName(path) + "\":", ConsoleColor.Red);
            foreach (CompilerError error in (CollectionBase) compilerResults.Errors)
              CFormat.WriteLine(string.Format("[CommandManager] ({0},{1}): error {2}: {3}", (object) error.Line, (object) error.Column, (object) error.ErrorNumber, (object) error.ErrorText), ConsoleColor.Red);
          }
          else
          {
            foreach (Type key in ((IEnumerable<Type>) compilerResults.CompiledAssembly.GetTypes()).Where<Type>((Func<Type, bool>) (t => t.IsClass)).ToList<Type>())
            {
              IEnumerable<MethodInfo> methodInfos = ((IEnumerable<MethodInfo>) key.GetMethods(BindingFlags.Static | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => ((IEnumerable<object>) m.GetCustomAttributes(false)).Where<object>((Func<object, bool>) (a => a.GetType().Name.Contains(typeof (MMasterCommand).Name))).Count<object>() > 0));
              Dictionary<string, MethodInfo> dictionary = new Dictionary<string, MethodInfo>();
              foreach (MethodInfo methodInfo in methodInfos)
              {
                if (dictionary.ContainsKey(methodInfo.Name))
                {
                  CFormat.WriteLine(string.Format("[CommandManager] Could not load command named \"{0}.{1}\" because one already exists.", (object) key.Name, (object) methodInfo.Name), ConsoleColor.Red);
                }
                else
                {
                  dictionary.Add(methodInfo.Name, methodInfo);
                  fileId.Methods.Add(methodInfo.Name);
                }
              }
              CommandsManager._externalLibraryTypes.Add(key.Name, key);
              CommandsManager._externalCommandLibraries.Add(key, dictionary);
              fileId.Types.Add(key.Name);
            }
            CommandsManager._listLoadedFiles.Add(fileId);
            if (!successMessage)
              return;
            CFormat.WriteLine("[CommandManager] Loaded \"" + Path.GetFileName(path) + "\" successfully!", ConsoleColor.Gray);
          }
        }
      }
      catch (Exception ex)
      {
        CFormat.WriteLine("[CommandManager] Could not load file named \"" + Path.GetFileName(path) + "\": " + ex.Message, ConsoleColor.Red);
      }
    }

    internal static void LoadDirectory(string initialPath, bool successMessage = true)
    {
      string path = initialPath;
      int count = CommandsManager._externalCommandLibraries.Count;
      try
      {
        if (string.IsNullOrEmpty(path))
          path = AppDomain.CurrentDomain.BaseDirectory;
        if (Directory.Exists(path))
        {
          foreach (string file in Directory.GetFiles(CommandsManager._externalDirectory, "*.cs"))
            CommandsManager.LoadFile(file, successMessage);
          if (CommandsManager._externalCommandLibraries.Count > count)
          {
            if (string.IsNullOrEmpty(initialPath))
              CFormat.WriteLine("[CommandManager] External commands loaded.", ConsoleColor.Gray);
            else
              CFormat.WriteLine("[CommandManager] External commands in \"" + path + "\" directory successfully loaded!", ConsoleColor.Gray);
          }
          else
            CFormat.WriteLine("[CommandManager] No external commands loaded.", ConsoleColor.Gray);
        }
        else
          CFormat.WriteLine("[CommandManager] Could not find directory named \"" + Path.GetDirectoryName(path) + "\".", ConsoleColor.Red);
      }
      catch (Exception ex)
      {
        CFormat.WriteLine("[CommandManager] Could not load directory named \"" + Path.GetDirectoryName(path) + "\": " + ex.Message, ConsoleColor.Red);
      }
    }

    internal static void ClearExternalCommands()
    {
      CommandsManager._externalLibraryTypes.Clear();
      CommandsManager._externalCommandLibraries.Clear();
      CommandsManager._listLoadedFiles.Clear();
    }

    internal static void UnloadFile(int id)
    {
      try
      {
        foreach (string type in CommandsManager._listLoadedFiles[id].Types)
        {
          foreach (string method in CommandsManager._listLoadedFiles[id].Methods)
            CommandsManager._externalCommandLibraries[CommandsManager._externalLibraryTypes[type]].Remove(method);
          CommandsManager._externalLibraryTypes.Remove(type);
        }
        CommandsManager._listLoadedFiles.RemoveAll((Predicate<FileID>) (x => x.ID == id));
        CFormat.WriteLine("File unloaded successfully.", ConsoleColor.Gray);
      }
      catch (Exception ex)
      {
        CFormat.WriteLine("[CommandManager] Could not unload file \"" + CommandsManager._listLoadedFiles[id].Path + "\". Details: " + ex.Message, ConsoleColor.Red);
      }
    }
  }
}
