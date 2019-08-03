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
        private static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();
        internal const string _ExternalExtension = "*.cs";
        internal const string _InternalNamespace = "MMaster.Commands";

        internal static Dictionary<string, Type> InternalLibraryCallNames { get; }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> InternalLibraries { get; }

        internal static Dictionary<string, Type> ExternalLibraryCallNames { get; }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> ExternalLibraries { get; }

        internal static List<FileID> LoadedFileIDs { get; } = new List<FileID>();

        static CommandManager()
        {
            InternalLibraryCallNames = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            InternalLibraries = new Dictionary<Type, Dictionary<string, MethodInfo>>();
            ExternalLibraryCallNames = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            ExternalLibraries = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        }

        internal static void LoadInternalCommands()
        {
            CFormat.WriteLine("[CommandManager] Loading internal commands...");

            List<Type> foundInternalLibraries = ((IEnumerable<Type>)Assembly.GetExecutingAssembly().GetTypes()).Where(t =>
            {
                if (t.IsClass && t.GetCustomAttributes().Where(a => a.GetType() == typeof(MMasterLibrary)).Any())
                    return t.Namespace == _InternalNamespace;
                return false;
            }).ToList();

            if (foundInternalLibraries.Count() == 0)
            {
                CFormat.WriteLine(ConsoleColor.Red, "[CommandManager] CRITICAL ISSUE: No internal commands loaded.",
                                                    "[CommandManager] Please report bug on GitHub at MMaster-Command-Prompt page.");
                CFormat.JumpLine();
                return;
            }

            foreach (Type library in foundInternalLibraries)
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

                InternalLibraryCallNames.Add(libraryCallName, library);
                InternalLibraries.Add(library, dictionary);
            }

            CFormat.WriteLine("[CommandManager] Internal commands loaded.");
            CFormat.JumpLine();
        }

        internal static void LoadExternalCommands(bool successMessage = false)
        {
            CFormat.WriteLine("[CommandManager] Loading external commands in application's directory (and sub-directories)...", ConsoleColor.Gray);
            LoadDirectory("", true, successMessage);
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
                    CFormat.WriteLine(string.Format("[CommandManager]    Found referenced assembly in \"{0}\": \"{1}\"", fileName, match.Groups[1].Value), ConsoleColor.Gray);
                }
                catch (Exception ex)
                {
                    CFormat.WriteLine(string.Format("[CommandManager]    Could not load referenced assembly in {0}: \"{1}\". Details: {2}", fileName, match.Captures[3].Value, ex.Message), ConsoleColor.Red);
                }
            }
        }

        internal static void LoadFile(string path, bool successMessage = true)
        {
            string fileName = Path.GetFileName(path);

            try
            {
                if (LoadedFileIDs.Any(x => x.Path == Path.Combine(Path.GetDirectoryName(path) == "" ? AppDomain.CurrentDomain.BaseDirectory : "", path)))
                {
                    CFormat.WriteLine("[CommandManager] Could not load file named \"" + fileName + "\" because it has already been loaded.", ConsoleColor.Red);
                }
                else
                {
                    FileID fileId = new FileID(CommandManager.LoadedFileIDs.Count, path);

                    CFormat.WriteLine("[CommandManager] Loading \"" + fileName + "\"...");

                    string fileCode = "";
                    using (StreamReader streamReader = new StreamReader(path))
                        fileCode = streamReader.ReadToEnd();

                    CompilerParameters parameters = new CompilerParameters()
                    {
                        GenerateInMemory = true
                    };

                    parameters.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);
                    CommandManager.ReferenceAssemblies(ref parameters, fileCode, fileName);
                    CompilerResults compilerResults = CommandManager._provider.CompileAssemblyFromSource(parameters, fileCode);

                    if ((uint)compilerResults.Errors.Count > 0U)
                    {
                        CFormat.WriteLine("[CommandManager]    Could not load file named \"" + fileName + "\":", ConsoleColor.Red);
                        foreach (CompilerError error in (CollectionBase)compilerResults.Errors)
                            CFormat.WriteLine(string.Format("[CommandManager]    ({0},{1}): error {2}: {3}", error.Line, error.Column, error.ErrorNumber, error.ErrorText), ConsoleColor.Red);
                    }
                    else
                    {
                        List<Type> foundExternalLibraries = compilerResults.CompiledAssembly.GetTypes()
                            .Where(t => t.IsClass && t.GetCustomAttributes().Where(a => a.GetType() == typeof(MMasterLibrary)).Any()).ToList();

                        if (foundExternalLibraries.Count == 0)
                        {
                            CFormat.WriteLine("[CommandManager]    \"" + fileName + "\" does not contain any library.", ConsoleColor.DarkYellow);
                            return;
                        }

                        foreach (Type library in foundExternalLibraries)
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
                                    CFormat.WriteLine(string.Format("[CommandManager]    Could not load command named \"{0}.{1}\" because one already exists.", libraryCallName, commandCallName), ConsoleColor.Red);
                                else
                                    dictionary.Add(commandCallName, methodInfo);
                            }

                            ExternalLibraryCallNames.Add(libraryCallName, library);
                            ExternalLibraries.Add(library, dictionary);
                        }

                        CommandManager.LoadedFileIDs.Add(fileId);
                        if (!successMessage)
                            return;
                        CFormat.WriteLine("[CommandManager]    Loaded \"" + fileName + "\" successfully!", ConsoleColor.Green);
                    }
                }
            }
            catch (Exception ex)
            {
                CFormat.WriteLine("[CommandManager]    Could not load file named \"" + fileName + "\": " + ex.Message, ConsoleColor.Red);
            }
        }

        internal static void LoadDirectory(string initialPath, bool subdirectories = true, bool successMessage = true)
        {
            string path = initialPath;
            int count = ExternalLibraries.Count;

            try
            {
                if (string.IsNullOrEmpty(path))
                    path = _externalDirectory;

                if (Directory.Exists(path))
                {
                    foreach (string file in Directory.GetFiles(path, "*.cs",
                        subdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))

                        LoadFile(file, successMessage);

                    if (ExternalLibraries.Count > count)
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
            ExternalLibraryCallNames.Clear();
            ExternalLibraries.Clear();
            LoadedFileIDs.Clear();
        }

        internal static void UnloadFile(int id)
        {
            try
            {
                foreach (string type in CommandManager.LoadedFileIDs[id].Types)
                {
                    foreach (string method in CommandManager.LoadedFileIDs[id].Methods)
                        CommandManager.ExternalLibraries[CommandManager.ExternalLibraryCallNames[type]].Remove(method);
                    CommandManager.ExternalLibraryCallNames.Remove(type);
                }
                CommandManager.LoadedFileIDs.RemoveAll((Predicate<FileID>)(x => x.ID == id));
                CFormat.WriteLine("File unloaded successfully.", ConsoleColor.Gray);
            }
            catch (Exception ex)
            {
                CFormat.WriteLine("[CommandManager] Could not unload file \"" + CommandManager.LoadedFileIDs[id].Path + "\". Details: " + ex.Message, ConsoleColor.Red);
            }
        }
    }
}