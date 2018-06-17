using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageModels
{
    public class StatusMessage
    {
        public string Machine { get; set; }
        public string Application { get; set; }
        public string Process { get; set; }
        public string Message { get; set; }
        public string Warning { get; set; }
        public string Error { get; set; }
    }
}
