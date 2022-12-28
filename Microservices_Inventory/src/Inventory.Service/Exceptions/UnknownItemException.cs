using System;

namespace Inventory.Service.Exceptions
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