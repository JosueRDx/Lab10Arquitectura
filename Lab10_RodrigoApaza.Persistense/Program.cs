using Lab10_RodrigoApaza.Persistense.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticketera API Lab10 v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization(); 


app.MapControllers();

app.Run();