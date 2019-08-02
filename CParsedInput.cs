using MMaster.Commands;
using MMaster.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MMaster
{
    internal class CParsedInput
    {
        private List<string> Arguments;

        internal string RawCall { get; }

        internal Type Library { get; }

        internal string LibraryCallName
        {
            get
            {
                return CommandManager.InternalLibraryCallNames.FirstOrDefault(x => x.Value == Library).Key;
            }
        }

        internal MethodInfo CommandMethodInfo { get; }

        internal string CommandCallName { get; }

        internal string FullCallName
        {
            get
            {
                return this.LibraryCallName + "." + this.CommandCallName;
            }
        }

        internal CParsedInput(string input)
        {
            string[] splitInput = Regex.Split(input, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            Arguments = new List<string>();

            this.RawCall = splitInput[0];

            string[] splitCall = splitInput[0].Split('.');

            // LIBRARY & COMMAND
            if (splitCall.Length == 1) // We assume here that if there is no dot, then this implies that the target library is Default.
            {
                Library = typeof(Default);

                if (CommandManager.InternalLibraries[Library].Keys.Any(x => x.ToLower() == splitCall[1]))
                {
                    CommandCallName = CommandManager.InternalLibraries[Library].Keys.FirstOrDefault(x => x.ToLower() == splitCall[1]);
                    CommandMethodInfo = CommandManager.InternalLibraries[Library][CommandCallName];
                }
                else
                {
                    throw new CommandNotExistingException();
                }
            }
            else if (splitCall.Length == 2) // Most common scenarios
            {
                if (CommandManager.InternalLibraryCallNames.ContainsKey(splitCall[0]))
                {
                    Library = CommandManager.InternalLibraryCallNames[splitCall[0]];

                    if (CommandManager.InternalLibraries[Library].Keys.Any(x => x.ToLower() == splitCall[1]))
                    {
                        CommandCallName = CommandManager.InternalLibraries[Library].Keys.FirstOrDefault(x => x.ToLower() == splitCall[1]);
                        CommandMethodInfo = CommandManager.InternalLibraries[Library][CommandCallName];
                    }
                    else
                    {
                        throw new CommandNotExistingException();
                    }
                }
                else if (CommandManager.ExternalLibraryCallNames.ContainsKey(splitCall[0]))
                {
                    Library = CommandManager.ExternalLibraryCallNames[splitCall[0]];

                    if (CommandManager.ExternalLibraries[Library].Keys.Any(x => x.ToLower() == splitCall[1]))
                    {
                        CommandCallName = CommandManager.ExternalLibraries[Library].Keys.FirstOrDefault(x => x.ToLower() == splitCall[1]);
                        CommandMethodInfo = CommandManager.ExternalLibraries[Library][CommandCallName];
                    }
                    else
                    {
                        throw new CommandNotExistingException();
                    }
                }
                else
                {
                    throw new LibraryNotExistingException();
                }
            }
            else
            {
                throw new WrongCallFormatException("The call should be as it follows: <Library>.<Command> [arg1] [arg2] [etc.]");
            }

            // PARSE ARGS
            for (int index = 1; index < splitInput.Length; ++index)
            {
                string rawArg = splitInput[index];
                string computedArg = rawArg;
                Match match = new Regex("\"(.*?)\"", RegexOptions.Singleline).Match(rawArg);
                if (match.Captures.Count > 0)
                    computedArg = new Regex("[^\"]*[^\"]").Match(match.Captures[0].Value).Captures[0].Value;
                this.Arguments.Add(computedArg);
            }
        }
    }
}