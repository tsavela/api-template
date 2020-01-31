using System;

namespace Core.Exceptions
{
    /// <summary>
    /// Used for raising validation errors.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
