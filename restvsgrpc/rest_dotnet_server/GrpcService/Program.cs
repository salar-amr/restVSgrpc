using Test;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/", () =>
{
    var q = new TestResponse();
    for (int i = 0; i < 10000; i++)
    {
        q.Result.Add(i);
    }
    return Task.FromResult(q);
}
);

app.Run();
