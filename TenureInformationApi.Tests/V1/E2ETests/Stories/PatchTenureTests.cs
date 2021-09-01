using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenureInformationApi.Tests.V1.E2ETests.Fixtures;
using TenureInformationApi.Tests.V1.E2ETests.Steps;
using TestStack.BDDfy;
using Xunit;

namespace TenureInformationApi.Tests.V1.E2ETests.Stories
{
    [Story(
       AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
       IWant = "the ability to capture a new tenure",
       SoThat = "I can create a new tenure in the system with all the relevant information")]
    [Collection("Aws collection")]
    public class PatchTenureTests : IDisposable
    {
        private readonly AwsIntegrationTests<Startup> _dbFixture;
        private readonly TenureFixture _tenureFixture;
        private readonly PatchTenureStep _steps;

        public PatchTenureTests(AwsIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
            _tenureFixture = new TenureFixture(_dbFixture.DynamoDbContext, _dbFixture.SimpleNotificationService);
            _steps = new PatchTenureStep(_dbFixture.Client);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != _tenureFixture)
                    _tenureFixture.Dispose();

                _disposed = true;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ServiceUpdateTheRequestedTenureWithNewHousehold(bool nullTenuredAssetType)
        {
            this.Given(g => _tenureFixture.GivenanUpdateTenureWithNewHouseholdReqeust(nullTenuredAssetType))
                .When(w => _steps.WhenUpdateTenureApiIsCalled(_tenureFixture.TenureId, _tenureFixture.PersonId, _tenureFixture.UpdateTenureRequestObject))
                .Then(t => _steps.ThenTheTenureDetailsAreUpdated(_tenureFixture))
                .BDDfy();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ServiceUpdatesTheRequestedUpdateTenureHouseHold(bool nullTenuredAssetType)
        {
            this.Given(g => _tenureFixture.GivenanUpdateTenureHouseholdRequest(nullTenuredAssetType))
                .When(w => _steps.WhenUpdateTenureApiIsCalled(_tenureFixture.TenureId, _tenureFixture.PersonId, _tenureFixture.UpdateTenureRequestObject))
                .Then(t => _steps.ThenTheTenureDetailsAreUpdated(_tenureFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsNotFoundIfPersonNotExist()
        {
            this.Given(g => _tenureFixture.GivenAUpdateTenureDoesNotExist())
                .When(w => _steps.WhenUpdateTenureApiIsCalled(_tenureFixture.TenureId, _tenureFixture.PersonId, _tenureFixture.UpdateTenureRequestObject))
                .Then(t => _steps.ThenNotFoundIsReturned())
                .BDDfy();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ServiceReturnsBadRequestWhenTheyAreValidationErrors(bool nullTenuredAssetType)
        {
            this.Given(g => _tenureFixture.GivenanUpdateTenureRequestWithValidationError(nullTenuredAssetType))
                .When(w => _steps.WhenUpdateTenureApiIsCalled(_tenureFixture.TenureId, _tenureFixture.PersonId, _tenureFixture.UpdateTenureRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .BDDfy();
        }


    }
}