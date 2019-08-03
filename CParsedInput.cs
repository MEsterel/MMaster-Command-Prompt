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

        internal object[] Parameters { get; } = null;

        internal string RawCall { get; }

        internal Type Library { get; }

        internal string LibraryCallName
        {
            get
            {
                if (CommandManager.InternalLibraries.ContainsKey(Library))
                    return CommandManager.InternalLibraryCallNames.FirstOrDefault(x => x.Value == Library).Key;
                else
                    return CommandManager.ExternalLibraryCallNames.FirstOrDefault(x => x.Value == Library).Key;
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

        internal CParsedInput(string input, bool ignoreArguments = false)
        {
            string[] splitInput = Regex.Split(input, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            Arguments = new List<string>();

            this.RawCall = splitInput[0];

            string[] splitCall = splitInput[0].Split('.');

            // LIBRARY & COMMAND
            if (splitCall.Length == 1) // We assume here that if there is no dot, then this implies that the target library is Default.
            {
                Library = typeof(Default);

                if (CommandManager.InternalLibraries[Library].Keys.Any(x => x.ToLower() == splitCall[0].ToLower()))
                {
                    CommandCallName = CommandManager.InternalLibraries[Library].Keys.FirstOrDefault(x => x.ToLower() == splitCall[0].ToLower());
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

                    if (CommandManager.InternalLibraries[Library].Keys.Any(x => x.ToLower() == splitCall[1].ToLower()))
                    {
                        CommandCallName = CommandManager.InternalLibraries[Library].Keys.FirstOrDefault(x => x.ToLower() == splitCall[1].ToLower());
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

                    if (CommandManager.ExternalLibraries[Library].Keys.Any(x => x.ToLower() == splitCall[1].ToLower()))
                    {
                        CommandCallName = CommandManager.ExternalLibraries[Library].Keys.FirstOrDefault(x => x.ToLower() == splitCall[1].ToLower());
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
                throw new WrongCallFormatException();
            }



            if (ignoreArguments)
                return;



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

            // COERCE ARGS
            IEnumerable<ParameterInfo> parameterInfos = CommandMethodInfo.GetParameters().ToList();
            List<object> objectList = new List<object>(); // Collecting args as objects

            IEnumerable<ParameterInfo> parameterInfosNecessary = parameterInfos.Where(p => !p.IsOptional);
            IEnumerable<ParameterInfo> parameterInfosOptional = parameterInfos.Where(p => p.IsOptional);

            if (parameterInfosNecessary.Count() > Arguments.Count())
            {
                throw new MissingArgumentException(this);
            }
            else
            {
                if (parameterInfos.Count() > 0)
                {
                    foreach (ParameterInfo PI in parameterInfos)
                        objectList.Add(PI.DefaultValue);

                    for (int index = 0; index < parameterInfos.Count(); ++index)
                    {
                        ParameterInfo PI = parameterInfos.ElementAt(index);
                        Type parameterType = PI.ParameterType;

                        try // The following instruction can fail (in case of optional args) so its accessibility is tested before.
                        {
                            Arguments.ElementAt(index);
                        }
                        catch
                        {
                            continue;
                        }

                        object obj = CFormat.CoerceArgument(parameterType, Arguments.ElementAt(index));
                        objectList.RemoveAt(index);
                        objectList.Insert(index, obj);
                    }

                    // EXPORT PARAMETERS to an array
                    if (objectList.Count > 0)
                        Parameters = objectList.ToArray();
                }
            }
        }

        internal static Type ParseLibrary(string userInput)
        {
            if (CommandManager.InternalLibraryCallNames.Keys.Any(x => x.ToLower() == userInput.ToLower()))
            {
                return CommandManager.InternalLibraryCallNames.FirstOrDefault(x => x.Key.ToLower() == userInput.ToLower()).Value;
            }
            else if (CommandManager.ExternalLibraryCallNames.Keys.Any(x => x.ToLower() == userInput.ToLower()))
            {
                return CommandManager.ExternalLibraryCallNames.FirstOrDefault(x => x.Key.ToLower() == userInput.ToLower()).Value;
            }
            else
            {
                return null;
            }
        }
    }
}