using Common;
using System;

namespace Play.Trading.Service.Entities
{
    public class ApplicationUser : IEntity
    {
        public Guid Id {get; set;}

        public decimal Gil {get; set;}
    }
}