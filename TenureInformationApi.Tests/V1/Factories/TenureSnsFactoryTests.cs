using AutoFixture;
using FluentAssertions;
using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using System;
using System.Collections.Generic;
using TenureInformationApi.V1.Factories;
using TenureInformationApi.V1.Infrastructure;
using Xunit;

namespace TenureInformationApi.Tests.V1.Factories
{
    public class TenureSnsFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();

        private UpdateEntityResult<TenureInformationDb> CreateUpdateEntityResult(TenureInformationDb tenureDb)
        {
            return _fixture.Build<UpdateEntityResult<TenureInformationDb>>()
                                       .With(x => x.UpdatedEntity, tenureDb)
                                       .With(x => x.OldValues, new Dictionary<string, object> { { "title", "Dr" } })
                                       .With(x => x.NewValues, new Dictionary<string, object> { { "title", "Mr" } })
                                       .Create();
        }

        [Fact]
        public void CreateTenureTestCreatesSnsEvent()
        {
            var tenure = _fixture.Create<TenureInformation>();
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = tenure };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new TenureSnsFactory();
            var result = factory.CreateTenure(tenure, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
            result.EntityId.Should().Be(tenure.Id);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(CreateTenureEventConstants.EVENTTYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(CreateTenureEventConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(CreateTenureEventConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(CreateTenureEventConstants.V1_VERSION);
        }

        [Fact]
        public void UpdateTestCreatesSnsEvent()
        {
            var tenureDb = _fixture.Create<TenureInformationDb>();

            var updateResult = CreateUpdateEntityResult(tenureDb);
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = updateResult.NewValues, OldData = updateResult.OldValues };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new TenureSnsFactory();
            var result = factory.UpdateTenure(updateResult, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
            result.EntityId.Should().Be(tenureDb.Id);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(UpdateTenureConstants.EVENTTYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(UpdateTenureConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(UpdateTenureConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(UpdateTenureConstants.V1_VERSION);
        }

        [Fact]
        public void PersonAddedToTenureTestCreatesSnsEvent()
        {
            var tenureDb = _fixture.Create<TenureInformationDb>();

            var updateResult = CreateUpdateEntityResult(tenureDb);
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = updateResult.NewValues, OldData = updateResult.OldValues };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new TenureSnsFactory();
            var result = factory.PersonAddedToTenure(updateResult, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
            result.EntityId.Should().Be(tenureDb.Id);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(PersonAddedToTenureConstants.EVENTTYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(PersonAddedToTenureConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(PersonAddedToTenureConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(PersonAddedToTenureConstants.V1_VERSION);
        }

        [Fact]
        public void PersonRemovedFromTenureTestCreatesSnsEvent()
        {
            var tenureDb = _fixture.Create<TenureInformationDb>();

            var updateResult = CreateUpdateEntityResult(tenureDb);
            var token = _fixture.Create<Token>();

            var expectedEventData = new EventData() { NewData = updateResult.NewValues, OldData = updateResult.OldValues };
            var expectedUser = new User() { Email = token.Email, Name = token.Name };

            var factory = new TenureSnsFactory();
            var result = factory.PersonRemovedFromTenure(updateResult, token);

            result.CorrelationId.Should().NotBeEmpty();
            result.DateTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
            result.EntityId.Should().Be(tenureDb.Id);
            result.EventData.Should().BeEquivalentTo(expectedEventData);
            result.EventType.Should().Be(PersonRemovedFromTenureConstants.EVENTTYPE);
            result.Id.Should().NotBeEmpty();
            result.SourceDomain.Should().Be(PersonRemovedFromTenureConstants.SOURCE_DOMAIN);
            result.SourceSystem.Should().Be(PersonRemovedFromTenureConstants.SOURCE_SYSTEM);
            result.User.Should().BeEquivalentTo(expectedUser);
            result.Version.Should().Be(PersonRemovedFromTenureConstants.V1_VERSION);
        }
    }
}
