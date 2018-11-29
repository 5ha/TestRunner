using System;

namespace QueueService.Interfaces
{
    public interface IReceiveTypeSelector
    {
        IPermanentReceiverBuilder IReceiveForever();

        ITemporaryReceiverBuilder IReceiveUntilNoMoreMessages(TimeSpan startupTime, TimeSpan messageWaitTimeout, Action whenNoMoreMessages);
    }
}
