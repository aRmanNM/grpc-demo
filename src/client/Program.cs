using client;
using Grpc.Net.Client;
using Spectre.Console;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var httpHandler = new HttpClientHandler();
        httpHandler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel.ForAddress("https://localhost:7220",
            new GrpcChannelOptions { HttpHandler = httpHandler });

        var client = new Demo.DemoClient(channel);

        var header = args.Length >= 1 ? args[0] : "GrpcClient";
        AnsiConsole.Write(new FigletText(header));

        while (true)
        {
            Console.WriteLine("Specify server action:");
            Console.WriteLine("0 - say hello");
            Console.WriteLine("exit - Exit the program");

            var action = Console.ReadLine();

            if (action == "exit")
                break;

            switch (action)
            {
                case "0":
                    await SayHello(client);
                    break;
                default:
                    Console.WriteLine("Invalid action specified");
                    break;
            }

            if (action == "0")
            {

            }
        }
    }

    private static async Task SayHello(Demo.DemoClient client)
    {
        var reply = await client.SayHelloAsync(
                    new HelloRequest { Name = "console_client" });

        Console.WriteLine("\n ----- \nServer response is: {0}\n ----- \n", reply.Message);
    }
}