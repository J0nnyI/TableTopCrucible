using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.Library.Exceptions
{
    public class ThumbnailException : Exception
    {
        public ThumbnailException(string message = null, Exception innerException = null) : base(message, innerException) { }
    }
}
