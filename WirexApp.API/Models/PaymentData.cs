using WirexApp.Domain;

namespace WirexApp.API.Models
{
    public class PaymentData
    {
        public Currency sourceCurrency { get; set; }

        public Currency targetCurrency { get; set; }

        public decimal sourceValue { get; set; }
    }
}
