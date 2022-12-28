
namespace Inventory.Contracts
{
    public record GrantItems(
        Guid UserId,
        Guid CatalogItemId, 
        int Quantity, 
        Guid CorrelationId);

        public record InventoryItemsGranted(Guid CorrelationId);
    public record SubstractItems(
        Guid UserId,
        Guid CatalogItemId, 
        int Quantity, 
        Guid CorrelationId);

        public record InventoryItemsSubstracted(Guid CorrelationId);

        public record InventoryItemUpdated(
            Guid UserId,
            Guid CatalogItemId,
            int NewTotalQuantity
        );

}
