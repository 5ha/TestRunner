using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportService
{
    public class ConnectorBase : IDisposable
    {
        string _exchangeName;
        string _pathName;

        public ConnectorBase(string exchangeName, string pathName)
        {
            _exchangeName = exchangeName;
            _pathName = pathName;
        }

        private IConnection __connection;
        private IConnection _connection
        {
            get
            {
                if (__connection == null)
                {
                    __connection = new ConnectionFactory() { HostName = "my-rabbit", UserName="remote", Password="remote" }.CreateConnection();
                }

                return __connection;
            }

            set
            {
                __connection = value;
            }
        }

        private IModel __channel;
        public IModel Channel
        {
            get
            {
                if (__channel == null)
                {
                    __channel = _connection.CreateModel();
                    __channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");
                    __channel.QueueDeclare(queue: _pathName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                    __channel.QueueBind(queue: _pathName, exchange: _exchangeName, routingKey: _pathName);
                }

                return __channel;
            }
            set
            {
                __channel = value;
            }
        }

        public void Send<TMessageType>(TMessageType message)
        {
            string serialisedMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialisedMessage);
            Channel.BasicPublish(exchange: _exchangeName,
                                 routingKey: _pathName,
                                 basicProperties: null,
                                 body: body);
        }

        public void Dispose()
        {
            if (Channel != null)
            {
                if (Channel.IsOpen)
                    Channel.Close();

                Channel = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }

    }
}
