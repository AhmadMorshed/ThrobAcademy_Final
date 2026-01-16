using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Service.Interfaces.Payment;

namespace Throb.Service.Services.Payment
{


    public class PaymentProvider : IPaymentProvider
    {
        private readonly IEnumerable<IPaymentStrategy> _strategies;

        public PaymentProvider(IEnumerable<IPaymentStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IPaymentStrategy GetStrategy(string method)
        {
            return method switch
            {
                "Card" => _strategies.OfType<StripePaymentStrategy>().FirstOrDefault(),
                "Cash" => _strategies.OfType<CashPaymentStrategy>().FirstOrDefault(),
                _ => throw new Exception("طريقة دفع غير مدعومة")
            };
        }
    }
}
