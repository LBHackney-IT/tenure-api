using AutoFixture;
using Hackney.Core.DynamoDb.EntityUpdater;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Boundary.Requests;
using Hackney.Shared.Tenure.Infrastructure;
using Moq;
using System.Threading.Tasks;
using TenureInformationApi.V1.Factories.Interfaces;
using TenureInformationApi.V1.Gateways.Interfaces;
using TenureInformationApi.V1.UseCase;
using Xunit;

namespace TenureInformationApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class DeletePersonFromTenureUseCaseTests
    {
        private readonly Mock<ITenureDynamoDbGateway> _mockGateway;
        private readonly Mock<ISnsGateway> _tenureSnsGateway;
        private readonly Mock<ITenureSnsFactory> _tenureSnsFactory;

        private readonly DeletePersonFromTenureUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public DeletePersonFromTenureUseCaseTests()
        {
            _mockGateway = new Mock<ITenureDynamoDbGateway>();
            _tenureSnsGateway = new Mock<ISnsGateway>();
            _tenureSnsFactory = new Mock<ITenureSnsFactory>();

            _classUnderTest = new DeletePersonFromTenureUseCase(_mockGateway.Object, _tenureSnsGateway.Object, _tenureSnsFactory.Object);
        }

        [Fact]
        public async Task WhenCalledCallsUseCase()
        {
            // Arrange
            var mockQuery = _fixture.Create<DeletePersonFromTenureQueryRequest>();
            var mockToken = _fixture.Create<Token>();

            // Act
            await _classUnderTest.Execute(mockQuery, mockToken).ConfigureAwait(false);

            // Assert
            _mockGateway.Verify(x => x.DeletePersonFromTenure(mockQuery));
        }

        [Fact]
        public async Task WhenCalledPublishesEvent()
        {
            // Arrange
            var mockQuery = _fixture.Create<DeletePersonFromTenureQueryRequest>();
            var mockToken = _fixture.Create<Token>();
            var gatewayResult = _fixture.Create<UpdateEntityResult<TenureInformationDb>>();
            var snsEvent = _fixture.Create<EntityEventSns>();

            _mockGateway
                .Setup(x => x.DeletePersonFromTenure(It.IsAny<DeletePersonFromTenureQueryRequest>()))
                .ReturnsAsync(gatewayResult);

            _tenureSnsFactory
                 .Setup(x => x.PersonRemovedFromTenure(gatewayResult, It.IsAny<Token>()))
                 .Returns(snsEvent);

            // Act
            await _classUnderTest.Execute(mockQuery, mockToken).ConfigureAwait(false);

            // Assert
            _tenureSnsFactory.Verify(x => x.PersonRemovedFromTenure(gatewayResult, It.IsAny<Token>()), Times.Once);
            _tenureSnsGateway.Verify(x => x.Publish(snsEvent, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
