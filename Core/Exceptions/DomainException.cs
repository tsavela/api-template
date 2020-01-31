using System;

namespace Core.Exceptions
{
    /// <summary>
    /// Used for raising "business rule exceptions". The message should be user friendly.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string userFriendlyMessage) : base(userFriendlyMessage)
        {
        }
    }
}
