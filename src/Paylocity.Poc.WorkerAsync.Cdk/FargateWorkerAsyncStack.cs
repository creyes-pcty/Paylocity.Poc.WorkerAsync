using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace Paylocity.Poc.WorkerAsync.Cdk;

public class FargateWorkerAsyncStack : Stack
{
  internal FargateWorkerAsyncStack(
    Construct scope, string id, IStackProps props = null)
    : base(scope, id, props)
  {
    var containerImage = ContainerImage.FromEcrRepository(
      Repository.FromRepositoryName(
        this,
        "container-repository",
        "poc-fargateworkerasync-svc-us-east-2"));

    var vpc = Vpc.FromLookup(this, "vpc", new VpcLookupOptions
    {
      VpcId = "vpc-04e578b76292c953b"
    });

    var deadLetterQueue = new Queue(this, "dead-letter-queue", new QueueProps
    {
      RetentionPeriod = Duration.Days(3),
      VisibilityTimeout = Duration.Seconds(900)
    });
    var queue = new Queue(this, "queue", new QueueProps
    {
      DeadLetterQueue = new DeadLetterQueue
      {
        MaxReceiveCount = 3,
        Queue = deadLetterQueue
      },
    });

    var taskDefinition = new FargateTaskDefinition(this, "task", new FargateTaskDefinitionProps
    {
      MemoryLimitMiB = 512,
      Cpu = 256
    });
    taskDefinition.AddContainer("container", new ContainerDefinitionOptions
    {
      Image = containerImage,
      Environment = new Dictionary<string, string>
      {
        {"SQS_URL", queue.QueueUrl}
      }
    });

    _ = new QueueProcessingFargateService(this, "service", new QueueProcessingFargateServiceProps
    {
      Cluster = new Cluster(this, "cluster", new ClusterProps {Vpc = vpc}),
      SecurityGroups = new ISecurityGroup[]
      {
        new SecurityGroup(this, "sg", new SecurityGroupProps {Vpc = vpc})
      },
      Image = containerImage,
      Environment = new Dictionary<string, string>
      {
        {"SQS_URL", queue.QueueUrl}
      },
      LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
      {
        StreamPrefix = "poc-fargateworkerasync-svc-us-east-2",
        LogRetention = RetentionDays.THREE_DAYS
      }),
      Queue = queue,
      TaskDefinition = taskDefinition
    });
  }
}
