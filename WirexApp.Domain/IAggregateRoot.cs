using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain
{
    public interface IAggregateRoot
    {
        public Guid Id { get; set; }
      
    }
}
