using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMaster.Exceptions
{
    internal class ArgumentParseException : Exception
    {
        internal string ArgumentName { get; }
        internal string ArgumentTargetType { get; }

        internal ArgumentParseException() : base() { }

        internal ArgumentParseException(string argumentName, string argumentTargetType) : base()
        {
            ArgumentName = argumentName;
            ArgumentTargetType = argumentTargetType;
        }
    }
}
