using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace DemoRebbitMQ_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost", Port = 5672, Password = "guest", UserName = "guest"};
            using (var connection = factory.CreateConnection())
            {
                using (var chanel = connection.CreateModel())
                {
                    
                    chanel.ExchangeDeclare(exchange: "topic_logs", type:"topic");

                    var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";

                    var message = GetMessage(args);

                    var body = Encoding.UTF8.GetBytes(message);
                  
                    chanel.BasicPublish(exchange: "topic_logs", routingKey: routingKey, basicProperties:null, body:body);

                    Console.WriteLine($" [x] Sent {routingKey}:{message}");
                    Console.ReadLine();
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return args.Length > 1
                ? string.Join(" ", args.Skip(1).ToArray())
                : "Hello World!";
        }
    }
}
