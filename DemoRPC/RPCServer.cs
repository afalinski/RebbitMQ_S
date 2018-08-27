using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DemoRPC
{
    internal class RpcServer
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, Password = "guest", UserName = "guest" };
            using (var connection = factory.CreateConnection())
            using (var chanel = connection.CreateModel())
            {
                chanel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false,
                    arguments: null);
                chanel.BasicQos(0,1,false);
                var consumer = new EventingBasicConsumer(chanel);
                chanel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = chanel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        int n = int.Parse(message);
                        Console.WriteLine($" [.] fib({message})");
                        response = Fib(n).ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        chanel.BasicPublish(exchange:"", routingKey:props.ReplyTo, basicProperties:replyProps, body:responseBytes);
                        chanel.BasicAck(deliveryTag:ea.DeliveryTag, multiple:false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

        }

        private static int Fib(int n)
        {
            if (n == 0 || n == 1) return n;

            return Fib(n - 1) + Fib(n - 2);
        }
    }
}
