using BuildManager;
using BuildManager.Model;
using Common;
using log4net;
using MessageModels;
using Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace SocketServer
{
    public class BuildRunner : ITestRunObserver
    {
        ILog _log = LogManager.GetLogger("SocketServer.Buildrunner");

        private Action<string> _send;
        private readonly BuildRunRequest _request;

        public BuildRunner(BuildRunRequest request, Action<string> send)
        {
            _send = send;
            this._request = request;
        }
        public void OnCompleted()
        {
            _send("DONE");
        }

        public void OnError(Exception error)
        {
            try
            {
                StatusMessage mess = new StatusMessage
                {
                    Error = error.Message
                };

                _send(JsonConvert.SerializeObject(mess, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
            }
            catch (Exception e)
            {
                NotifySocketServerException(e);
            }
        }

        public void OnNext(TestExecutionResult value)
        {
            try
            {
                _send(JsonConvert.SerializeObject(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
            }
            catch (Exception e)
            {
                NotifySocketServerException(e);
            }
        }

        public void OnNext(StatusMessage value)
        {
            try
            {
                _send(JsonConvert.SerializeObject(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
            }
            catch (Exception e)
            {
                NotifySocketServerException(e);
            }
        }

        private void NotifySocketServerException(Exception e)
        {
            string err = $"Socket server choked: {e.Message} {e.StackTrace}";
            Console.WriteLine(err);
            _log.Error("SocketProtocol Server Choked", e);

        }

        public Task StartBuild()
        {
            BuildController buildController = new BuildController();

            return buildController.KickOffBuild(_request, this);
        }


    }
}
