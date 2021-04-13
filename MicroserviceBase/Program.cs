using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace MicroserviceBase
{
    public class MicroserviceBaseClass
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        public enum LogNames
        {
            Critical = 1,
            Error = 2,
            Info = 3,
            Warning = 4
        }
        public static void RabbitMQPublisher()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://klcrkeba:JnfCJH6Kdo4H2rNQ9iM9GO-8u71VjOHv@coyote.rmq.cloudamqp.com/klcrkeba")
            };

            using IConnection connection = factory.CreateConnection();
            using (IModel channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("topic_exchange", durable: true, type: ExchangeType.Topic);

                Array log_name_array = Enum.GetValues(typeof(LogNames));

                for (int i = 1; i < 11; i++)
                {
                    Random rnd = new Random();

                    LogNames log1 = (LogNames)log_name_array.GetValue(rnd.Next(log_name_array.Length));
                    LogNames log2 = (LogNames)log_name_array.GetValue(rnd.Next(log_name_array.Length));
                    LogNames log3 = (LogNames)log_name_array.GetValue(rnd.Next(log_name_array.Length));

                    string RoutingKey = $"{log1}.{log2}.{log3}";

                    var bodyByte = Encoding.UTF8.GetBytes($"log={log1}-{log2}-{log3}");

                    var properties = channel.CreateBasicProperties();

                    properties.Persistent = true;

                    channel.BasicPublish("topic_exchange", routingKey: RoutingKey, properties, body: bodyByte);

                    Console.WriteLine($"log mesajı gönderilmiştir=> mesaj:{RoutingKey}");
                }
            }

            Console.WriteLine("İşlemler bitti. Çıkış yapmak tıklayınız..");
            Console.ReadLine();

        }
        public static void FireAndForget(string[] args)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://klcrkeba:JnfCJH6Kdo4H2rNQ9iM9GO-8u71VjOHv@coyote.rmq.cloudamqp.com/klcrkeba")
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("topic_exchange", durable: true, type: ExchangeType.Topic);

                    var queueName = channel.QueueDeclare().QueueName;

                    string routingKey = "#.Warning";

                    channel.QueueBind(queue: queueName, exchange: "topic_exchange", routingKey: routingKey);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, false);

                    Console.WriteLine("Custom log bekliyorum....");

                    var consumer = new EventingBasicConsumer(channel);

                    channel.BasicConsume(queueName, false, consumer);

                    consumer.Received += (model, ea) =>
                    {
                        var log = Encoding.UTF8.GetString(ea.Body.ToArray());
                        Console.WriteLine("log alındı:" + log);

                        int time = int.Parse(GetMessage(args));
                        Thread.Sleep(time);

                        File.AppendAllText("logs.txt", log + "\n");

                        Console.WriteLine("loglama bitti");

                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                    };
                    Console.WriteLine("Çıkış yapmak tıklayınız..");
                    Console.ReadLine();
                }
            }
        }
        private static string GetMessage(string[] args)
        {
            return args[0].ToString();
        }
        public static async void Call(string Url)
        {
            using HttpResponseMessage res = await new HttpClient().GetAsync(Url);
            using HttpContent content = res.Content;
            string data = await content.ReadAsStringAsync();
            if (data != null)
            {
                Console.WriteLine(data);
            }
        }
    }
}