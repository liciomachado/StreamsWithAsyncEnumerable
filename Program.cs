using StreamsWithAsyncEnumerable;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.CustomAddCors();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/stream", async context =>
{
    context.Response.ContentType = "application/json";
    await foreach (var pessoa in GetPessoas())
    {
        var json = System.Text.Json.JsonSerializer.Serialize(pessoa);
        await context.Response.WriteAsync(json + "\n");
        await context.Response.Body.FlushAsync(); // Ensure the data is sent immediately
    }
});

async IAsyncEnumerable<Pessoa> GetPessoas()
{
    foreach (var pessoa in Helper.GetPessoas())
    {
        yield return pessoa;
        await Task.Delay(500);
    }
}

app.Run();