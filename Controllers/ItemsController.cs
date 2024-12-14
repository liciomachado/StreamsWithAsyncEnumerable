using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json;

namespace StreamsWithAsyncEnumerable.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly string _connectionString = "Host=localhost;Database=yourdatabase;Username=yourusername;Password=yourpassword";

    [HttpGet("stream")]
    public async Task GetPessoasStream()
    {
        Response.ContentType = "application/json";
        await foreach (var pessoa in GetPessoasFromDatabase())
        {
            var json = JsonSerializer.Serialize(pessoa);
            await Response.WriteAsync(json + "\n");
            await Response.Body.FlushAsync(); // Ensure the data is sent immediately
        }
    }

    private async IAsyncEnumerable<Pessoa> GetPessoasFromDatabase()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new CommandDefinition(@"select cd_agent as id, 
        nm_agent as Name 
        from geoid.agent 
        order by cd_agent 
        limit 1000000;", cancellationToken: default, flags: CommandFlags.Pipelined);
        var reader = await connection.ExecuteReaderAsync(command);

        while (await reader.ReadAsync())
        {
            yield return new Pessoa
            {
                Id = reader.GetInt64(0),
                Nome = reader.GetString(1)
            };
        }
    }
}