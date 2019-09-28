using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;
using Calculation.Models;
using System.Threading.Tasks;

namespace Calculation
{
    public class MessageHandler
    {
        private ConnectionFactory _Factory = null;
        private IModel _Channel = null;
        private MessageSender _MessageSender = null;
        private LoadSaveRepo _LoadSaveRepo = new LoadSaveRepo();
        private Processing _Processing = new Processing();
        private bool _Polling = false;

        public MessageHandler(MessageSender messageSender)
        {
            _MessageSender = messageSender;
            _Factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = _Factory.CreateConnection();
            Console.WriteLine("Established connection");
            _Channel = connection.CreateModel();
            _Channel.ExchangeDeclare(exchange: "roundMessage", type: "topic");
            _Channel.QueueDeclare(queue: "roundData", durable: false, exclusive: false, autoDelete: false, arguments: null);
            Console.WriteLine("created queue");
            _Channel.QueueBind(queue: "roundData", exchange: "roundMessage", routingKey: "stationRecord");
            _Channel.QueueBind(queue: "roundData", exchange: "roundMessage", routingKey: "observationRecord");
            _Channel.QueueBind(queue: "roundData", exchange: "roundMessage", routingKey: "roundEnd");
            Console.WriteLine("subscribed to queue");
            _Polling = true;
            PollMessages();
            Console.WriteLine("Polling messages ...");
            Console.ReadLine();
        }

        private async void PollMessages()
        {
            while(_Polling)
            {
                var lub = _Channel.BasicGet(queue: "roundData", autoAck: false);
                if (lub != null)
                {
                    var body = lub.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = lub.RoutingKey;
                    switch (routingKey)
                    {
                        case "stationRecord":
                            await HandleStationRecord(message);
                            break;
                        case "observationRecord":
                            await HandleObservationRecord(message);
                            break;
                        case "roundEnd":
                            await HandleRoundEnd(message);
                            break;
                        default:
                            Console.WriteLine("Unknown Key: " + routingKey);
                            break;
                    }
                    _Channel.BasicAck(lub.DeliveryTag, false);
                }
            }
        }

        public async Task<bool>HandleStationRecord(string message)
        {
            Console.WriteLine("got StationRecord");
            var messageObj = JsonConvert.DeserializeObject<TerrestrialRoundMessage>(message);
            messageObj.StationId = CalcStationId(messageObj.DeviceId, messageObj.Station.PointId);
            var saveResult = await _LoadSaveRepo.SaveStationRecordAsync(messageObj);
            await _Processing.ProcessStationRecord(messageObj.StationId, saveResult);
            return true;
        }

        public async Task<bool>HandleObservationRecord(string message)
        {
            Console.WriteLine("got ObservationRecord");
            var messageObj = JsonConvert.DeserializeObject<TerrestrialRoundMessage>(message);
            var result = await _LoadSaveRepo.SaveObservationAsync(messageObj);
            await _Processing.ProcessNewObs(messageObj.RoundId);
            return true;
        }

        public async Task<bool> HandleRoundEnd(string message)
        {
            Console.WriteLine("got RoundEnd");
            var messageObj = JsonConvert.DeserializeObject<TerrestrialRoundMessage>(message);
            var result = await _LoadSaveRepo.SaveRoundEndAsync(messageObj);
            await _Processing.ProcessRoundEnd(messageObj.RoundId);
            return true;
        }

        private string CalcStationId(string deviceId, string pointId)
        {
            var stationId = deviceId + pointId;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(stationId)).Trim('=');
        }
    }
}
