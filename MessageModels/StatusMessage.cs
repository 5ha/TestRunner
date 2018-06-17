using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModels
{
    public class StatusMessage
    {
        public bool IsError { get; set; }
        public Exception Error { get; set; }
        public string Message { get; set; }

        public StatusMessage()
        {

        }

        public StatusMessage(string message)
        {
            Message = message;
        }
    }
}
