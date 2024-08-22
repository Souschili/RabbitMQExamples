using RabbitMQ.Client;
using System.Text;

namespace PublisherRabbitApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection=factory.CreateConnection();
            using var channel = connection.CreateModel();

            // создаем очередь 
            string queueName = "demo-queue";
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);



            // подготовка сообщения 
            string message = "Hello from local host";
            var body=Encoding.UTF8.GetBytes(message);

            // так как мы не прикрутили очередь  неуказали роутингКей
            // то все сообщения будут попадать в дефаултЭксчейндж
            // который неявно связан со всеми очередями по имени в роутингкей
            channel.BasicPublish(exchange: "",  // имя обменика неуказанно
                                 routingKey: queueName, // путь как имя очереди сообщение попадет в нее
                                 basicProperties: null,
                                 body: body);


            Console.WriteLine($" [x] Sent '{message}' to queue '{queueName}' using default exchange");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
