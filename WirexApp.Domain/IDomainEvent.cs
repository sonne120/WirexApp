using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain
{
    public interface IDomainEvent : INotification
    {
        DateTime TimeStamp { get; }
    }
}
