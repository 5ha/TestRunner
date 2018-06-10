using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiQ.Interfaces
{
    public interface IDirectionSelector
    {
        ISenderBuilder ISendTo(string queueName);

        IReceiveTypeSelector IReceiveFrom(string queueName);
    }
}
