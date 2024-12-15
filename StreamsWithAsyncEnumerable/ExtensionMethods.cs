namespace StreamsWithAsyncEnumerable;

public static class ExtensionMethods
{
    public static IServiceCollection CustomAddCors(this IServiceCollection service)
    {
        service.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        return service;
    }
}