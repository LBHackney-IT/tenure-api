using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Shared.Tenure.Boundary.Requests;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using System.Threading.Tasks;

namespace TenureInformationApi.V1.Gateways.Interfaces
{
    public interface ITenureDynamoDbGateway
    {
        Task<TenureInformation> GetEntityById(TenureQueryRequest query);

        Task<TenureInformation> PostNewTenureAsync(CreateTenureRequestObject createTenureRequestObject);

        Task<UpdateEntityResult<TenureInformationDb>> UpdateTenureForPerson(
            UpdateTenureRequest query, UpdateTenureForPersonRequestObject updateTenureRequestObject, int? ifMatch);

        Task<UpdateEntityResult<TenureInformationDb>> EditTenureDetails(TenureQueryRequest query, EditTenureDetailsRequestObject editTenureDetailsRequestObject, string requestBody, int? ifMatch);

        Task<UpdateEntityResult<TenureInformationDb>> DeletePersonFromTenure(DeletePersonFromTenureQueryRequest query);
    }
}
