using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIdentity.Service.Dtos;
using MyIdentity.Service.Entities;
using static Duende.IdentityServer.IdentityServerConstants;
using MassTransit;
using MyIdentity.Contracts;

namespace MyIdentity.Service.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize(Policy = LocalApi.PolicyName, Roles = Roles.Admin)] // policy for securing API that lives in Identity server
    public class UsersController: ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;

        private readonly IPublishEndpoint publishEndpoint;

        public UsersController(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint)
        {
            this.userManager = userManager;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get()
        {
            var users = userManager.Users
                .ToList()
                .Select(user => user.AsDto());
                
                return Ok(users);
        }   
        
        // /users/{123}
         [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            
            return user.AsDto();
        }  
        
         // /users/{123}
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(Guid id, UpdateUserDto userDto)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            
            user.Email = userDto.Email;
            user.UserName = userDto.Email;
            user.Gil = userDto.Gil;

            await userManager.UpdateAsync(user);

            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

            return NoContent();
        }

        // /users/{123}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
    
            await userManager.DeleteAsync(user);

            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, 0));


            return NoContent();
        }

    }
}