using System;

namespace Play.Trading.Service.Exceptions
{

    [Serializable]
    internal class UnknownItemException : Exception
    {
        public Guid itemId { get; }

        public UnknownItemException(Guid itemId): base($"Unknown item '{itemId}'")
        {
            this.itemId = itemId;
        }

    }
}