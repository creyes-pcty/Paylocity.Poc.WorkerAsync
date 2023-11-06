using Amazon.SQS;
using Amazon.SQS.Model;

namespace Paylocity.Poc.WorkerAsync.App;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly IAmazonSQS _sqsClient;
  private readonly string _sqsUrl;

  public Worker(ILogger<Worker> logger, IAmazonSQS sqsClient)
  {
    _logger = logger;
    _sqsClient = sqsClient;
    _sqsUrl = Environment.GetEnvironmentVariable("SQS_URL")
      ?? throw new ArgumentException( "Must set SQS_URL environment variable");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var receiveRequest = new ReceiveMessageRequest
        {
          QueueUrl = _sqsUrl,
          WaitTimeSeconds = 15
        };

        var result = await _sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

        foreach (var message in result.Messages)
        {
          _logger.LogInformation($"Processing message: {message.Body} | {DateTimeOffset.Now}");

          var deleteRequest = new DeleteMessageRequest
          {
            QueueUrl = _sqsUrl,
            ReceiptHandle = message.ReceiptHandle
          };

          await _sqsClient.DeleteMessageAsync(deleteRequest, stoppingToken);
        }
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message);
      }
    }
  }
}
