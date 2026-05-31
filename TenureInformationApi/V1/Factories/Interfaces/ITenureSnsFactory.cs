using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;

namespace TenureInformationApi.V1.Factories.Interfaces
{
    public interface ITenureSnsFactory
    {
        EntityEventSns CreateTenure(TenureInformation tenure, Token token);
        EntityEventSns PersonAddedToTenure(UpdateEntityResult<TenureInformationDb> updateResult, Token token);
        EntityEventSns UpdateTenure(UpdateEntityResult<TenureInformationDb> updateResult, Token token);
        EntityEventSns PersonRemovedFromTenure(UpdateEntityResult<TenureInformationDb> updateResult, Token token);
    }
}
