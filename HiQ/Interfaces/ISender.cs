using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiQ.Interfaces
{
    public interface ISender : IDisposable
    {
        void Send<TMessageType>(TMessageType message);

        void DeleteQueue();
    }
}
