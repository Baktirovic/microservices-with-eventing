using Shared.Models.Models;
using Microsoft.EntityFrameworkCore;
using Audit.API.Data;
using MassTransit;
using Audit.API.Consumers;

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PersonNameCreatedConsumer>();
    x.AddConsumer<PersonNameChangedConsumer>();
    x.AddConsumer<RandomLogEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ") ?? "localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
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
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    context.Database.Migrate();
}

app.Run();
