using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using System;
using TenureInformationApi.V1.Factories.Interfaces;
using TenureInformationApi.V1.Infrastructure;

namespace TenureInformationApi.V1.Factories
{
    public class TenureSnsFactory : ITenureSnsFactory
    {
        public EntityEventSns CreateTenure(TenureInformation tenure, Token token)
        {
            return new EntityEventSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = tenure.Id,
                Id = Guid.NewGuid(),
                EventType = CreateTenureEventConstants.EVENTTYPE,
                Version = CreateTenureEventConstants.V1_VERSION,
                SourceDomain = CreateTenureEventConstants.SOURCE_DOMAIN,
                SourceSystem = CreateTenureEventConstants.SOURCE_SYSTEM,
                EventData = new EventData
                {
                    NewData = tenure
                },
                User = new User { Name = token.Name, Email = token.Email }
            };
        }

        public EntityEventSns UpdateTenure(UpdateEntityResult<TenureInformationDb> updateResult, Token token)
        {
            return new EntityEventSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = updateResult.UpdatedEntity.Id,
                Id = Guid.NewGuid(),
                EventType = UpdateTenureConstants.EVENTTYPE,
                Version = UpdateTenureConstants.V1_VERSION,
                SourceDomain = UpdateTenureConstants.SOURCE_DOMAIN,
                SourceSystem = UpdateTenureConstants.SOURCE_SYSTEM,
                EventData = new EventData
                {
                    NewData = updateResult.NewValues,
                    OldData = updateResult.OldValues
                },
                User = new User { Name = token.Name, Email = token.Email }
            };
        }

        public EntityEventSns PersonAddedToTenure(UpdateEntityResult<TenureInformationDb> updateResult, Token token)
        {
            return new EntityEventSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = updateResult.UpdatedEntity.Id,
                Id = Guid.NewGuid(),
                EventType = PersonAddedToTenureConstants.EVENTTYPE,
                Version = PersonAddedToTenureConstants.V1_VERSION,
                SourceDomain = PersonAddedToTenureConstants.SOURCE_DOMAIN,
                SourceSystem = PersonAddedToTenureConstants.SOURCE_SYSTEM,
                EventData = new EventData
                {
                    NewData = updateResult.NewValues,
                    OldData = updateResult.OldValues
                },
                User = new User { Name = token.Name, Email = token.Email }
            };
        }

        public EntityEventSns PersonRemovedFromTenure(UpdateEntityResult<TenureInformationDb> updateResult, Token token)
        {
            return new EntityEventSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = updateResult.UpdatedEntity.Id,
                Id = Guid.NewGuid(),
                EventType = PersonRemovedFromTenureConstants.EVENTTYPE,
                Version = PersonRemovedFromTenureConstants.V1_VERSION,
                SourceDomain = PersonRemovedFromTenureConstants.SOURCE_DOMAIN,
                SourceSystem = PersonRemovedFromTenureConstants.SOURCE_SYSTEM,
                EventData = new EventData
                {
                    NewData = updateResult.NewValues,
                    OldData = updateResult.OldValues
                },
                User = new User { Name = token.Name, Email = token.Email }
            };
        }
    }
}
