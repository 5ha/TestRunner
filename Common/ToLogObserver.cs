using BuildManager.Model;
using log4net;
using MessageModels;
using System;
using System.Text;

namespace Common
{
    public class ToLogObserver : BaseObserver, ITestRunObserver
    {
        private ILog _log;

        public ToLogObserver(string logName)
        {
            _log = LogManager.GetLogger(logName);
        }

        public void OnCompleted()
        {
            _log.Info("Completed");
        }

        public void OnError(Exception error)
        {
            _log.Error(error.Message, error);
        }

        public void OnNext(TestExecutionResult value)
        {
            string testResult = value.Passed ? "PASSED" : "FAILED";
            _log.Info($"Observer: [{testResult}] {value.FullName}");
        }

        protected override void OutputMessage(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Message))
            {
                _log.Info($"{s.ToString()} : {mess.Message}");
            }
        }

        protected override void OutputWarning(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Warning))
            {
                _log.Warn($"{s.ToString()} WARNING: {mess.Warning}");
            }
        }

        protected override void OutputError(StringBuilder s, StatusMessage mess)
        {
            if (!String.IsNullOrEmpty(mess.Error))
            {
                _log.Error($"{s.ToString()} ERROR: {mess.Error}");
            }
        }
    }
}
