using System;
using System.Collections.Generic;
using System.Linq;

namespace WirexApp.Domain
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        private int _version = -1;

        public Guid Id { get; set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public int Version => _version;

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void MarkChangesAsCommitted()
        {
            _version++;
            ClearDomainEvents();
        }

        protected void LoadsFromHistory(IEnumerable<IDomainEvent> history)
        {
            foreach (var @event in history)
            {
                ApplyEvent(@event, false);
                _version++;
            }
        }

        protected virtual void ApplyEvent(IDomainEvent @event, bool isNew = true)
        {
            this.AsDynamic().Apply(@event);

            if (isNew)
            {
                AddDomainEvent(@event);
            }
        }
    }

    internal static class AggregateRootExtensions
    {
        public static dynamic AsDynamic(this object obj)
        {
            return obj;
        }
    }
}
