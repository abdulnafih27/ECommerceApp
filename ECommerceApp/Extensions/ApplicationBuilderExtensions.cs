using ECommerceApp.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseCustomPipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/openapi/v1.json"
                    ,"ECommerce API V1"
                    );
            });
        }

        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}