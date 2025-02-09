using System.Text.Json;
using System.Text.Json.Nodes;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

try
{
    await FetchPessoasStream(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operação cancelada pelo usuário.");
}

return;

static async Task FetchPessoasStream(CancellationToken cancellationToken)
{
    Console.WriteLine("Insira uma tecla para começar o processamento...");
    Console.ReadLine();
    using var client = new HttpClient();
    using var response = await client.GetAsync("https://localhost:7162/items/stream2", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    response.EnsureSuccessStatusCode();

    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    using var reader = new StreamReader(stream);

    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (line != null)
        {
            var jsonObject = JsonNode.Parse(line)?.AsObject();
            if (jsonObject != null)
            {
                var id = jsonObject["id"]?.ToString();
                var name = jsonObject["name"]?.ToString();
                var document = jsonObject["document"]?.ToString();
                Console.WriteLine($"Id: {id}, Name: {name}, Document: {document}");
            }
        }

        // Check for cancellation
        if (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Request cancelada");
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}

static async Task FetchPessoasStreamV2(CancellationToken cancellationToken)
{
    Console.WriteLine("Insira uma tecla para começar o processamento...");
    Console.ReadLine();
    using var client = new HttpClient();
    var response = await client.GetStreamAsync("https://localhost:7162/items/stream2", cancellationToken);

    // Processar o stream de dados
    await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<dynamic>(response))
    {
        Console.WriteLine(item);

        // Check for cancellation
        if (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Request cancelada");
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}