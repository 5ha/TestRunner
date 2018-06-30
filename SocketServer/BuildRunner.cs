using BuildManager;
using BuildManager.Model;
using MessageModels;
using Model;
using SocketProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SocketServer
{
    public class BuildRunner : ITestRunObserver
    {
        private readonly StringChannel _protocol;
        private readonly string _build;

        public BuildRunner(StringChannel protocol, string build)
        {
            this._protocol = protocol;
            this._build = build;
        }
        public void OnCompleted()
        {
            _protocol.SendAsync("ALL COMPLETE");
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

            return buildController.KickOffBuild(_build, this);
        }
    }
}
