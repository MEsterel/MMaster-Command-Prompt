using System;

namespace MMaster.Exceptions
{
    [Serializable]
    internal class CommandNotExistingException : Exception
    {
        internal CParsedInput ParsedInput { get; }
        internal CommandNotExistingException() : base()
        { }

        internal CommandNotExistingException(CParsedInput parsedInput) : base()
        {
            ParsedInput = parsedInput;
        }
    }
}