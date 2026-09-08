using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Services;
using ServiceNowCM.Application.Tests.Fakes;
using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.Tests
{
    public class ServiceNowConnectionServiceTests
    {

        [Fact]
        public async Task CreateAsync_WithValidRequest_CreatesConnection()
        {
            // Arrange
            var repository = new FakeServiceNowConnectionRepository();

            var credentialStore = new FakeCredentialStore();

            var serviceNowClient = new FakeServiceNowClient();

            var service = new ServiceNowConnectionService(repository, credentialStore, serviceNowClient);

            var request = new CreateServiceNowConnectionRequest
            {
                Name = "Production SNOW",
                InstanceUrl = "https://company.service-now.com",
                AuthenticationType = AuthenticationType.OAuth2,
                ClientId = "client-id",
                ClientSecret = "client-secret"
            };

            // Act
            var connection = await service.CreateAsync(request);

            // Assert
            Assert.Equal("Production SNOW", connection.Name);
            Assert.Equal(AuthenticationType.OAuth2, connection.AuthenticationType);
            Assert.True(connection.IsActive);

            Assert.NotNull(connection.CredentialReference);
            var storedSecret = await credentialStore.GetAsync(connection.CredentialReference);
            Assert.Equal("client-secret", storedSecret);

            var savedConnections = await repository.GetAllAsync();

            Assert.Single(savedConnections);
        }


        [Fact]
        public async Task CreateAsync_WhenNameAlreadyExists_ThrowsException()
        {
            // Arrange
            var repository = new FakeServiceNowConnectionRepository();

            var credentialStore = new FakeCredentialStore();

            var serviceNowClient = new FakeServiceNowClient();

            var service = new ServiceNowConnectionService(repository, credentialStore, serviceNowClient);

            var request =
                new CreateServiceNowConnectionRequest
                {
                    Name = "Production SNOW",
                    InstanceUrl = "https://company.service-now.com",
                    AuthenticationType = AuthenticationType.OAuth2,
                    ClientId = "client-id",
                    ClientSecret = "client-secret"
                };

            await service.CreateAsync(request);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(request));
        }
    }
}
