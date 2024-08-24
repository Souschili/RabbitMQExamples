using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ErrorConsumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory=new ConnectionFactory { HostName="localhost"};
            using var connection=factory.CreateConnection();
            using var channel = connection.CreateModel();

            // ?? why in example we do this 
            channel.ExchangeDeclare(exchange: "demo-direct",type: ExchangeType.Direct);

            //create and return uniq queue name from server 
            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(
                queue:queueName,
                exchange:"demo-direct",
                routingKey:"Error",
                arguments:null
                );


            var consumer = new EventingBasicConsumer(channel);
            // subscribe to event
            consumer.Received += (sender, e) =>
            {
                string message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine($"receive message: {message}");

                // Подтверждение сообщения после его обработки
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);

            };

            //subscribe to queue
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
