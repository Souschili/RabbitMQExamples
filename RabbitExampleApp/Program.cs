using RabbitMQ.Client;
using System;
using System.Text;

namespace RabbitExampleApp
{
    // простое создание обменика и канала на локал хосте раббитМг
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();


            // Создание обменника типа 'fanout'
            string exchangeName = "my_fanout_exchange";
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);

            // Создание очереди
            string queueName = "my_queue";
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


            // Привязка очереди к обменнику
            channel.QueueBind(queue: queueName,
                              exchange: exchangeName,
                              routingKey: "");

            // Отправка сообщения
            string message = "Hello, from LocalHost!";
            var body = Encoding.UTF8.GetBytes(message);

            // отправка сообщения
            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "", // Для 'fanout' тип рутинговый ключ не важен
                                 basicProperties: null,
                                 body: body);

            // показываем сообщение
            Console.WriteLine($" [x] Sent '{message}' to exchange '{exchangeName}'");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
