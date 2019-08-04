using System;

namespace MMaster.Exceptions
{
    [Serializable]
    internal class LibraryNotExistingException : Exception
    {
        internal CParsedInput ParsedInput { get; }

        internal LibraryNotExistingException() : base() { }

        internal LibraryNotExistingException(CParsedInput parsedInput) : base()
        {
            ParsedInput = parsedInput;
        }
    }
}