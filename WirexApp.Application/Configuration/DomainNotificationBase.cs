using System;
using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;

namespace WirexApp.Application.Configuration
{
    public class DomainNotificationBase<T> : IDomainEventNotification<T>, INotification
    {      
        public T DomainEvent { get; }

        public DomainNotificationBase(T domainEvent)
        {
            this.DomainEvent = domainEvent;
        }
    }
}
