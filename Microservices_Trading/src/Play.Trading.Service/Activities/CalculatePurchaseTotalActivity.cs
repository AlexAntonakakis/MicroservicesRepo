using Play.Trading.Service.StateMachines;
using Automatonymous;
using System.Threading.Tasks;
using Play.Trading.Service.Contracts;
using Play.Trading.Service.Entities;
using Play.Trading.Service.Exceptions;
using GreenPipes;
using Common;
using System;

namespace  Play.Trading.Service.Activities
{
    public class CalculatePurchaseTotalActivity : Activity<PurchaseState, PurchaseRequested>
    {
        private readonly IRepository<CatalogItem> repository;

        public CalculatePurchaseTotalActivity(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }
        public async Task Execute(BehaviorContext<PurchaseState, PurchaseRequested> context, Behavior<PurchaseState, PurchaseRequested> next)
        {
            var message = context.Data;

            var item = await repository.GetAsync(message.ItemId);
            var items = await repository.GetAllAsync();

            if (item == null)
            {
                throw new UnknownItemException(message.ItemId);
            }

            context.Instance.PurchaseTotal = item.Price * message.Quantity;
            context.Instance.LastUpdated = DateTimeOffset.UtcNow;

            await next.Execute(context).ConfigureAwait(false);
        }


        public Task Faulted<TException>(BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, Behavior<PurchaseState, PurchaseRequested> next) where TException : System.Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("calculate-purchase-total");
        }

    }
}