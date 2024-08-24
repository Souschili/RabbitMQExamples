using RabbitMQ.Client;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProducerApp
{
    internal class Program
    {
        // thread safety
        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        static async Task Main(string[] args)
        {
            var ctc = new CancellationTokenSource();

            var task1 = Task.Run(CreateTask(25000, "error", ctc.Token));
            var task2 = Task.Run(CreateTask(18000, "log", ctc.Token));
            var task3 = Task.Run(CreateTask(11000, "Info", ctc.Token));
            Console.WriteLine("Press any key to stop all task");
            Console.ReadLine();

            // stop
            ctc.Cancel();

            // correct stop all task 
            await Task.WhenAll(task1, task2, task3);


        }


        static Func<Task> CreateTask(int sleepinterval, string routeKey, CancellationToken tokenSource)
        {
            return async () =>
            {
                int count = 0;
                var factory = new ConnectionFactory { HostName = "localhost" };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: "demo-direct", type: ExchangeType.Direct);

                // тут в цикле отправка сообщений
                do
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        Console.WriteLine($"Cancellation requested for {routeKey}. Exiting...");
                        break; // Выходим из цикла и задача завершает выполнение
                    }

                    var interval = Random.Value.Next(1000,sleepinterval);
                    await Task.Delay(interval);

                    count++;
                    string message = $"Message type[ {routeKey} ] from publishe N'{count}'";
                    byte[] body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(
                        exchange: "demo-direct",
                        routingKey: routeKey,
                        basicProperties: null,
                        body: body
                        );

                    Console.WriteLine($"Message type [ {routeKey} ] sent to direct exchange N'{count}");

                } while (true);

            };
        }
    }
}
