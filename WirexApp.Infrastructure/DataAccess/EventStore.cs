using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess
{
    public class EventStore : IEventStore

    {
        public IMediator Bus { get; set; }

        private struct EventDescriptor
        {
            public readonly DomainEventBase EventData;
            public readonly Guid Id;
            public readonly int Version;

            public EventDescriptor(Guid id, DomainEventBase eventData, int version)
            {
                EventData = eventData;
                Version = version;
                Id = id;
            }
        }

        public EventStore()
        {
        }

        private readonly IDictionary<Guid, List<EventDescriptor>> current = new ConcurrentDictionary<Guid, List<EventDescriptor>>();

        public void SaveEvents(Guid aggregateId, IEnumerable<DomainEventBase> events, int expectedVersion)
        {
            List<EventDescriptor> eventDescriptors;

            // try to get event descriptors list for given aggregate id
            // otherwise -> create empty dictionary
            if (!current.TryGetValue(aggregateId, out eventDescriptors))
            {
                eventDescriptors = new List<EventDescriptor>();
                current.Add(aggregateId, eventDescriptors);
            }
            // check whether latest event version matches current aggregate version
            // otherwise -> throw exception
            else if (eventDescriptors[eventDescriptors.Count - 1].Version != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;
                // push event to the event descriptors list for current aggregate
                eventDescriptors.Add(new EventDescriptor(aggregateId, @event, i));

                // publish current event to the bus for further processing by subscribers
                Bus.Publish(@event);
            }
        }

        public List<DomainEventBase> GetEventsForAggregate(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors;

            if (!current.TryGetValue(aggregateId, out eventDescriptors))
            {
                throw new AggregateNotFoundException();
            }

            return eventDescriptors.Select(desc => desc.EventData).ToList();
        }
    }

    public class AggregateNotFoundException : Exception
    {
    }

    public class ConcurrencyException : Exception
    {
    }
}
