using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS;
using Paylocity.Poc.WorkerAsync.App;

var host = Host.CreateDefaultBuilder(args)
  .ConfigureServices(services =>
  {
    services.AddHostedService<Worker>();
    services.AddDefaultAWSOptions(new AWSOptions
    {
      Region = RegionEndpoint.USEast2
    });
    services.AddAWSService<IAmazonSQS>();
  })
  .Build();

host.Run();
