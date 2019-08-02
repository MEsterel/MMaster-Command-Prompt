using System;

namespace MMaster.Exceptions
{
    internal class WrongCallFormatException : Exception
    {
        internal WrongCallFormatException() : base()
        {
        }

        internal WrongCallFormatException(string message) : base(message)
        {
        }
    }
}