using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Test;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
Console.WriteLine(Environment.GetEnvironmentVariable("GRPC_SERVER_ADDR"));
Console.WriteLine(Environment.GetEnvironmentVariable("REST_SERVER_ADDR"));


var restServerUrl = new Uri(Environment.GetEnvironmentVariable("REST_SERVER_ADDR")!);
var address = new Uri(Environment.GetEnvironmentVariable("GRPC_SERVER_ADDR")!);

builder.Services
    .AddGrpcClient<TestService.TestServiceClient>(o =>
    {
        o.Address = address;
    })
    .ConfigureChannel(o =>
    {
        o.Credentials = ChannelCredentials.Insecure;
        o.ServiceConfig = new ServiceConfig
        {
            LoadBalancingConfigs = { new RoundRobinConfig() }
        };
    });

builder.Services.AddSingleton<ResolverFactory>(
    sp => new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(30)));

var app = builder.Build();


// var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { Credentials = ChannelCredentials.Insecure, ServiceConfig = new ServiceConfig { LoadBalancingConfigs = { new RoundRobinConfig() } } });
// var client = new TestService.TestServiceClient(channel);
// using var call = client.Test();

// var responseDictionary = new ConcurrentDictionary<int, TaskCompletionSource<Test.TestResponse>>();


// var readTask = Task.Run(async () =>
// {
//     await foreach (var response in call.ResponseStream.ReadAllAsync())
//     {
//         if (responseDictionary.TryGetValue(response.Number, out var tcs))
//         {
//             tcs.SetResult(response); // Complete the Task for the matched request
//             await call.ResponseStream.MoveNext();
//         }
//     }
// });

// app.MapGet("/grpc/{num}", async (int num) =>
// {


//     var request = new Test.TestRequest() { Number = num };
//     var tcs = new TaskCompletionSource<Test.TestResponse>();
//     var b = responseDictionary.TryAdd(num, tcs);


//     try
//     {

//         await call.RequestStream.WriteAsync(request);
//         // var nn = call.ResponseStream.MoveNext();
//         // await call.RequestStream.CompleteAsync();
//         // var r = call.ResponseStream.ReadAllAsync();
//         // var jj = r.ToBlockingEnumerable();
//         // var xx = jj.FirstOrDefault();
//         // while (await call.ResponseStream.MoveNext())
//         // {
//         //     Console.WriteLine("Greeting: " + call.ResponseStream.Current.Message);
//         //     // "Greeting: Hello World" is written multiple times
//         // }

//         // var response = await client.TestAsync(request);
//         if (!b)
//         {
//             responseDictionary.TryGetValue(num, out var tcss);
//             if (tcss != null)
//             {
//                 var response = tcss.Task;

//                 return Results.Json(response.Result.Result);
//             }

//             return Results.Json(new { });


//         }
//         else
//         {
//             var response = await tcs.Task;

//             return Results.Json(response.Result);
//         }
//     }
//     catch (Exception)
//     {
//         return Results.Problem();
//     }
// });

app.MapGet("/grpc", async (TestService.TestServiceClient client) =>
{

    await client.TestAsync(new TestRequest() { Number = 1 });

    return;
});

app.MapGet("/http", async (HttpClient httpClient) =>
{

    await httpClient.PostAsJsonAsync(restServerUrl, new TestRequest { Number = 1 });

    return;

});

app.Run();
