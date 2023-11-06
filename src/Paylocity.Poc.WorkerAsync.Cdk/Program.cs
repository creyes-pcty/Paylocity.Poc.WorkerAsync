using Amazon.CDK;

namespace Paylocity.Poc.WorkerAsync.Cdk;

internal sealed class Program
{
  public static void Main(string[] args)
  {
    var app = new App();
    _ = new FargateWorkerAsyncStack(app, "poc-fargateworkerasync-svc-us-east-2", new StackProps
    {
      Env = new Environment
      {
          Account = "620915815640",
          Region = "us-east-2"
      }
    });
    app.Synth();
  }
}
