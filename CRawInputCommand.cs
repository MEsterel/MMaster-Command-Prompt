using MMaster.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MMaster
{
    internal class CRawInputCommand
    {
        private List<string> _arguments;

        internal string Name { get; set; }

        internal Type LibraryClassType { get; set; }

        internal string FullName
        {
            get
            {
                return this.LibraryClassType.Name + "." + this.Name;
            }
        }

        internal IEnumerable<string> Arguments
        {
            get
            {
                return (IEnumerable<string>)this._arguments;
            }
        }

        internal CRawInputCommand(string input)
        {
            string[] strArray1 = Regex.Split(input, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            this._arguments = new List<string>();
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (index == 0)
                {
                    this.Name = strArray1[index];
                    this.LibraryClassType = typeof(Default);
                    string[] strArray2 = strArray1[0].Split('.');
                    if (strArray2.Length == 2)
                    {
                        this.LibraryClassType = !CommandManager._internalLibraryTypes.ContainsKey(strArray2[0]) ? (!CommandManager._externalLibraryTypes.ContainsKey(strArray2[0]) ? (Type)null : CommandManager._externalLibraryTypes[strArray2[0]]) : CommandManager._internalLibraryTypes[strArray2[0]];
                        this.Name = strArray2[1];
                    }
                }
                else
                {
                    string input1 = strArray1[index];
                    string str = input1;
                    Match match = new Regex("\"(.*?)\"", RegexOptions.Singleline).Match(input1);
                    if (match.Captures.Count > 0)
                        str = new Regex("[^\"]*[^\"]").Match(match.Captures[0].Value).Captures[0].Value;
                    this._arguments.Add(str);
                }
            }
        }
    }
}