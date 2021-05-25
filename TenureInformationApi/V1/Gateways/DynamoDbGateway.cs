using Amazon.DynamoDBv2.DataModel;
using TenureInformationApi.V1.Domain;
using TenureInformationApi.V1.Factories;
using TenureInformationApi.V1.Infrastructure;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace TenureInformationApi.V1.Gateways
{
    public class DynamoDbGateway : ITenureGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;

        public DynamoDbGateway(IDynamoDBContext dynamoDbContext)
        {
            _dynamoDbContext = dynamoDbContext;
        }


        public TenureInformation GetEntityById(Guid id)
        {
            var result = _dynamoDbContext.LoadAsync<TenureInformationDb>(id).GetAwaiter().GetResult();
            return result?.ToDomain();
        }
    }
}
