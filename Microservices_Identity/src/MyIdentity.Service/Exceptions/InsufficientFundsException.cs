using System;

namespace MyIdentity.Service.Exceptions
{
    internal class InsufficientFundsException : Exception
    {
        public Guid UserId { get; }
        public decimal GilToDebit { get; }
        public InsufficientFundsException(Guid userId, decimal gilToDebit) : base($"Not enough gil to debit {gilToDebit} from user '{userId}'")
        {
            this.UserId = userId;
            this.GilToDebit = gilToDebit;
        }
    }
}