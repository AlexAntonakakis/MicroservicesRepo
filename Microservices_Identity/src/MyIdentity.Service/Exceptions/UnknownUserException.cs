using System;

namespace MyIdentity.Service.Exceptions
{
    internal class UnknownUserException : Exception 
    {
        public Guid UserId {get;}

        public UnknownUserException(Guid userId) : base ($"Unknown user '{userId}'")
        {
            this.UserId = userId;
        }

    }
}