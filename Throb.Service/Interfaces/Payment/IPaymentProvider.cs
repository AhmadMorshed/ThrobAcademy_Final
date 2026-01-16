using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Service.Interfaces.Payment
{
    public interface IPaymentProvider
    {
        IPaymentStrategy GetStrategy(string method);
    }
}
