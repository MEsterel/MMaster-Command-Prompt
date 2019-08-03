using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MMaster.Exceptions;
using System.Text;
using System.IO;

namespace MMaster.Commands
{
    [MMasterLibrary("The commands of this library can be called without the '_default.' prefix.","_default")]
    public static class Default
    {
        [MMasterCommand("Debug command for various tests.")]
        public static void Debug()
        {
            CFormat.Write("Download progress: ");
            for (int i = 0; i <= 100; i++)
            {
                CFormat.DrawProgressBar(i, 100, 10, '■', ConsoleColor.Green, ConsoleColor.DarkGray);
                System.Threading.Thread.Sleep(10);
            }
        }

        [MMasterCommand("Get the help prompt for a specific command.", "Help")]
        public static void Help(string stringCommand = null)
        {
            if (stringCommand == null)
            {
                Default.List();
            }
            else
            {
                try
                {
                    CParsedInput parsedInput = new CParsedInput(stringCommand, true);

                    string helpPrompt = parsedInput.CommandMethodInfo.GetCustomAttribute<MMasterCommand>().HelpPrompt;

                    if (helpPrompt == "")
                    {
                        CFormat.WriteLine(CFormat.GetArgsFormat(parsedInput.FullCallName, parsedInput.CommandMethodInfo.GetParameters()));
                    }
                    else
                    {
                        CFormat.WriteLine(helpPrompt, CFormat.GetArgsFormat(parsedInput.FullCallName, parsedInput.CommandMethodInfo.GetParameters()));
                    }
                }
                catch (WrongCallFormatException)
                {
                    CFormat.WriteLine("Wrong call format.", "The call should be as it follows: <Library>.<Command> [arg1] [arg2] [etc.]");
                }
                catch (LibraryNotExistingException)
                {
                    CFormat.WriteLine("This library does not exist.");
                }
                catch (CommandNotExistingException)
                {
                    CFormat.WriteLine("This command does not exist.");
                }
            }
        }

        [MMasterCommand("Get the list of available commands.")]
        public static void List()
        {
            CFormat.WriteLine("For more information about a command, type 'Help <command>'.", ConsoleColor.Gray);
            CFormat.JumpLine();
            CFormat.WriteLine("[Internal commands]", ConsoleColor.Green);
            foreach (Type library in CommandManager.InternalLibraryCallNames.Values)
            {
                if (CommandManager.InternalLibraries[library].Values.Count != 0)
                {
                    string libraryCallName = CommandManager.InternalLibraryCallNames.FirstOrDefault(x => x.Value == library).Key;
                    string libraryHelpPrompt = library.GetCustomAttribute<MMasterLibrary>().HelpPrompt;
                    if (!String.IsNullOrEmpty(libraryHelpPrompt))
                    {
                        libraryHelpPrompt = " (" + libraryHelpPrompt + ")";
                    }

                    CFormat.WriteLine(libraryCallName + libraryHelpPrompt, ConsoleColor.Yellow);

                    foreach (MethodInfo methodInfo in CommandManager.InternalLibraries[library].Values)
                    {
                        MMasterCommand mMasterCommand = methodInfo.GetCustomAttribute<MMasterCommand>();
                        string helpPrompt = mMasterCommand.HelpPrompt;
                        if (!String.IsNullOrEmpty(helpPrompt))
                        {
                            helpPrompt = " (" + helpPrompt + ")";
                        }
                        CFormat.WriteLine(CFormat.Indent(3) + "." + methodInfo.Name + helpPrompt);
                    }
                    CFormat.JumpLine();
                }
            }

            if (CommandManager.ExternalLibraryCallNames.Count == 0)
                return;

            CFormat.WriteLine("[External commands]", ConsoleColor.Green);
            int num = 1;

            foreach (Type library in CommandManager.ExternalLibraryCallNames.Values)
            {
                string libraryCallName = CommandManager.ExternalLibraryCallNames.FirstOrDefault(x => x.Value == library).Key;
                string libraryHelpPrompt = library.GetCustomAttribute<MMasterLibrary>().HelpPrompt;
                if (!String.IsNullOrEmpty(libraryHelpPrompt))
                {
                    libraryHelpPrompt = " (" + libraryHelpPrompt + ")";
                }

                CFormat.WriteLine(libraryCallName + libraryHelpPrompt, ConsoleColor.Yellow);
                foreach (MethodInfo methodInfo in CommandManager.ExternalLibraries[library].Values)
                {
                    MMasterCommand mMasterCommand = methodInfo.GetCustomAttribute<MMasterCommand>();
                    string helpPrompt = mMasterCommand.HelpPrompt;
                    if (!String.IsNullOrEmpty(helpPrompt))
                    {
                        helpPrompt = " (" + helpPrompt + ")";
                    }
                    CFormat.WriteLine(CFormat.Indent(3) + "." + methodInfo.Name + helpPrompt);
                }

                if (num < CommandManager.ExternalLibraryCallNames.Values.Count)
                    CFormat.JumpLine();
                ++num;
            }
        }

        [MMasterCommand("Create a template file of external commands. If path is not specified, file is saved in application's directory.")]
        public static void CreateTemplate(string path = null)
        {
            try
            {
                if (path == null)
                    path = "TemplateFile" + Path.GetExtension("*.cs");
                CFormat.WriteLine("Creating template file \"" + path + "\"");
                if (File.Exists(path))
                {
                    CFormat.WriteLine("A file named \"" + path + "\" already exists. Replace it?");
                    ConsoleAnswer answer = CInput.UserChoice(ConsoleAnswerType.YesNo, true);
                    if (answer == ConsoleAnswer.No)
                    {
                        int num = 1;
                        string withoutExtension = Path.GetFileNameWithoutExtension(path);
                        while (File.Exists(path))
                        {
                            path = Path.Combine(Path.GetDirectoryName(path), withoutExtension + " (" + num + ")", Path.GetExtension(path));
                            ++num;
                        }
                    }
                    else if (answer == ConsoleAnswer.Escaped)
                    {
                        return;
                    }
                }

                using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                    streamWriter.Write(Properties.Resources.FileTemplate);

                CFormat.WriteLine("Template file named \"" + path + "\" created!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                CFormat.WriteLine("Could not create template file \"" + path + "\" Details: " + ex.Message, ConsoleColor.Red);
            }
        }

        // PREVIOUS CmdMngr
        public const string _templateFileNameWithoutExtension = "MMaster Command File";

        [MMasterCommand("Reload '*.cs' external commands located in the application's directory.")]
        public static void Reload()
        {
            CommandManager.ClearExternalCommands();
            CFormat.WriteLine("[CommandManager] Cleared loaded external commands.", ConsoleColor.Gray);
            CFormat.JumpLine();
            CommandManager.LoadExternalCommands(true);
        }

        [MMasterCommand("Load a file of external commands.")]
        public static void LoadFile(string path)
        {
            CommandManager.LoadFile(path, true);
        }

        [MMasterCommand("Load '*.cs' files of external commands in a directory.")]
        public static void LoadDirectory(string path = null, bool subdirectories = false)
        {
            CommandManager.LoadDirectory(path, subdirectories, true);
        }

        [MMasterCommand("Unload loaded external commands.")]
        public static void Unload()
        {
            CommandManager.ClearExternalCommands();
            CFormat.WriteLine("[CommandManager] Cleared loaded external commands.", ConsoleColor.Gray);
        }

        [MMasterCommand("Get the list of the loaded files.")]
        public static void LoadedFiles()
        {
            if (CommandManager.LoadedFileIDs.Count == 0)
            {
                CFormat.WriteLine("[CommandManager] There are no external files loaded.", ConsoleColor.Gray);
            }
            else
            {
                CFormat.WriteLine("[CommandManager] List of loaded files: ", ConsoleColor.Gray);
                foreach (FileID listLoadedFile in CommandManager.LoadedFileIDs)
                    CFormat.WriteLine(string.Format("{0}({1}) {2}", CFormat.Indent(2), listLoadedFile.ID, listLoadedFile.Path), ConsoleColor.Gray);
            }
        }

        [MMasterCommand("Unload a file.")]
        public static void UnloadFile()
        {
            LoadedFiles();
            CFormat.JumpLine();
            CFormat.WriteLine("Please enter the number ID of the file you want to unload.", ConsoleColor.Gray);
            int id = CInput.UserPickInt(CommandManager.LoadedFileIDs.Count - 1);
            if (id == -1)
                return;
            CommandManager.UnloadFile(id);
        }


        [MMasterCommand()]
        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}