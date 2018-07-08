using BuildManager;
using BuildManager.Model;
using Common;
using MessageModels;
using Model;
using Newtonsoft.Json;
using SocketProtocol;
using System;
using System.Threading.Tasks;

namespace SocketServer
{
    public class BuildRunner : ITestRunObserver
    {
        private readonly StringChannel _protocol;
        private readonly BuildRunRequest _request;

        public BuildRunner(StringChannel protocol, BuildRunRequest request)
        {
            this._protocol = protocol;
            this._request = request;
        }
        public void OnCompleted()
        {
            _protocol.SendAsync("DONE");
        }

        public void OnError(Exception error)
        {
            StatusMessage mess = new StatusMessage
            {
                Error = error.Message
            };

            _protocol.SendAsync(JsonConvert.SerializeObject(mess, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }

        public void OnNext(TestExecutionResult value)
        {
            _protocol.SendAsync(JsonConvert.SerializeObject(value,new JsonSerializerSettings {  TypeNameHandling = TypeNameHandling.All}));
        }

        public void OnNext(StatusMessage value)
        {
            _protocol.SendAsync(JsonConvert.SerializeObject(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }

        public Task StartBuild()
        {
            BuildController buildController = new BuildController();

            return buildController.KickOffBuild(_request, this);
        }

        
    }
}
