using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MMaster.Exceptions;
using System.Text;

namespace MMaster.Commands
{
    [MMasterLibrary("The commands of this library can be called without the 'Default.' prefix.")]
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

            foreach (Type index in CommandManager.ExternalLibraryCallNames.Values)
            {
                CFormat.WriteLine(index.Name, ConsoleColor.Yellow);
                foreach (MethodInfo methodInfo in CommandManager.ExternalLibraries[index].Values)
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

        [MMasterCommand()]
        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}