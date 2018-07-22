using RabbitMQ.Client;
using System;

namespace HiQ.Implementations
{
    public class BaseRabbitEndpoint : IDisposable
    {
        protected string _exchangeName;
        protected string _pathName;
        string _hostName;
        string _vhost;
        string _userName;
        string _password;

        public BaseRabbitEndpoint(string hostName, string vHost, string userName, string password, string exchangeName, string pathName)
        {
            _hostName = hostName;
            _vhost = vHost;
            _userName = userName;
            _password = password;
            _exchangeName = $"{exchangeName}_{Guid.NewGuid().ToString("N")}";
            _pathName = pathName;
        }

        private IConnection __connection;
        private IConnection _connection
        {
            get
            {
                if (__connection == null)
                {
                    __connection = new ConnectionFactory() { HostName = _hostName, UserName = _userName, Password = _password, VirtualHost = _vhost }
                    .CreateConnection();
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

        public void DeleteQueue()
        {
            Channel.QueueDelete(_pathName);
        }



        public void Dispose()
        {
            if (Channel != null)
            {
                if (Channel.IsOpen)
                    Channel.ExchangeDelete(_exchangeName);
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
