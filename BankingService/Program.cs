using Amazon;
using Amazon.SimpleNotificationService;
using BankingService.Configuration;
using BankingService.Messaging;
using BankingService.Repositories;
using BankingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind typed configuration
builder.Services.Configure<SnsConfiguration>(
    builder.Configuration.GetSection("Sns"));

// Register AWS SNS client as singleton — one instance, reused across requests
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
    new AmazonSimpleNotificationServiceClient(
        RegionEndpoint.GetBySystemName(
            builder.Configuration["Sns:Region"])));

// Register application services as scoped — one instance per HTTP request
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IEventPublisher, SnsEventPublisher>();
builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();