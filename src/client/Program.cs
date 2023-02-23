using client;
using Grpc.Core;
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
            Console.WriteLine("1 - server streaming counter");
            Console.WriteLine("2 - client streaming summation");
            Console.WriteLine("3 - bidirectional ping/ping");
            Console.WriteLine("exit - Exit the program");

            var action = Console.ReadLine();

            if (action == "exit")
                break;

            switch (action)
            {
                case "0":
                    await SayHello(client);
                    break;
                case "1":
                    await StreamServerCounter(client);
                    break;
                case "2":
                    await StreamClientSummation(client);
                    break;
                case "3":
                    await StreamBidirectionalPingPong(client);
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

    private static async Task StreamBidirectionalPingPong(Demo.DemoClient client)
    {
        Console.Write("iterations: ");
        int.TryParse(Console.ReadLine(), out int iterations);

        Console.Write("\n ----- \nResult:\t");

        using var call = client.StreamBothWays();

        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.Write("{0}\t", response.Message);
            }
        });

        int counter = 0;
        string msg = "Ping";

        while (counter < iterations)
        {
            Console.Write("{0}\t", msg);
            await call.RequestStream.WriteAsync(new PingRequest { Message = msg });
            await Task.Delay(TimeSpan.FromSeconds(1));
            counter++;
        }

        await call.RequestStream.CompleteAsync();
        await readTask;

        Console.WriteLine("\n ----- \n");
    }

    private static async Task StreamClientSummation(Demo.DemoClient client)
    {
        Console.Write("numbers (seperated by space): ");
        var numbers = Console.ReadLine()?
            .Split(",")
            .Select(x => int.Parse(x)).ToList() ?? new List<int>() { 1, 2, 3 };

        using var call = client.StreamFromClient();

        foreach (var number in numbers)
            await call.RequestStream.WriteAsync(new NumbersRequest { Number = number });

        await call.RequestStream.CompleteAsync();

        var response = await call;
        Console.WriteLine("\n ----- \nServer response is: sum equals {0} and numbers are {1}\n ----- \n", response.Sum, response.Numbers);
    }

    private static async Task StreamServerCounter(Demo.DemoClient client)
    {
        int start = 1;
        int finish = 5;

        Console.Write("start: ");
        int.TryParse(Console.ReadLine(), out start);

        Console.Write("finish: ");
        int.TryParse(Console.ReadLine(), out finish);

        using var call = client.StreamFromServer(new CounterRequest
        {
            Start = start,
            Finish = finish
        });

        Console.Write("\n ----- \nServer response is:\t");

        await foreach (var response in call.ResponseStream.ReadAllAsync())
            Console.Write("{0}\t", response.Counter);

        Console.WriteLine("\n ----- \n");
    }

    private static async Task SayHello(Demo.DemoClient client)
    {
        var response = await client.SayHelloAsync(
                    new HelloRequest { Name = "console_client" });

        Console.WriteLine("\n ----- \nServer response is: {0}\n ----- \n", response.Message);
    }
}