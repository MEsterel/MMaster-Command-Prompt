using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMaster.Exceptions
{
    [Serializable]
    internal class MissingArgumentException : Exception
    {
        internal CParsedInput ParsedInput { get; }

        internal MissingArgumentException() : base() { }
        internal MissingArgumentException(CParsedInput parsedInput) : base()
        {
            ParsedInput = parsedInput;
        }
    }
}
