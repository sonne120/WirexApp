using System;
using System.Collections.Generic;
using System.Text;
using WirexApp.Domain.UserAccounts;

namespace WirexApp.Domain
{
    public class DomainEventBase : IDomainEvent
    {
        public DomainEventBase()
        {
            this.TimeStamp = DateTime.Now;
            this.id = Guid.NewGuid();           
        }

        public int Version;
        public DateTime TimeStamp { get; }
        public Guid id;     
    }
}
