using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Hackney.Core.DynamoDb;
using Hackney.Core.Sns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace TenureInformationApi.Tests
{
    public class AwsMockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly List<TableDef> _tables;

        public IAmazonDynamoDB DynamoDb { get; private set; }
        public IDynamoDBContext DynamoDbContext { get; private set; }
        public IAmazonSimpleNotificationService SimpleNotificationService { get; private set; }
        public IAmazonSQS AmazonSQS { get; private set; }


        public AwsMockWebApplicationFactory(List<TableDef> tables)
        {
            _tables = tables;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();
            builder.ConfigureServices(services =>
            {
                services.ConfigureDynamoDB();
                services.ConfigureSns();

                var serviceProvider = services.BuildServiceProvider();
                DynamoDb = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
                DynamoDbContext = serviceProvider.GetRequiredService<IDynamoDBContext>();
                SimpleNotificationService = serviceProvider.GetRequiredService<IAmazonSimpleNotificationService>();

                var localstackUrl = Environment.GetEnvironmentVariable("Localstack_SnsServiceUrl");
                AmazonSQS = new AmazonSQSClient(new AmazonSQSConfig() { ServiceURL = localstackUrl });

                EnsureTablesExist(DynamoDb, _tables);
            });
        }

        private static void EnsureTablesExist(IAmazonDynamoDB dynamoDb, List<TableDef> tables)
        {
            foreach (var table in tables)
            {
                try
                {
                    var request = new CreateTableRequest(table.Name,
                        new List<KeySchemaElement> { new KeySchemaElement(table.KeyName, KeyType.HASH) },
                        new List<AttributeDefinition> { new AttributeDefinition(table.KeyName, table.KeyType) },
                        new ProvisionedThroughput(3, 3));
                    _ = dynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
            }
        }
    }
}
