using RabbitMQ.Client;
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
            Console.WriteLine("Press any key to stop all tasks");
            Console.ReadLine();

            // Stop
            ctc.Cancel();

            // Wait for all tasks to complete
            try
            {
                await Task.WhenAll(task1, task2, task3);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Tasks were canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static Func<Task> CreateTask(int sleepInterval, string routeKey, CancellationToken token)
        {
            return async () =>
            {
                int count = 0;
                var factory = new ConnectionFactory { HostName = "localhost" };

                try
                {
                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    channel.ExchangeDeclare(exchange: "demo-direct", type: ExchangeType.Direct);

                    while (!token.IsCancellationRequested)
                    {
                        var interval = Random.Value.Next(1000, sleepInterval);
                        await Task.Delay(interval, token);  // Pass the cancellation token

                        count++;
                        string message = $"Message type[ {routeKey} ] from publisher N'{count}'";
                        byte[] body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(
                            exchange: "demo-direct",
                            routingKey: routeKey,
                            basicProperties: null,
                            body: body
                        );

                        Console.WriteLine($"Message type [ {routeKey} ] sent to direct exchange N'{count}'");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Task for route '{routeKey}' was canceled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred in task for route '{routeKey}': {ex.Message}");
                }
            };
        }
    }
}
