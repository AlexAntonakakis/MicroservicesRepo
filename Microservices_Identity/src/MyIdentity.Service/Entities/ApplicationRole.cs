using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MyIdentity.Service.Entities
{
    [CollectionName("Roles")]
    public class ApplicationRole: MongoIdentityRole<Guid>
    {

    }
}