using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiQ.Interfaces
{
    public interface IReceiveTypeSelector
    {
        IPermanentReceiverBuilder IReceiveForever();

        ITemporaryReceiverBuilder IReceiveUntilNoMoreMessages(TimeSpan startupTime, TimeSpan messageWaitTimeout, Action whenNoMoreMessages);
    }
}
