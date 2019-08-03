using MMaster.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MMaster
{
    internal class Program
    {
        private static readonly string appName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string bootMessage = Program.appName + " Command Prompt [version " + Program.appVersion.ToString() + "]" + Environment.NewLine + "(c) 2017-2019 Matthieu Badoy. All rights reserved.";

        private static void Main(string[] args)
        {
            Console.Title = Program.appName;
            CFormat.WriteLine(Program.bootMessage, ConsoleColor.Cyan);
            CFormat.JumpLine();
            CommandManager.LoadInternalCommands();
            CommandManager.LoadExternalCommands(true);
            CFormat.JumpLine();
            CFormat.WriteLine("Type 'List' to get the list of available commands.", ConsoleColor.Gray);
            Program.Run();
        }

        private static void Run()
        {
            while (true)
            {
                string userInput;
                do
                {
                    CFormat.JumpLine();
                    userInput = CInput.ReadFromConsole("", ConsoleInputType.String, false, -1, char.MinValue).ToString();
                }
                while (string.IsNullOrWhiteSpace(userInput));

                Execute(userInput);
            }
        }

        private static void Execute(string userInput)
        {
            try
            {
                CParsedInput parsedInput = new CParsedInput(userInput);

                parsedInput.CommandMethodInfo.Invoke(null, parsedInput.Parameters);
            }
            catch (WrongCallFormatException)
            {
                CFormat.WriteLine("Wrong call format.","The call should be as it follows: <Library>.<Command> [arg1] [arg2] [etc.]");
            }
            catch (LibraryNotExistingException)
            {
                CFormat.WriteLine("This library does not exist.");
            }
            catch (CommandNotExistingException)
            {
                CFormat.WriteLine("This command does not exist.");
            }
            catch (MissingArgumentException ex)
            {
                CFormat.WriteLine("Missing required argument.", CFormat.GetArgsFormat(ex.ParsedInput.FullCallName, ex.ParsedInput.CommandMethodInfo.GetParameters()));
            }
            catch (ArgumentCoerceException ex)
            {
                CFormat.WriteLine(string.Format("The argument '{0}' cannot be parsed to type '{1}'", ex.ArgumentName, ex.ArgumentTargetType));
            }
            catch (Exception ex)
            {
                CFormat.WriteLine(ex.Message);
            }
        }
    }
}