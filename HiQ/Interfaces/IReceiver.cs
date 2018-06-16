using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiQ.Interfaces
{
    public interface IReceiver : IDisposable
    {
        void Receive<TMessageType>(Action<TMessageType> onReceived);

        void DeleteQueue();
    }
}
