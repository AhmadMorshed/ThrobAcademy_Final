using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Service.Interfaces.Payment;

namespace Throb.Service.Services.Payment
{
    public class CashPaymentStrategy : IPaymentStrategy
    {
        public async Task<string> ExecutePaymentAsync(decimal amount, int studentId)
        {
            return await Task.FromResult("Cash_Success");
        }
    }
}
