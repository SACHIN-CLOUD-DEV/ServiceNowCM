using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowCM.Domain.Tests
{
    public class ServiceNowConnectionTests
    {
        [Fact]
        public void Constructor_WithValidOAuth2Data_CreatesConnection()
        {
            // Arrange
            string name = "Production SNOW";
            string url = "https://company.service-now.com";

            // Act
            var connection = new ServiceNowConnection(
                name,
                url,
                AuthenticationType.OAuth2,
                "client-id-123",
                "client-secret-xyz",
                null,
                null);

            // Assert
            Assert.Equal(name, connection.Name);
            Assert.Equal(url, connection.InstanceUrl);
            Assert.Equal(
                AuthenticationType.OAuth2,
                connection.AuthenticationType);

            Assert.True(connection.IsActive);
        }

        [Fact]
        public void Constructor_OAuth2WithoutClientId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ServiceNowConnection(
                    "Production SNOW",
                    "https://company.service-now.com",
                    AuthenticationType.OAuth2,
                    null,
                    "client-secret",
                    null,
                    null));
        }

        [Fact]
        public void Constructor_WithValidBasicData_CreatesConnection()
        {
            var connection = new ServiceNowConnection(
                "Production SNOW",
                "https://company.service-now.com",
                AuthenticationType.Basic,
                null,
                null,
                "svc_snow",
                "password123");

            Assert.Equal(
                AuthenticationType.Basic,
                connection.AuthenticationType);

            Assert.Equal(
                "svc_snow",
                connection.Username);

            Assert.True(connection.IsActive);
        }

        [Fact]
        public void Constructor_BasicWithoutUsername_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new ServiceNowConnection(
                    "Production SNOW",
                    "https://company.service-now.com",
                    AuthenticationType.Basic,
                    null,
                    null,
                    null,
                    "password123"));
        }

        [Fact]
        public void Disable_WhenConnectionIsActive_DeactivatesConnection()
        {
            // Arrange
            var connection = new ServiceNowConnection(
                "Production SNOW",
                "https://company.service-now.com",
                AuthenticationType.OAuth2,
                "client-id",
                "client-secret",
                null,
                null);

            // Act
            connection.Disable();

            // Assert
            Assert.False(connection.IsActive);
            Assert.NotNull(connection.ModifiedAtUtc);
        }

        [Fact]
        public void Enable_WhenConnectionIsInactive_ActivatesConnection()
        {
            // Arrange
            var connection = new ServiceNowConnection(
                "Production SNOW",
                "https://company.service-now.com",
                AuthenticationType.OAuth2,
                "client-id",
                "client-secret",
                null,
                null);

            connection.Disable();

            // Act
            connection.Enable();

            // Assert
            Assert.True(connection.IsActive);
            Assert.NotNull(connection.ModifiedAtUtc);
        }

    }
}
