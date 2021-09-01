using System.Threading.Tasks;
using TenureInformationApi.V1.Boundary.Requests;
using TenureInformationApi.V1.Domain;

namespace TenureInformationApi.V1.Gateways
{
    public interface ITenureGateway
    {
        Task<TenureInformation> GetEntityById(TenureQueryRequest query);

        Task<TenureInformation> PostNewTenureAsync(CreateTenureRequestObject createTenureRequestObject);

        Task<TenureInformation> UpdateTenure(TenureQueryRequest query, UpdateTenureRequestObject updateTenureRequest);
    }
}
