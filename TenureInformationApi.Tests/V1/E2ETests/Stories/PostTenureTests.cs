using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using System;
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
    [Collection("AppTest collection")]
    public class PostTenureTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly TenureFixture _tenureFixture;
        private readonly PostTenureSteps _steps;

        public PostTenureTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _tenureFixture = new TenureFixture(_dbFixture.DynamoDbContext, _snsFixture.SimpleNotificationService);
            _steps = new PostTenureSteps(appFactory.Client);
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
                _tenureFixture?.Dispose();
                _steps?.Dispose();
                _snsFixture?.PurgeAllQueueMessages();

                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturnsTheRequestedPerson()
        {
            this.Given(g => _tenureFixture.GivenNewTenureRequest())
                .When(w => _steps.WhenCreateTenureApiIsCalled(_tenureFixture.CreateTenureRequestObject))
                .Then(t => _steps.ThenTheTenureDetailsAreReturnedAndIdIsNotEmpty(_tenureFixture))
                .Then(t => _steps.ThenTheTenureCreatedEventIsRaised(_tenureFixture, _snsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWhenTheyAreValidationErrors()
        {
            this.Given(g => _tenureFixture.GivenNewTenureRequestWithValidationErrors())
                .When(w => _steps.WhenCreateTenureApiIsCalled(_tenureFixture.CreateTenureRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .And(t => _steps.ThenTheValidationErrorsAreReturned())
                .BDDfy();

        }
    }
}
