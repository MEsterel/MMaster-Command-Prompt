using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MMaster.Commands
{
    [MMasterLibrary("The commands of this script can be called without the 'Default.' prefix.")]
    public static class Default
    {
        [MMasterCommand("Debug command for various tests.")]
        public static void Debug()
        {
            CFormat.Write("Download progress: ");
            for (int i = 0; i <= 1000; i++)
            {
                CFormat.DrawProgressBar(i, 1000, 47, '■', ConsoleColor.Green, ConsoleColor.DarkGray);
                System.Threading.Thread.Sleep(1);
            }
        }

        [MMasterCommand("Help", "Get the help prompt for a specific command.")]
        public static void Help(string command = null)
        {
            if (command == null)
            {
                Default.List();
            }
            else
            {
                CRawInputCommand rawInput = new CRawInputCommand(command);
                if (rawInput.LibraryClassType == (Type)null)
                {
                    CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
                }
                else
                {
                    Dictionary<string, MethodInfo> source;
                    if (CommandManager._internalCommandLibraries.ContainsKey(rawInput.LibraryClassType))
                        source = CommandManager._internalCommandLibraries[rawInput.LibraryClassType];
                    else if (CommandManager._externalCommandLibraries.ContainsKey(rawInput.LibraryClassType))
                    {
                        source = CommandManager._externalCommandLibraries[rawInput.LibraryClassType];
                    }
                    else
                    {
                        CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
                        return;
                    }
                    if (source.Any<KeyValuePair<string, MethodInfo>>((Func<KeyValuePair<string, MethodInfo>, bool>)(i => i.Key.ToLower().Equals(rawInput.Name.ToLower()))))
                    {
                        rawInput.Name = source.Keys.Where<string>((Func<string, bool>)(i => i.ToLower().Equals(rawInput.Name.ToLower()))).ToArray<string>()[0];
                        MethodInfo method = rawInput.LibraryClassType.GetMethod(rawInput.Name);
                        object[] array = ((IEnumerable<object>)method.GetCustomAttributes(false)).Where<object>((Func<object, bool>)(a => a.GetType().Name == typeof(MMasterCommand).Name)).ToArray<object>();
                        MMasterCommand mmasterCommand;
                        try
                        {
                            mmasterCommand = (MMasterCommand)array[0];
                        }
                        catch
                        {
                            CFormat.WriteLine("Help: This command does not exist.", ConsoleColor.Gray);
                            return;
                        }
                        if (mmasterCommand.HelpPrompt == "")
                        {
                            CFormat.WriteLine(CFormat.GetArgsFormat(rawInput.FullName, (IEnumerable<ParameterInfo>)method.GetParameters()), ConsoleColor.Gray);
                        }
                        else
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(mmasterCommand.HelpPrompt);
                            stringBuilder.Append(CFormat.GetArgsFormat(rawInput.FullName, (IEnumerable<ParameterInfo>)method.GetParameters()));
                            CFormat.WriteLine(stringBuilder.ToString(), ConsoleColor.Gray);
                        }
                    }
                    else
                        CFormat.WriteLine("This command does not exist.", ConsoleColor.Gray);
                }
            }
        }

        [MMasterCommand("Get the list of available commands.")]
        public static void List()
        {
            CFormat.WriteLine("For more information about a command, type 'Help <command>'.", ConsoleColor.Gray);
            CFormat.JumpLine();
            CFormat.WriteLine("[Internal commands]", ConsoleColor.Green);
            foreach (Type index in CommandManager._internalLibraryTypes.Values)
            {
                if (CommandManager._internalCommandLibraries[index].Values.Count != 0)
                {
                    CFormat.WriteLine(index.Name, ConsoleColor.Yellow);
                    foreach (MethodInfo methodInfo in CommandManager._internalCommandLibraries[index].Values)
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
                    CFormat.JumpLine();
                }
            }
            if (CommandManager._externalLibraryTypes.Count == 0)
                return;
            CFormat.WriteLine("[External commands]", ConsoleColor.Green);
            int num = 1;
            foreach (Type index in CommandManager._externalLibraryTypes.Values)
            {
                CFormat.WriteLine(index.Name, ConsoleColor.Yellow);
                foreach (MethodInfo methodInfo in CommandManager._externalCommandLibraries[index].Values)
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
                if (num < CommandManager._externalLibraryTypes.Values.Count)
                    CFormat.JumpLine();
                ++num;
            }
        }

        [MMasterCommand("Exit", "Exit the application.")]
        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}