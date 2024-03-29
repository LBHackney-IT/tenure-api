using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Boundary.Requests;
using Hackney.Shared.Tenure.Boundary.Response;
using Hackney.Shared.Tenure.Factories;
using System;
using System.Threading.Tasks;
using TenureInformationApi.V1.Factories;
using TenureInformationApi.V1.Factories.Interfaces;
using TenureInformationApi.V1.Gateways.Interfaces;
using TenureInformationApi.V1.UseCase.Interfaces;

namespace TenureInformationApi.V1.UseCase
{
    public class UpdateTenureForPersonUseCase : IUpdateTenureForPersonUseCase
    {
        private readonly ITenureDynamoDbGateway _tenureGateway;
        private readonly ISnsGateway _snsGateway;
        private readonly ITenureSnsFactory _snsFactory;
        public UpdateTenureForPersonUseCase(ITenureDynamoDbGateway gateway, ISnsGateway snsGateway, ITenureSnsFactory snsFactory)
        {
            _tenureGateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        [LogCall]
        public async Task<TenureResponseObject> ExecuteAsync(UpdateTenureRequest query, UpdateTenureForPersonRequestObject updateTenureRequestObject,
            Token token, int? ifMatch)
        {
            var updateResult = await _tenureGateway.UpdateTenureForPerson(query, updateTenureRequestObject, ifMatch).ConfigureAwait(false);
            if (updateResult == null) return null;

            var tenureSnsMessage = _snsFactory.PersonAddedToTenure(updateResult, token);
            var tenureTopicArn = Environment.GetEnvironmentVariable("TENURE_SNS_ARN");

            await _snsGateway.Publish(tenureSnsMessage, tenureTopicArn).ConfigureAwait(false);
            return updateResult.UpdatedEntity.ToDomain().ToResponse();
        }
    }
}
