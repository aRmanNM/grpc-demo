using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace server.Services;

public class DemoService : Demo.DemoBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = $"hello {request.Name}"
        });
    }

    public override async Task StreamFromServer(CounterRequest request, IServerStreamWriter<CounterResponse> responseStream, ServerCallContext context)
    {
        var counter = request.Start;
        while (!context.CancellationToken.IsCancellationRequested && counter <= request.Finish)
        {
            await responseStream.WriteAsync(new CounterResponse
            {
                Counter = counter
            });

            if (counter == request.Finish)
                break;

            counter++;
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        }
    }

    public override async Task<SumResponse> StreamFromClient(IAsyncStreamReader<NumbersRequest> requestStream, ServerCallContext context)
    {
        var sum = 0;
        var numbers = new RepeatedField<int>();
        await foreach (var request in requestStream.ReadAllAsync())
        {
            sum += request.Number;
            numbers.Add(request.Number);
        }

        var response = new SumResponse();
        response.Numbers.Add(numbers);
        response.Sum = sum;

        return response;
    }

    public override async Task StreamBothWays(IAsyncStreamReader<PingRequest> requestStream, IServerStreamWriter<PongResponse> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new PongResponse
            {
                Message = "Pong"
            });
        }
    }

    [Authorize(Policy = "none")]
    public override Task<HelloReply> SayHelloAuthenticated(HelloRequest request, ServerCallContext context)
    {
        var user = context.GetHttpContext().User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

        return Task.FromResult(new HelloReply
        {
            Message = $"hello {user} from {request.Name}"
        });
    }
}
