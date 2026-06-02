using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using BankingService.Configuration;
using BankingService.Messaging;
using BankingService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankingService.Messaging
{
    public class SnsEventPublisher : IEventPublisher
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly SnsConfiguration _config;
        private readonly ILogger<SnsEventPublisher> _logger;

        public SnsEventPublisher(
            IAmazonSimpleNotificationService snsClient,
            IOptions<SnsConfiguration> config,
            ILogger<SnsEventPublisher> logger)
        {
            _snsClient = snsClient;
            _config = config.Value;
            _logger = logger;
        }

        public async Task PublishWithdrawalEventAsync(WithdrawalEvent withdrawalEvent)
        {
            var messageJson = JsonSerializer.Serialize(withdrawalEvent);

            var request = new PublishRequest
            {
                TopicArn = _config.TopicArn,
                Message = messageJson
            };

            try
            {
                var response = await _snsClient.PublishAsync(request);
                _logger.LogInformation(
                    "SNS event published. EventId={EventId}, MessageId={MessageId}",
                    withdrawalEvent.EventId, response.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish SNS event. EventId={EventId}, AccountId={AccountId}",
                    withdrawalEvent.EventId, withdrawalEvent.AccountId);
                throw;
            }
        }
    }
}