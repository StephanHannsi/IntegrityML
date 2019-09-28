using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CalcSim
{
    public class MessageSender
    {
        private ConnectionFactory _Factory = null;
        private IModel _Channel = null;

        public MessageSender()
        {
            // Setting up the Connection to the MessageQueue
            _Factory = new ConnectionFactory() { HostName = "localhost" }; // change "localhost" to something else if the routing is changed on this machine
            var connection = _Factory.CreateConnection();
            _Channel = connection.CreateModel();
            _Channel.ExchangeDeclare(exchange: "roundMessage", type: "topic");

            _Channel.QueueDeclare(queue: "roundData", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _Channel.QueueBind(queue: "roundData", exchange: "roundMessage", routingKey: "stationRecord");
            _Channel.QueueBind(queue: "roundData", exchange: "roundMessage", routingKey: "observationRecord");
            _Channel.QueueBind(queue: "roundData", exchange: "roundMessage", routingKey: "roundEnd");
        }

        public void SendStationRecord(TerrestrialRoundMessage roundMessage)
        {
            string message = JsonConvert.SerializeObject(roundMessage);
            publish(message, _Channel, "stationRecord");
        }

        public void SendObservationRecord(TerrestrialRoundMessage roundMessage)
        {
            string message = JsonConvert.SerializeObject(roundMessage);
            publish(message, _Channel, "observationRecord");
        }

        public void SendRoundEnd(TerrestrialRoundMessage roundMessage)
        {
            string message = JsonConvert.SerializeObject(roundMessage);
            publish(message, _Channel, "roundEnd");
        }

        private void publish(string message, IModel channel, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _Channel.BasicPublish(exchange: "roundMessage", routingKey: routingKey, basicProperties: null, body: body);
            Console.WriteLine("Message sent: " + routingKey);
        }
    }
}
