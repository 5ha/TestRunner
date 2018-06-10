using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiQ.Interfaces
{
    public interface IQueueBuilder
    {
        IDirectionSelector ConfigureTransport(string hostName, string userName, string password);
    }
}
