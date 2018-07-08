using MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public abstract class BaseObserver 
    {
        public void OnNext(StatusMessage value)
        {
            StringBuilder s = new StringBuilder();
            ComposePrefix(s, value);
            OutputMessage(s, value);
            OutputWarning(s, value);
            OutputError(s, value);
        }

        protected abstract void OutputMessage(StringBuilder s, StatusMessage mess);
        protected abstract void OutputWarning(StringBuilder s, StatusMessage mess);
        protected abstract void OutputError(StringBuilder s, StatusMessage mess);



        protected void ComposePrefix(StringBuilder s, StatusMessage mess)
        {
            s.Append($"Observer:");
            if (!string.IsNullOrEmpty(mess.Machine)) s.Append($"[{mess.Machine}]");
            if (!string.IsNullOrEmpty(mess.Application)) s.Append($"[{mess.Application}]");
            if (!string.IsNullOrEmpty(mess.Process)) s.Append($"[{mess.Process}]");
        }
    }
}
