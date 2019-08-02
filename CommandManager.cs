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
    internal static class CommandManager
    {
        private static readonly string _externalDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly CSharpCodeProvider provider = new CSharpCodeProvider();
        internal const string _externalExtension = "*.cs";
        internal const string _internalNamespace = "MMaster.Commands";

        internal static Dictionary<string, Type> _internalLibraryTypes { get; private set; }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> _internalCommandLibraries { get; private set; }

        internal static Dictionary<string, Type> _externalLibraryTypes { get; private set; }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> _externalCommandLibraries { get; private set; }

        internal static List<FileID> _listLoadedFiles { get; private set; } = new List<FileID>();

        static CommandManager()
        {
            CommandManager._internalLibraryTypes = new Dictionary<string, Type>((IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);
            CommandManager._internalCommandLibraries = new Dictionary<Type, Dictionary<string, MethodInfo>>();
            CommandManager._externalLibraryTypes = new Dictionary<string, Type>((IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);
            CommandManager._externalCommandLibraries = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        }

        internal static void LoadInternalCommands()
        {
            CFormat.WriteLine("[CommandManager] Loading internal commands...");


            foreach (Type library in ((IEnumerable<Type>)Assembly.GetExecutingAssembly().GetTypes()).Where<Type>(t =>
          {
              if (t.IsClass && t.GetCustomAttributes().Where(a => a.GetType() == typeof(MMasterLibrary)).Any())
                  return t.Namespace == _internalNamespace;
              return false;
          }).ToList<Type>())

            {
                string libraryCallName = library.GetCustomAttribute<MMasterLibrary>().CallName;
                if (String.IsNullOrEmpty(libraryCallName))
                {
                    libraryCallName = library.Name;
                }

                IEnumerable<MethodInfo> methodInfos = library.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                .Where<MethodInfo>((Func<MethodInfo, bool>)(m => ((IEnumerable<object>)m.GetCustomAttributes(false))
                                                                .Where<object>((Func<object, bool>)(a => a.GetType().Name.Contains(typeof(MMasterCommand).Name))).Any()));

                Dictionary<string, MethodInfo> dictionary = new Dictionary<string, MethodInfo>();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    string commandCallName = methodInfo.GetCustomAttribute<MMasterCommand>().CallName;
                    if (String.IsNullOrEmpty(commandCallName))
                    {
                        commandCallName = methodInfo.Name;
                    }

                    if (dictionary.ContainsKey(commandCallName))
                        CFormat.WriteLine(string.Format("[CommandManager] Could not load command named \"{0}.{1}\" because one already exists.", libraryCallName, commandCallName), ConsoleColor.Red);
                    else
                        dictionary.Add(commandCallName, methodInfo);
                }

                _internalLibraryTypes.Add(libraryCallName, library);
                _internalCommandLibraries.Add(library, dictionary);
            }

            CFormat.WriteLine("[CommandManager] Internal commands loaded.");
        }

        internal static void LoadExternalCommands()
        {
            CFormat.WriteLine("[CommandManager] Loading external commands in application's directory (and sub-directories)...", ConsoleColor.Gray);
            LoadDirectory("", false);
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
                    CFormat.WriteLine(string.Format("[CommandManager] Found referenced assembly in \"{0}\": \"{1}\"", fileName, match.Groups[1].Value), ConsoleColor.Gray);
                }
                catch (Exception ex)
                {
                    CFormat.WriteLine(string.Format("[CommandManager] Could not load referenced assembly in {0}: \"{1}\". Details: {2}", fileName, match.Captures[3].Value, ex.Message), ConsoleColor.Red);
                }
            }
        }

        internal static void LoadFile(string path, bool successMessage = true)
        {
            string fileName = Path.GetFileName(path);

            try
            {
                if (CommandManager._listLoadedFiles.Any<FileID>((Func<FileID, bool>)(x => x.Path == Path.Combine(Path.GetDirectoryName(path) == "" ? AppDomain.CurrentDomain.BaseDirectory : "", path))))
                {
                    CFormat.WriteLine("[CommandManager] Could not load file named \"" + fileName + "\" because it has already been loaded.", ConsoleColor.Red);
                }
                else
                {
                    FileID fileId = new FileID(CommandManager._listLoadedFiles.Count, path);

                    CFormat.WriteLine("[CommandManager] Loading \"" + fileName + "\"...", ConsoleColor.Gray);

                    string fileCode = "";
                    using (StreamReader streamReader = new StreamReader(path))
                        fileCode = streamReader.ReadToEnd();

                    CompilerParameters parameters = new CompilerParameters()
                    {
                        GenerateInMemory = true
                    };

                    parameters.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);
                    CommandManager.ReferenceAssemblies(ref parameters, fileCode, fileName);
                    CompilerResults compilerResults = CommandManager.provider.CompileAssemblyFromSource(parameters, fileCode);

                    if ((uint)compilerResults.Errors.Count > 0U)
                    {
                        CFormat.WriteLine("[CommandManager] Could not load file named \"" + fileName + "\":", ConsoleColor.Red);
                        foreach (CompilerError error in (CollectionBase)compilerResults.Errors)
                            CFormat.WriteLine(string.Format("[CommandManager] ({0},{1}): error {2}: {3}", error.Line, error.Column, error.ErrorNumber, error.ErrorText), ConsoleColor.Red);
                    }
                    else
                    {
                        foreach (Type library in ((IEnumerable<Type>)compilerResults.CompiledAssembly.GetTypes()).Where<Type>((Func<Type, bool>)(t => t.IsClass)).ToList<Type>())
                        {
                            string libraryCallName = library.GetCustomAttribute<MMasterLibrary>().CallName;
                            if (String.IsNullOrEmpty(libraryCallName))
                            {
                                libraryCallName = library.Name;
                            }

                            IEnumerable<MethodInfo> methodInfos = library.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                                            .Where<MethodInfo>((Func<MethodInfo, bool>)(m => ((IEnumerable<object>)m.GetCustomAttributes(false))
                                                                            .Where<object>((Func<object, bool>)(a => a.GetType().Name.Contains(typeof(MMasterCommand).Name))).Any()));

                            Dictionary<string, MethodInfo> dictionary = new Dictionary<string, MethodInfo>();

                            foreach (MethodInfo methodInfo in methodInfos)
                            {
                                string commandCallName = methodInfo.GetCustomAttribute<MMasterCommand>().CallName;
                                if (String.IsNullOrEmpty(commandCallName))
                                {
                                    commandCallName = methodInfo.Name;
                                }

                                if (dictionary.ContainsKey(commandCallName))
                                    CFormat.WriteLine(string.Format("[CommandManager] Could not load command named \"{0}.{1}\" because one already exists.", libraryCallName, commandCallName), ConsoleColor.Red);
                                else
                                    dictionary.Add(commandCallName, methodInfo);
                            }

                            _internalLibraryTypes.Add(libraryCallName, library);
                            _internalCommandLibraries.Add(library, dictionary);
                        }

                        CommandManager._listLoadedFiles.Add(fileId);
                        if (!successMessage)
                            return;
                        CFormat.WriteLine("[CommandManager] Loaded \"" + fileName + "\" successfully!", ConsoleColor.Gray);
                    }
                }
            }
            catch (Exception ex)
            {
                CFormat.WriteLine("[CommandManager] Could not load file named \"" + fileName + "\": " + ex.Message, ConsoleColor.Red);
            }
        }

        internal static void LoadDirectory(string initialPath, bool successMessage = true)
        {
            string path = initialPath;
            int count = CommandManager._externalCommandLibraries.Count;
            try
            {
                if (string.IsNullOrEmpty(path))
                    path = AppDomain.CurrentDomain.BaseDirectory;

                if (Directory.Exists(path))
                {
                    foreach (string file in Directory.GetFiles(CommandManager._externalDirectory, "*.cs"))
                        CommandManager.LoadFile(file, successMessage);

                    if (CommandManager._externalCommandLibraries.Count > count)
                    {
                        if (string.IsNullOrEmpty(initialPath))
                            CFormat.WriteLine("[CommandManager] External commands loaded.");
                        else
                            CFormat.WriteLine("[CommandManager] External commands in \"" + path + "\" directory successfully loaded!", ConsoleColor.Gray);
                    }
                    else
                        CFormat.WriteLine("[CommandManager] No external commands loaded.");

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
            CommandManager._externalLibraryTypes.Clear();
            CommandManager._externalCommandLibraries.Clear();
            CommandManager._listLoadedFiles.Clear();
        }

        internal static void UnloadFile(int id)
        {
            try
            {
                foreach (string type in CommandManager._listLoadedFiles[id].Types)
                {
                    foreach (string method in CommandManager._listLoadedFiles[id].Methods)
                        CommandManager._externalCommandLibraries[CommandManager._externalLibraryTypes[type]].Remove(method);
                    CommandManager._externalLibraryTypes.Remove(type);
                }
                CommandManager._listLoadedFiles.RemoveAll((Predicate<FileID>)(x => x.ID == id));
                CFormat.WriteLine("File unloaded successfully.", ConsoleColor.Gray);
            }
            catch (Exception ex)
            {
                CFormat.WriteLine("[CommandManager] Could not unload file \"" + CommandManager._listLoadedFiles[id].Path + "\". Details: " + ex.Message, ConsoleColor.Red);
            }
        }
    }
}