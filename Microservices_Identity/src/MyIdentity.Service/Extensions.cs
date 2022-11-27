using MyIdentity.Service.Dtos;
using MyIdentity.Service.Entities;

namespace MyIdentity.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this ApplicationUser user)
        {
            return new UserDto(
                user.Id,
                user.UserName,
                user.Email,
                user.Gil,
                user.CreatedOn);
        }
    }
}