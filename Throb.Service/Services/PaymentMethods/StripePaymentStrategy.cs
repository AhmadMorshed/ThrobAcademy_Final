using Microsoft.Extensions.Configuration;
using Stripe; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Throb.Service.Interfaces.Payment;

namespace Throb.Service.Services.Payment
{
    public class StripePaymentStrategy : IPaymentStrategy
    {
        private readonly IConfiguration _configuration;

        public StripePaymentStrategy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> ExecutePaymentAsync(decimal amount, int studentId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            };
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);
            return intent.ClientSecret; 
        }
    }
}
