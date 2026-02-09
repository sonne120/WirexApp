using System;
using System.Collections.Generic;
using MediatR;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess
{
    //https://github.com/gregoryyoung/m-r/blob/master/SimpleCQRS/Events.cs
    public interface IEventStore
    {
        IMediator Bus { get; set; }
        void SaveEvents(Guid aggregateId, IEnumerable<DomainEventBase> events, int expectedVersion);
        List<DomainEventBase> GetEventsForAggregate(Guid aggregateId);
    }
}
