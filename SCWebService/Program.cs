using DotNetEnv;
using SCWebService.Services.Matchmaking;
using SCWebService.Services;

//Load variables from .env into Environment object
Env.Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IRankedMatchmakingService, RankedMatchmakingService>();

builder.Services.AddControllers();

//Allow access from ITCH
builder.Services.AddCors(options => {
    options.AddPolicy(
        "AllowSpecificOrigin", policy => policy.WithOrigins("https://html-classic.itch.zone/").
        AllowAnyHeader().
        AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
