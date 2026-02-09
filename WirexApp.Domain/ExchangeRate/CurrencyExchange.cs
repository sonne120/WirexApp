using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain.ExchangeRate
{
   public  class CurrencyExchange : ICurrencyExchange
    {
        public List<ConversionRate> GetConversionRates()
        {
            var conversionRates = new List<ConversionRate>();

            conversionRates.Add(new ConversionRate(Currency.USD, Currency.EUR, (decimal)0.85));
            conversionRates.Add(new ConversionRate(Currency.EUR, Currency.USD, (decimal)1.11));

            return conversionRates;
        }
    }
}
