using Grpc.Core;
using Test;

namespace GrpcService.Services;

public class GreeterService : TestService.TestServiceBase
{
    public override Task<TestResponse> Test(TestRequest request, ServerCallContext context)
    {
        var q = new TestResponse();
        for (int i = 0; i < 10000; i++)
        {
            q.Result.Add(i);
        }

        return Task.FromResult(q);
    }
}
