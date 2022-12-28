using Common;
using Inventory.Contracts;
using Inventory.Service.Entities;
using Inventory.Service.Exceptions;
using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Service.Consumers
{
    public class SubstractItemsConsumer : IConsumer<SubstractItems>
    {
        private readonly IRepository<InventoryItem> inventoryItemsRepository;
        private readonly IRepository<CatalogItem> catalogItemsRepository;

        public SubstractItemsConsumer(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository)
        {
            this.inventoryItemsRepository = inventoryItemsRepository;
            this.catalogItemsRepository = catalogItemsRepository;
        }

        public async Task Consume(ConsumeContext<SubstractItems> context)
        {
         var message = context.Message;

         var item = await catalogItemsRepository.GetAsync(message.CatalogItemId);

         if (item == null)
         {
            throw new UnknownItemException(message.CatalogItemId);
         }

         var inventoryItem = await inventoryItemsRepository.GetAsync( item => 
                item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);
                

            if (inventoryItem != null)
            {

                if (inventoryItem.MessageIds.Contains(context.MessageId.Value))
                {
                    await context.Publish(new InventoryItemsSubstracted(message.CorrelationId));
                    return;

                }

                inventoryItem.Quantity -= message.Quantity;
                inventoryItem.MessageIds.Add(context.MessageId.Value);

                await inventoryItemsRepository.UpdateAsync(inventoryItem);

                await context.Publish(new InventoryItemUpdated(
                    inventoryItem.UserId,
                    inventoryItem.CatalogItemId,
                    inventoryItem.Quantity
            ));

            }

            await context.Publish(new InventoryItemsSubstracted(message.CorrelationId));
       } 
    }
}