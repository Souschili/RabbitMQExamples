using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace ConsumerRabbitApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var client = factory.CreateConnection();
            using var channel = client.CreateModel();

            // if we didn't has queue it create new one
            channel.QueueDeclare(queue: "demo-queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

            // create consumer
            var consumer = new EventingBasicConsumer(channel);

            // subscribe to event
            consumer.Received += (sender, e) =>
            {
                string message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine($"receive message: {message}");
            };

            //subscribe to queue
            channel.BasicConsume(
                queue: "demo-queue",
                autoAck: false,
                consumer: consumer
                );

            Console.WriteLine("Subsribed to demo-queue");
        }
    }
}
