using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace InfoCosumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // declare exchange in real code it must declared
            // Consmer should not declare the exchange (обменник должен быть уже создан)
            channel.ExchangeDeclare(exchange: "demo-direct", type: ExchangeType.Direct);

            // create queue and return uniq name
            // Consumer should not create queues (очередь должна быть уже создана)
            var queueName = channel.QueueDeclare().QueueName;

            // bind channels
            // Consumer may bind to the queue, but ideally,
            // this should be pre-configured (привязка очереди к обменнику также может быть предварительно настроена)
            channel.QueueBind(
                queue: queueName,
                exchange: "demo-direct",
                routingKey: "Info",
                arguments: null
                );

            channel.QueueBind(
                queue: queueName,
                exchange: "demo-direct",
                routingKey: "Error",
                arguments: null
                );
            channel.QueueBind(
                queue: queueName,
                exchange: "demo-direct",
                routingKey: "Log",
                arguments: null
                );

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, e) =>
            {
                //преоразуем массив айтов в сообщение
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                // вывод полученого сообщения
                Console.WriteLine($"Received message: {message}");

                // Подтверждение сообщения после его обработки
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            };

            // подписка на очередь
            channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer
                );


            Console.WriteLine("Subsribed to demo-queue");
            await Task.Delay(Timeout.Infinite);

        }
    }
}
