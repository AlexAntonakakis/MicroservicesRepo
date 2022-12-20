using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Service.Dtos;
using Catalog.Service.Entities;
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Catalog.Contracts;
using Common;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Service.Controllers{

    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;

        public ItemsController (IRepository<Item> itemsRepository, IPublishEndpoint publicshEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publicshEndpoint;
        }

        [HttpGet]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());
            return Ok(items);
        }

        [HttpGet("{id}")]
        [Authorize(Policies.Read)]

        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);
            if  (item == null)
            {
                return NotFound();
            }
            return item.AsDto();
        }

        
        [HttpPost]
        [Authorize(Policies.Write)]

        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {

            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTime.Now
            };

            await itemsRepository.CreateAsync(item);

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new {id = item.Id}, item);
        }

        [HttpPut("{id}")]
        [Authorize(Policies.Write)]

        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsync(id);
            if  (existingItem == null)
            {
                return NotFound();
            }
           
            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;
            
            await itemsRepository.UpdateAsync(existingItem);

            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));


            return NoContent();
        }

        // DELETE /item/{id}
        [HttpDelete("{id}")]
        [Authorize(Policies.Write)]

        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);
            if  (item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(item.Id);
            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
        
    }
}