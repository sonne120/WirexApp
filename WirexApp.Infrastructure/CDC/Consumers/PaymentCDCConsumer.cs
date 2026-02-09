using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WirexApp.Application.Payments.ReadModels;
using WirexApp.Infrastructure.CDC.Events;
using WirexApp.Infrastructure.CDC.Models;
using WirexApp.Infrastructure.DataAccess.Read;
using WirexApp.Infrastructure.Messaging.Kafka;

namespace WirexApp.Infrastructure.CDC.Consumers
{
    public class PaymentCDCConsumer : KafkaConsumerBase<CDCEvent<PaymentCDCData>>
    {
        private readonly PaymentReadService _readService;
        private readonly ILogger<PaymentCDCConsumer> _logger;

        public PaymentCDCConsumer(
            KafkaConfiguration configuration,
            PaymentReadService readService,
            ILogger<PaymentCDCConsumer> logger)
            : base(configuration, "cdc.payment", logger)
        {
            _readService = readService;
            _logger = logger;
        }

        protected override async Task HandleMessageAsync(
            CDCEvent<PaymentCDCData> cdcEvent,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Received CDC event: EventId={EventId}, Entity={EntityType}, Id={EntityId}, Operation={Operation}",
                cdcEvent.EventId,
                cdcEvent.EntityType,
                cdcEvent.EntityId,
                cdcEvent.Operation);

            try
            {
                switch (cdcEvent.Operation)
                {
                    case CDCOperationType.Create:
                        await HandleCreateAsync(cdcEvent, cancellationToken);
                        break;

                    case CDCOperationType.Update:
                        await HandleUpdateAsync(cdcEvent, cancellationToken);
                        break;

                    case CDCOperationType.Delete:
                        await HandleDeleteAsync(cdcEvent, cancellationToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown CDC operation: {Operation}", cdcEvent.Operation);
                        break;
                }

                _logger.LogInformation(
                    "CDC event processed successfully: EventId={EventId}, Entity={EntityType}, Id={EntityId}",
                    cdcEvent.EventId,
                    cdcEvent.EntityType,
                    cdcEvent.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing CDC event: EventId={EventId}, Entity={EntityType}, Id={EntityId}",
                    cdcEvent.EventId,
                    cdcEvent.EntityType,
                    cdcEvent.EntityId);
                throw;
            }
        }

        private async Task HandleCreateAsync(
            CDCEvent<PaymentCDCData> cdcEvent,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handling CDC Create for payment {PaymentId}", cdcEvent.EntityId);

            var readModel = MapCDCDataToReadModel(cdcEvent.Data);

            _readService.UpdateReadModel(readModel);

            _logger.LogInformation("Read model created for payment {PaymentId}", cdcEvent.EntityId);

            await Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(
            CDCEvent<PaymentCDCData> cdcEvent,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handling CDC Update for payment {PaymentId}", cdcEvent.EntityId);

            var existingReadModel = await _readService.GetByIdAsync(
                Guid.Parse(cdcEvent.EntityId),
                cancellationToken);

            if (existingReadModel != null)
            {
                UpdateReadModelFromCDCData(existingReadModel, cdcEvent.Data);

                _readService.UpdateReadModel(existingReadModel);

                _logger.LogInformation("Read model updated for payment {PaymentId}", cdcEvent.EntityId);
            }
            else
            {
                _logger.LogWarning(
                    "Read model not found for update, creating new one: PaymentId={PaymentId}",
                    cdcEvent.EntityId);

                var newReadModel = MapCDCDataToReadModel(cdcEvent.Data);
                _readService.UpdateReadModel(newReadModel);
            }
        }

        private async Task HandleDeleteAsync(
            CDCEvent<PaymentCDCData> cdcEvent,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Handling CDC Delete for payment {PaymentId}", cdcEvent.EntityId);

            _readService.RemoveReadModel(Guid.Parse(cdcEvent.EntityId));

            _logger.LogInformation("Read model deleted for payment {PaymentId}", cdcEvent.EntityId);

            await Task.CompletedTask;
        }

        private PaymentReadModel MapCDCDataToReadModel(PaymentCDCData cdcData)
        {
            return new PaymentReadModel
            {
                PaymentId = cdcData.PaymentId,
                UserAccountId = cdcData.UserAccountId,
                UserId = cdcData.UserId,
                UserName = string.Empty, 
                UserEmail = string.Empty, 
                SourceCurrency = cdcData.SourceCurrency,
                TargetCurrency = cdcData.TargetCurrency,
                SourceValue = cdcData.SourceValue,
                TargetValue = cdcData.TargetValue,
                Status = cdcData.Status,
                CreateDate = cdcData.CreateDate,
                IsRemoved = cdcData.IsRemoved,
                IsEmailNotificationSent = cdcData.IsEmailNotificationSent,
                ExchangeRate = cdcData.ExchangeRate,
                StatusDescription = GetStatusDescription(cdcData.Status),
                LastModifiedDate = cdcData.LastModifiedDate
            };
        }

        private void UpdateReadModelFromCDCData(PaymentReadModel readModel, PaymentCDCData cdcData)
        {
            readModel.Status = cdcData.Status;
            readModel.IsRemoved = cdcData.IsRemoved;
            readModel.IsEmailNotificationSent = cdcData.IsEmailNotificationSent;
            readModel.StatusDescription = GetStatusDescription(cdcData.Status);
            readModel.LastModifiedDate = DateTime.UtcNow;
        }

        private string GetStatusDescription(string status)
        {
            return status switch
            {
                "ToPay" => "Payment created and waiting to be processed",
                "Processing" => "Payment is being processed",
                "Completed" => "Payment completed successfully",
                "Failed" => "Payment failed",
                "Cancelled" => "Payment was cancelled",
                _ => "Unknown status"
            };
        }
    }
}
