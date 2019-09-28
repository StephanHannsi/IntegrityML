using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace InterpretationML
{
    public class MessageHandler
    {
        private ConnectionFactory _Factory = null;
        private IModel _Channel = null;
        private Processing _Processing = new Processing();
        private bool _Polling = false;

        public MessageHandler()
        {
            _Factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = _Factory.CreateConnection();
            Console.WriteLine("Established connection");
            _Channel = connection.CreateModel();
            _Channel.ExchangeDeclare(exchange: "integMessage", type: "topic");
            _Channel.QueueDeclare(queue: "integData", durable: false, exclusive: false, autoDelete: false, arguments: null);
            Console.WriteLine("created queue");
            _Channel.QueueBind(queue: "integData", exchange: "integMessage", routingKey: "stationRecord");
            _Channel.QueueBind(queue: "integData", exchange: "integMessage", routingKey: "observationRecord");
            _Channel.QueueBind(queue: "integData", exchange: "integMessage", routingKey: "roundEnd");

            Console.WriteLine("subscribed to queue");
            _Polling = true;
            PollMessages();
            Console.WriteLine("Polling messages ...");
            Console.ReadLine();
        }

        private async void PollMessages()
        {
            while (_Polling)
            {
                var lub = _Channel.BasicGet(queue: "integData", autoAck: false);
                if (lub != null)
                {
                    var body = lub.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = lub.RoutingKey;
                    switch (routingKey)
                    {
                        case "stationRecord":
                            Console.WriteLine(HandleStationRecord(message));
                            break;
                        case "observationRecord":
                            await _Processing.HandleObservationRecord(message);
                            break;
                        case "roundEnd":
                            Console.WriteLine(HandleRoundEnd(message));
                            break;
                        default:
                            Console.WriteLine("Unknown Key: " + routingKey);
                            break;
                    }
                    _Channel.BasicAck(lub.DeliveryTag, false);
                }
            }
        }

        public string HandleStationRecord(string message)
        {
            return ("StationRecord");
        }

        public string HandleRoundEnd(string message)
        {
            return ("RoundEnd");
        }
    }
}
