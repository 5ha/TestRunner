using System;

namespace QueueService.Interfaces
{
    public interface ISender : IDisposable
    {
        void Send<TMessageType>(TMessageType message);

        void DeleteQueue();
    }
}
