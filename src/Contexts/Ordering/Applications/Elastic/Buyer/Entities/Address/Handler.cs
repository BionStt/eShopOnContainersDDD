﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aggregates;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Queries;
using NServiceBus;

namespace eShop.Ordering.Buyer.Entities.Address
{
    public class Handler :
        IHandleQueries<Queries.Addresses>,
        IHandleMessages<Events.Added>,
        IHandleMessages<Events.Removed>
    {
        public async Task Handle(Queries.Addresses query, IMessageHandlerContext ctx)
        {
            if(query.Id.HasValue)
            {
                var result = await ctx.App<Infrastructure.IUnitOfWork>().Get<Models.Address>(query.Id.Value).ConfigureAwait(false);

                await ctx.Result(new[] { result }, 1, 0).ConfigureAwait(false);
                return;
            }

            var builder = new QueryBuilder();
            builder.Add("UserName", query.UserName.ToString(), Operation.EQUAL);
            if (!string.IsNullOrEmpty(query.Term))
            {
                var group = builder.Grouped(Group.ANY);
                group.Add("Street", query.Term, Operation.CONTAINS);
                group.Add("City", query.Term, Operation.CONTAINS);
                group.Add("State", query.Term, Operation.CONTAINS);
                group.Add("Alias", query.Term, Operation.CONTAINS);
            }

            var results = await ctx.App<Infrastructure.IUnitOfWork>().Query<Models.Address>(builder.Build())
                .ConfigureAwait(false);

            await ctx.Result(results.Records, results.Total, results.ElapsedMs).ConfigureAwait(false);
        }

        public Task Handle(Events.Added e, IMessageHandlerContext ctx)
        {
            var model = new Models.Address
            {
                Id = e.AddressId,
                UserName = e.UserName,
                Alias = e.Alias,
                City = e.City,
                State = e.State,
                Country = e.Country,
                Street = e.Street,
                ZipCode = e.ZipCode
            };

            return ctx.App<Infrastructure.IUnitOfWork>().Add(e.AddressId, model);
        }

        public Task Handle(Events.Removed e, IMessageHandlerContext ctx)
        {
            return ctx.App<Infrastructure.IUnitOfWork>().Delete<Models.Address>(e.AddressId);
        }
    }
}
