using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMaster.Exceptions
{
    internal class ArgumentCoerceException : Exception
    {
        internal string ArgumentName { get; }
        internal string ArgumentTargetType { get; }

        internal ArgumentCoerceException() : base() { }

        internal ArgumentCoerceException(string argumentName, string argumentTargetType) : base()
        {
            ArgumentName = argumentName;
            ArgumentTargetType = argumentTargetType;
        }
    }
}
