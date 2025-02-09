
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Dynamic;
using System.Text.Json;

namespace StreamsWithAsyncEnumerable.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly string _connectionString = "Host=localhost;Database=yourdatabase;Username=yourusername;Password=yourpassword";


    [HttpGet("stream")]
    public async Task GetPessoasStream(CancellationToken ct)
    {
        Response.ContentType = "application/json";
        await foreach (var pessoa in GetPessoasFromDatabase(ct))
        {
            var json = JsonSerializer.Serialize(pessoa);
            await Response.WriteAsync(json + "\n", ct);
            await Response.Body.FlushAsync(ct); // Ensure the data is sent immediately
        }
    }

    [HttpGet("stream2")]
    public async Task StreamData(CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "application/json; charset=utf-8");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var command =
            @"select cd_agent as id, 
                nm_agent as Name,
                vl_document as document
                from geoid.agent 
                order by cd_agent 
                limit 1000000;";

        var results = connection.QueryUnbufferedAsync<object>(command);
        await foreach (var item in results)
        {
            var json = JsonSerializer.Serialize(item) + "\n";
            await Response.WriteAsync(json, ct);
            await Response.Body.FlushAsync(ct);
        }
    }

    private async IAsyncEnumerable<object> GetPessoasFromDatabase(CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var command = new CommandDefinition(
    @"select cd_agent as id, 
                nm_agent as Name,
                vl_document as document
                from geoid.agent 
                order by cd_agent 
                limit 1000000;",
        cancellationToken: default, flags: CommandFlags.Pipelined);
        var reader = await connection.ExecuteReaderAsync(command);

        while (await reader.ReadAsync(ct))
        {
            var pessoa = new ExpandoObject() as IDictionary<string, object>;
            for (var i = 0; i < reader.FieldCount; i++)
            {
                pessoa.Add(reader.GetName(i), reader.GetValue(i));
            }
            yield return pessoa;

            Console.WriteLine("Obtendo dados");
            if (ct.IsCancellationRequested)
            {
                Console.WriteLine("Request cancelada");
                ct.ThrowIfCancellationRequested();
            }
        }
    }
}