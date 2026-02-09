using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain.ExchangeRate
{ 
   public interface ICurrencyExchange
    {
        List<ConversionRate> GetConversionRates();
    }
}
