using SCWebService.Services;

//Load variables from .env into Environment object
DotNetEnv.Env.Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddSingleton<MongoDBUserService>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
