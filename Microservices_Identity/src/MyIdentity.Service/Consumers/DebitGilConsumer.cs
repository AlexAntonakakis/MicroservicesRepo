using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using MyIdentity.Contracts;
using MyIdentity.Service.Entities;
using MyIdentity.Service.Exceptions;

namespace MyIdentity.Service.Consumers
{
    public class DebitGilCOnsumer: IConsumer<DebitGil>
    {
        private readonly UserManager<ApplicationUser> userManager;

        public DebitGilCOnsumer(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task Consume(ConsumeContext<DebitGil> context)
        {
            var message = context.Message;

            var user = await userManager.FindByIdAsync(message.UserId.ToString());

            if (user == null)
            {
                throw new UnknownUserException(message.UserId);
            }

            if (user.MessageIds.Contains(context.MessageId.Value))
            {
                await context.Publish(new GilDebited(message.CorrelationId));
                return;
            }

            user.Gil -= message.Gil;

            if (user.Gil < 0)
            {
                throw new InsufficientFundsException(message.UserId, message.Gil);
            }

            user.MessageIds.Add(context.MessageId.Value);

            await userManager.UpdateAsync(user);

            var gilDebitedTask = context.Publish(new GilDebited(message.CorrelationId));

            var userUpdatedTask = context.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

            await Task.WhenAll(userUpdatedTask, gilDebitedTask);


        }

        
    }
}