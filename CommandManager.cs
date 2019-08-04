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
        internal const string _ExternalExtension = ".cs";
        internal const string _InternalNamespace = "MMaster.Commands";

        internal static Dictionary<string, Type> InternalLibraryCallNames { get; }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> InternalLibraries { get; }

        internal static Dictionary<string, Type> ExternalLibraryCallNames { get; }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> ExternalLibraries { get; }

        internal static List<FileID> LoadedFileIDs { get; private set; } = new List<FileID>();

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
            CFormat.WriteLine("[CommandManager] Loading external commands in application's directory (and sub-directories)...");
            LoadDirectory("", true, successMessage);
            LoadedFileIDs = LoadedFileIDs.OrderBy(x => x.ID).ToList();
        }

        private static void ReferenceAssemblies(ref CompilerParameters parameters, string fileCode, string fileName, bool isdependency = false, int degree = 0)
        {
            foreach (Match match in new Regex("// REF \\\"(.+)\\\"").Matches(fileCode))
            {
                try
                {
                    parameters.ReferencedAssemblies.Add(match.Groups[1].Value);
                    CFormat.WriteLine(string.Format("[CommandManager]    {0}Found referenced assembly in \"{1}\": \"{2}\"", CFormat.Indent(degree*3), fileName, match.Groups[1].Value), ConsoleColor.Gray);
                }
                catch (Exception ex)
                {
                    CFormat.WriteLine(string.Format("[CommandManager]    {0}Could not load referenced assembly in {1}: \"{2}\". Details: {3}", CFormat.Indent(degree*3), fileName, match.Captures[3].Value, ex.Message), ConsoleColor.Red);
                }
            }
        }

        private static void Loaddependencies(FileID fileId, string fileCode, int degree = 0)
        {
            foreach (Match match in new Regex("// DEP \\\"(.+)\\\"").Matches(fileCode))
            {
                string result = Path.GetFullPath(match.Groups[1].Value);

                FileAttributes fileAttributes = File.GetAttributes(result);

                if (fileAttributes.HasFlag(FileAttributes.Directory) && Directory.Exists(result))
                {
                    CFormat.WriteLine(string.Format("[CommandManager]    {0}Found dependency in \"{1}\": loading \"{2}\" directory.", CFormat.Indent(degree*3), fileId.Name, result), ConsoleColor.Cyan);
                    LoadDirectory(result, true, true, true, degree + 1);
                }
                else if (File.Exists(result))
                {
                    CFormat.WriteLine(string.Format("[CommandManager]    {0}Found dependency in \"{1}\": loading \"{2}\" file.", CFormat.Indent(degree * 3), fileId.Name, result), ConsoleColor.Cyan);

                    if (Path.GetExtension(result) != _ExternalExtension)
                    {
                        CFormat.WriteLine(string.Format("[CommandManager]       {0}The file dependency \"{1}\" is not a (*.cs) file and was not loaded.", CFormat.Indent(degree * 3), result), ConsoleColor.Red);
                        return;
                    }

                    LoadFile(result, true, true, degree + 1);
                }
                else
                {
                    CFormat.WriteLine(string.Format("[CommandManager]    The dependency \"{0}\" found in \"{1}\" does not exist.", result, fileId.Name), ConsoleColor.DarkYellow);
                }
            }
        }

        internal static void LoadFile(string rawPath, bool successMessage = true, bool isdependency = false, int degree = 0)
        {
            string path = Path.GetFullPath(rawPath);

            FileID fileId = new FileID(CommandManager.LoadedFileIDs.Count + (isdependency ? 1:0), path, isdependency);

            try
            {
                if (LoadedFileIDs.Any(x => x.Path == path && !x.LoadedAsdependency))
                {
                    CFormat.WriteLine(string.Format("[CommandManager] {0}Could not load file named \"{1}\" because it has already been loaded.", CFormat.Indent(degree*3), fileId.Name), ConsoleColor.Red);
                    return;
                }
                else if (LoadedFileIDs.Any(x => x.Path == path && x.LoadedAsdependency))
                {
                    return;
                }
                else
                {
                    CFormat.WriteLine(string.Format("[CommandManager] {0}Loading \"{1}\"...", CFormat.Indent(degree*3), fileId.Name));

                    string fileCode = "";
                    using (StreamReader streamReader = new StreamReader(path))
                        fileCode = streamReader.ReadToEnd();

                    CompilerParameters parameters = new CompilerParameters()
                    {
                        GenerateInMemory = true
                    };

                    parameters.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);
                    ReferenceAssemblies(ref parameters, fileCode, fileId.Name, isdependency, degree);
                    Loaddependencies(fileId, fileCode, degree);
                    CompilerResults compilerResults = _provider.CompileAssemblyFromSource(parameters, fileCode);

                    if ((uint)compilerResults.Errors.Count > 0U)
                    {
                        CFormat.WriteLine(string.Format("[CommandManager] {0}Could not load file named \"{1}\":", CFormat.Indent(degree*3), fileId.Name), ConsoleColor.Red);
                        foreach (CompilerError error in compilerResults.Errors)
                            CFormat.WriteLine(string.Format("[CommandManager] {0}({1},{2}): error {3}: {4}", CFormat.Indent(degree*3), error.Line, error.Column, error.ErrorNumber, error.ErrorText), ConsoleColor.Red);
                    }
                    else
                    {
                        List<Type> foundExternalLibraries = compilerResults.CompiledAssembly.GetTypes()
                            .Where(t => t.IsClass && t.GetCustomAttributes().Where(a => a.GetType() == typeof(MMasterLibrary)).Any()).ToList();

                        if (foundExternalLibraries.Count == 0)
                        {
                            CFormat.WriteLine(string.Format("[CommandManager]    {0}\"{1}\" does not contain any library.", CFormat.Indent(degree*3), fileId.Name), ConsoleColor.DarkYellow);
                            return;
                        }

                        foreach (Type library in foundExternalLibraries)
                        {
                            string libraryCallName = library.GetCustomAttribute<MMasterLibrary>().CallName;
                            if (String.IsNullOrEmpty(libraryCallName))
                            {
                                libraryCallName = library.Name;
                            }

                            if (InternalLibraryCallNames.ContainsKey(libraryCallName) || ExternalLibraryCallNames.ContainsKey(libraryCallName))
                            {
                                CFormat.WriteLine(string.Format("[CommandManager]    {0}Could not load library named \"{1}\" in \"{2}\" because one with the same name already exists.", CFormat.Indent(degree*3), libraryCallName, fileId.Name), ConsoleColor.DarkYellow);
                                continue;
                            }

                            IEnumerable<MethodInfo> methodInfos = library.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
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
                                    CFormat.WriteLine(string.Format("[CommandManager]    {0}Could not load command named \"{1}.{2}\" in \"{3}\" because one with the same name already exists.", CFormat.Indent(degree*3), libraryCallName, commandCallName, fileId.Name), ConsoleColor.DarkYellow);
                                else
                                    dictionary.Add(commandCallName, methodInfo);
                            }

                            ExternalLibraryCallNames.Add(libraryCallName, library);
                            ExternalLibraries.Add(library, dictionary);
                            fileId.LibraryCallNames.Add(libraryCallName);
                        }

                        LoadedFileIDs.Add(fileId);
                        if (!successMessage)
                            return;
                        CFormat.WriteLine(string.Format("[CommandManager] {0}Loaded \"{1}\" successfully!", CFormat.Indent(degree*3), fileId.Name), isdependency ? ConsoleColor.Cyan : ConsoleColor.Green);
                    }
                }
            }
            catch (Exception ex)
            {
                CFormat.WriteLine(string.Format("[CommandManager] {0}Could not load file named \"{1}\": {2}", CFormat.Indent(degree*3), fileId.Name, ex.Message), ConsoleColor.Red);
            }
        }

        internal static void LoadDirectory(string initialPath, bool subdirectories = true, bool successMessage = true, bool isdependency = false, int degree = 0)
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

                        LoadFile(file, successMessage, isdependency, degree);

                    if (ExternalLibraries.Count > count)
                    {
                        if (successMessage && !isdependency)
                        {
                            if (string.IsNullOrEmpty(initialPath))
                                CFormat.WriteLine(string.Format("[CommandManager] {0}External commands loaded.", CFormat.Indent(degree*3)));
                            else
                                CFormat.WriteLine(string.Format("[CommandManager] {0}External commands in \"{1}\" directory successfully loaded!", CFormat.Indent(degree * 3), path));
                        }
                    }
                    else
                        if (!isdependency)
                            CFormat.WriteLine(string.Format("[CommandManager] {0}No external commands loaded.", CFormat.Indent(degree * 3)));
                }
                else
                    CFormat.WriteLine(string.Format("[CommandManager] {0}Could not find directory named \"{1}\".", CFormat.Indent(degree * 3), Path.GetDirectoryName(path)), ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                CFormat.WriteLine(string.Format("[CommandManager] {0}Could not load directory named \"{1}\": {2}", CFormat.Indent(degree * 3), Path.GetDirectoryName(path), ex.Message), ConsoleColor.Red);
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
                foreach (string libraryCallName in LoadedFileIDs[id].LibraryCallNames)
                {
                    ExternalLibraries.Remove(ExternalLibraryCallNames[libraryCallName]);
                    ExternalLibraryCallNames.Remove(libraryCallName);
                }
                LoadedFileIDs.RemoveAll(x => x.ID == id);
                CFormat.WriteLine("[ComandManager] File unloaded successfully!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                CFormat.WriteLine("[CommandManager] Could not unload file \"" + CommandManager.LoadedFileIDs[id].Path + "\". Details: " + ex.Message, ConsoleColor.Red);
            }
        }
    }
}