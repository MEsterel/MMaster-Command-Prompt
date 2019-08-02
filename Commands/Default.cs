using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MMaster.Commands
{
    [MMasterLibrary("The commands of this script can be called without the 'Default.' prefix.")]
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
                CParsedInput parsedInput = new CParsedInput(stringCommand);
                if (parsedInput.Library == (Type)null)
                {
                    CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
                }
                else
                {
                    Dictionary<string, MethodInfo> source;

                    if (CommandManager.InternalLibraries.ContainsKey(parsedInput.Library))
                    {
                        source = CommandManager.InternalLibraries[parsedInput.Library];
                    }
                    else if (CommandManager.ExternalLibraries.ContainsKey(parsedInput.Library))
                    {
                        source = CommandManager.ExternalLibraries[parsedInput.Library];
                    }
                    else
                    {
                        CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
                        return;
                    }

                    if (source.Any<KeyValuePair<string, MethodInfo>>((Func<KeyValuePair<string, MethodInfo>, bool>)(i => i.Key.ToLower().Equals(parsedInput.RawCall.ToLower()))))
                    {
                        parsedInput.RawCall = source.Keys.Where<string>((Func<string, bool>)(i => i.ToLower().Equals(parsedInput.RawCall.ToLower()))).ToArray<string>()[0];

                        MethodInfo method = parsedInput.Library.GetMethod(parsedInput.RawCall);

                        object[] array = ((IEnumerable<object>)method.GetCustomAttributes(false)).Where<object>((Func<object, bool>)(a => a.GetType().Name == typeof(MMasterCommand).Name)).ToArray<object>();

                        MMasterCommand mmasterCommand;
                        try
                        {
                            mmasterCommand = (MMasterCommand)array[0];
                        }
                        catch
                        {
                            CFormat.WriteLine("Help: This command does not exist.");
                            return;
                        }
                        if (mmasterCommand.HelpPrompt == "")
                        {
                            CFormat.WriteLine(CFormat.GetArgsFormat(parsedInput.FullName, (IEnumerable<ParameterInfo>)method.GetParameters()), ConsoleColor.Gray);
                        }
                        else
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(mmasterCommand.HelpPrompt);
                            stringBuilder.Append(CFormat.GetArgsFormat(parsedInput.FullName, (IEnumerable<ParameterInfo>)method.GetParameters()));
                            CFormat.WriteLine(stringBuilder.ToString());
                        }
                    }
                    else
                    {
                        CFormat.WriteLine("This command does not exist.", ConsoleColor.Gray);
                    }
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
                        string str = " (";
                        MMasterCommand mMasterCommand = methodInfo.GetCustomAttribute<MMasterCommand>();
                        try
                        {
                            str = str + mMasterCommand.HelpPrompt + ")";
                        }
                        catch
                        {
                        }
                        CFormat.WriteLine(CFormat.Indent(3) + "." + methodInfo.Name + str, ConsoleColor.Gray);
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
                    string str = " (";
                    object[] array = ((IEnumerable<object>)methodInfo.GetCustomAttributes(false)).Where<object>((Func<object, bool>)(a => a.GetType().Name == typeof(MMasterCommand).Name)).ToArray<object>();
                    try
                    {
                        MMasterCommand mmasterCommand = (MMasterCommand)array[0];
                        str = str + mmasterCommand.HelpPrompt + ")";
                    }
                    catch
                    {
                    }
                    CFormat.WriteLine(CFormat.Indent(3) + "." + methodInfo.Name + str, ConsoleColor.Gray);
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