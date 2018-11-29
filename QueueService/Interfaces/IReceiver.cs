using System;

namespace QueueService.Interfaces
{
    public interface IReceiver : IDisposable
    {
        void Receive<TMessageType>(Action<TMessageType> onReceived);

        void DeleteQueue();
    }
}
