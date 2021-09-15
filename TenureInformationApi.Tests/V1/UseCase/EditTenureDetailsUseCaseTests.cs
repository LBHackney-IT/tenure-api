using AutoFixture;
using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenureInformationApi.Tests.V1.Gateways;
using TenureInformationApi.V1.Boundary.Requests;
using TenureInformationApi.V1.Boundary.Response;
using TenureInformationApi.V1.Domain;
using TenureInformationApi.V1.Domain.Sns;
using TenureInformationApi.V1.Factories;
using TenureInformationApi.V1.Gateways;
using TenureInformationApi.V1.Infrastructure;
using TenureInformationApi.V1.UseCase;
using Xunit;

namespace TenureInformationApi.Tests.V1.UseCase
{
    public class EditTenureDetailsUseCaseTests
    {
        private readonly Mock<ITenureGateway> _mockGateway;
        private readonly EditTenureDetailsUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public EditTenureDetailsUseCaseTests()
        {
            _mockGateway = new Mock<ITenureGateway>();
            _classUnderTest = new EditTenureDetailsUseCase(_mockGateway.Object);
        }

        [Fact]
        public async Task EditTenureDetailsUseCaseWhenTenureDoesntExistReturnsNull()
        {
            var mockQuery = _fixture.Create<TenureQueryRequest>();
            var mockRequestObject = _fixture.Create<EditTenureDetailsRequestObject>();
            var mockRawBody = "";

            _mockGateway.Setup(x => x.EditTenureDetails(mockQuery, mockRequestObject, mockRawBody)).ReturnsAsync((UpdateEntityResult<TenureInformationDb>) null);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody).ConfigureAwait(false);

            response.Should().BeNull();
        }

        [Fact]
        public async Task EditTenureDetailsUseCaseWhenTenureExistsReturnsTenureResponseObject()
        {
            var mockQuery = _fixture.Create<TenureQueryRequest>();
            var mockRequestObject = _fixture.Create<EditTenureDetailsRequestObject>();
            var mockRawBody = "";

            var mockResponseObject = _fixture.Create<TenureResponseObject>();

            var gatewayResponse = new UpdateEntityResult<TenureInformationDb>
            {
                UpdatedEntity = _fixture.Create<TenureInformationDb>()
            };

            _mockGateway.Setup(x => x.EditTenureDetails(mockQuery, mockRequestObject, mockRawBody)).ReturnsAsync(gatewayResponse);

            var response = await _classUnderTest.ExecuteAsync(mockQuery, mockRequestObject, mockRawBody).ConfigureAwait(false);

            response.Should().NotBeNull();
            response.Should().BeOfType(typeof(TenureResponseObject));

            response.StartOfTenureDate.Should().Be(gatewayResponse.UpdatedEntity.StartOfTenureDate);
            response.EndOfTenureDate.Should().Be(gatewayResponse.UpdatedEntity.EndOfTenureDate);
            response.TenureType.Code.Should().Be(gatewayResponse.UpdatedEntity.TenureType.Code);
        }
    }
}