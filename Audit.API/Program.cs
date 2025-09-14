using Shared.Models.Models;
using Microsoft.EntityFrameworkCore;
using Audit.API.Data;
using MassTransit;
using Audit.API.Consumers;
using Audit.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddHttpClient();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PersonNameCreatedConsumer>();
    x.AddConsumer<PersonNameChangedConsumer>();
    x.AddConsumer<RandomLogEventConsumer>();
  
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("appuser");
            h.Password("supersecret");
        });


        cfg.ReceiveEndpoint("PersonNameCreated", e =>
        {
            e.ConfigureConsumer<PersonNameCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("PersonNameChanged", e =>
        {
            e.ConfigureConsumer<PersonNameChangedConsumer>(context);
        });

        cfg.ReceiveEndpoint("RandomLogEvent", e =>
        {
            e.ConfigureConsumer<RandomLogEventConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// Add a route to serve the frontend
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    context.Database.Migrate();
    
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
