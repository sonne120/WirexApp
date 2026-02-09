using System;
using Autofac;
using System.Collections.Generic;
using WirexApp.Domain.Payments;
using WirexApp.Domain.UserAccounts;
using WirexApp.Domain.User;
using WirexApp.Domain.BonusAccounts;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess
{
    public class DataAccessModule : Autofac.Module //  where T : DomainEventBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryPaymentRepository>()
                .As<IPaymentRepository>()
                .SingleInstance();

            builder.RegisterType<InMemoryUserAccountRepository>()
                .As<IUserAccountRepository>()
                .SingleInstance();

            builder.RegisterType<InMemoryUserRepository>()
                .As<IUserRepository>()
                .SingleInstance();

            builder.RegisterType<InMemoryBonusAccountRepository>()
                .As<IBonusAccountRepository>()
                .SingleInstance();

            builder.RegisterType<EventStore>()
                .As<IEventStore>()
                .SingleInstance();
        }
    }
}
