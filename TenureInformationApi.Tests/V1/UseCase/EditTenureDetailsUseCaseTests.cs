using AutoFixture;
using FluentAssertions;
using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Boundary.Requests;
using Hackney.Shared.Tenure.Boundary.Response;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenureInformationApi.V1.Factories.Interfaces;
using TenureInformationApi.V1.Gateways.Interfaces;
using TenureInformationApi.V1.UseCase;
using Xunit;

namespace TenureInformationApi.Tests.V1.UseCase
{
    public class EditTenureDetailsUseCaseTests
    {
        private readonly Mock<ITenureDynamoDbGateway> _mockGateway;
        private readonly EditTenureDetailsUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ISnsGateway> _tenureSnsGateway;
        private readonly Mock<ITenureSnsFactory> _tenureSnsFactory;

        public EditTenureDetailsUseCaseTests()
        {
            _mockGateway = new Mock<ITenureDynamoDbGateway>();
            _tenureSnsGateway = new Mock<ISnsGateway>();
            _tenureSnsFactory = new Mock<ITenureSnsFactory>();

            _classUnderTest = new EditTenureDetailsUseCase(_mockGateway.Object, _tenureSnsGateway.Object, _tenureSnsFactory.Object, new NullLogger<EditTenureDetailsUseCase>());
        }

        [Fact]
        public async Task EditTenureDetailsUseCaseWhenTenureDoesntExistReturnsNull()
        {
            var mockQuery = _fixture.Create<TenureQueryRequest>();
            var mockRequestObject = _fixture.Create<EditTenureDetailsRequestObject>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.EditTenureDetails(It.IsAny<TenureQueryRequest>(), It.IsAny<EditTenureDetailsRequestObject>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync((UpdateEntityResult<TenureInformationDb>) null);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().BeNull();
        }

        [Fact]
        public async Task EditTenureDetailsUseCaseWhenTenureExistsReturnsTenureResponseObject()
        {
            var mockQuery = _fixture.Create<TenureQueryRequest>();
            var mockRequestObject = _fixture.Create<EditTenureDetailsRequestObject>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResponse = new UpdateEntityResult<TenureInformationDb>
            {
                UpdatedEntity = _fixture.Create<TenureInformationDb>()
            };

            _mockGateway.Setup(x => x.EditTenureDetails(It.IsAny<TenureQueryRequest>(), It.IsAny<EditTenureDetailsRequestObject>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(gatewayResponse);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Should().BeOfType(typeof(TenureResponseObject));

            response.StartOfTenureDate.Should().Be(gatewayResponse.UpdatedEntity.StartOfTenureDate);
            response.EndOfTenureDate.Should().Be(gatewayResponse.UpdatedEntity.EndOfTenureDate);
            response.TenureType.Code.Should().Be(gatewayResponse.UpdatedEntity.TenureType.Code);
        }

        [Fact]
        public async Task EditTenureDetailsUseCaseWhenNoChangesSNSGatewayIsntCalled()
        {
            var mockQuery = _fixture.Create<TenureQueryRequest>();
            var mockRequestObject = _fixture.Create<EditTenureDetailsRequestObject>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            // setup mock gateway to return UpdateEntityResult with no changes
            var gatewayResult = MockUpdateEntityResultWhereNoChangesAreMade();

            _mockGateway
                .Setup(x => x.EditTenureDetails(It.IsAny<TenureQueryRequest>(), It.IsAny<EditTenureDetailsRequestObject>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(gatewayResult);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            // assert result is TenureResponseObject
            response.Should().BeOfType(typeof(TenureResponseObject));

            // assert that sns factory wasnt called
            _tenureSnsGateway.Verify(x => x.Publish(It.IsAny<EntityEventSns>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditTenureDetailsUseCaseWhenChangesSNSGatewayIsCalled()
        {
            // Arrange
            var mockQuery = _fixture.Create<TenureQueryRequest>();
            var mockRequestObject = _fixture.Create<EditTenureDetailsRequestObject>();
            var mockRawBody = "";
            var mockToken = _fixture.Create<Token>();

            var gatewayResult = MockUpdateEntityResultWhereChangesAreMade();

            _mockGateway
                .Setup(x => x.EditTenureDetails(It.IsAny<TenureQueryRequest>(), It.IsAny<EditTenureDetailsRequestObject>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(gatewayResult);

            var snsEvent = _fixture.Create<EntityEventSns>();

            _tenureSnsFactory
                 .Setup(x => x.UpdateTenure(gatewayResult, It.IsAny<Token>()))
                 .Returns(snsEvent);

            // Act
            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody, mockToken, null).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(TenureResponseObject));

            _tenureSnsFactory.Verify(x => x.UpdateTenure(gatewayResult, It.IsAny<Token>()), Times.Once);
            _tenureSnsGateway.Verify(x => x.Publish(snsEvent, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private UpdateEntityResult<TenureInformationDb> MockUpdateEntityResultWhereChangesAreMade()
        {
            return new UpdateEntityResult<TenureInformationDb>
            {
                UpdatedEntity = _fixture.Create<TenureInformationDb>(),
                NewValues = new Dictionary<string, object>
                {
                    { "startOfTenureDate", _fixture.Create<DateTime>() }
                }
            };
        }

        private UpdateEntityResult<TenureInformationDb> MockUpdateEntityResultWhereNoChangesAreMade()
        {
            return new UpdateEntityResult<TenureInformationDb>
            {
                UpdatedEntity = _fixture.Create<TenureInformationDb>()
                // empty
            };
        }
    }
}
