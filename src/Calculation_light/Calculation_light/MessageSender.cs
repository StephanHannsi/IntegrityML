using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Calculation
{
    public class MessageSender
    {
        private ConnectionFactory _Factory = null;
        private IModel _Channel = null;

        public MessageSender()
        {
            _Factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = _Factory.CreateConnection();
            _Channel = connection.CreateModel();
            _Channel.ExchangeDeclare(exchange: "integMessage", type: "topic");

            _Channel.QueueDeclare(queue: "integData", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _Channel.QueueBind(queue: "integData", exchange: "integMessage", routingKey: "stationRecord");
            _Channel.QueueBind(queue: "integData", exchange: "integMessage", routingKey: "observationRecord");
            _Channel.QueueBind(queue: "integData", exchange: "integMessage", routingKey: "roundEnd");
        }

        public void NewObservation(string roundId)
        {
            Publish(roundId,_Channel, "observationRecord");
        }

        private void Publish(string message, IModel channel, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _Channel.BasicPublish(exchange: "integMessage", routingKey: routingKey, basicProperties: null, body: body);
        }
    }
}
