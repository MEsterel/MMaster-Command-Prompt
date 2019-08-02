using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMaster.Exceptions
{
    internal class MissingArgumentException : Exception
    {
        internal string MissingArgumentName { get; }

        internal MissingArgumentException() : base() { }

        internal MissingArgumentException(string missingArgumentName) : base()
        {
            MissingArgumentName = missingArgumentName;
        }
    }
}
