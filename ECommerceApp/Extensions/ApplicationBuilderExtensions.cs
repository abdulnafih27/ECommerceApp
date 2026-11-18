using ECommerceApp.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseCustomPipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}