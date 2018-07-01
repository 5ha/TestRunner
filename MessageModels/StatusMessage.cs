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

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append($"[{Machine} - {Application}");
            if (!string.IsNullOrEmpty(Process)) s.Append($" - {Process}");
            if (!string.IsNullOrEmpty(Message)) s.Append($"INFO - {Message}");
            if (!string.IsNullOrEmpty(Warning)) s.Append($"WARNING - {Warning}");
            if (!string.IsNullOrEmpty(Error)) s.Append($"INFO - {Error}");
            return s.ToString();
        }
    }
}
