using System;
using System.IO;
using System.Linq;
using System.Threading;
using GenerateAspNetCoreClient.Command;
using GenerateAspNetCoreClient.Command.Model;
using GenerateAspNetCoreClient.Options;
using NUnit.Framework;
using Services.Accounts.Controllers;
using Services.Training.Controllers.SeasonPlan;

namespace GenerateAspNetCoreClient.Tests
{
    public class ClientModelBuilderTests
    {
        [Test]
        public void AddCancellationTokenParameterIfRequested()
        {
            // Arrange
            var options = new GenerateClientOptions { AddCancellationTokenParameters = true };
            var existingParameter = ApiDescriptionTestData.CreateParameter();
            var apiExplorer = ApiDescriptionTestData.CreateApiExplorer(apiParameters: new[] { existingParameter });
            var builder = new ClientModelBuilder(apiExplorer, options, Array.Empty<string>());

            // Act
            var client = builder.GetClientCollection().Clients[0];
            var parameters = client.EndpointMethods[0].Parameters;

            // Assert
            Assert.That(client.ImportedNamespaces, Does.Contain("System.Threading"));

            Assert.That(parameters.Count, Is.EqualTo(2), "(existing and cancellationToken)");

            var cancellationTokenParameter = parameters.Last();

            Assert.That(cancellationTokenParameter.Type, Is.EqualTo(typeof(CancellationToken)));
            Assert.That(cancellationTokenParameter.Name, Is.EqualTo("cancellationToken"));
            Assert.That(cancellationTokenParameter.DefaultValueLiteral, Is.EqualTo("default"));
            Assert.That(cancellationTokenParameter.Source, Is.EqualTo(ParameterSource.Other));
        }

        [Test]
        public void NamespaceMapRewritesDerivedNamespaceAndLocation()
        {
            var options = new GenerateClientOptions
            {
                Namespace = "Coached.Shared.ApiClients",
                NamespaceMap = "Services.Accounts=Accounts"
            };

            var apiExplorer = ApiDescriptionTestData.CreateApiExplorer(new[]
            {
                ApiDescriptionTestData.CreateApiDescription(controllerType: typeof(MappedAccountsController))
            });

            var builder = new ClientModelBuilder(apiExplorer, options, Array.Empty<string>());

            var client = builder.GetClientCollection().Clients.Single();

            Assert.That(client.Namespace, Is.EqualTo("Coached.Shared.ApiClients.Accounts"));
            Assert.That(client.Location, Is.EqualTo("Accounts"));
        }

        [Test]
        public void NamespaceMapPreservesNestedSuffixes()
        {
            var options = new GenerateClientOptions
            {
                Namespace = "Coached.Shared.ApiClients",
                NamespaceMap = "Services.Training=Training"
            };

            var apiExplorer = ApiDescriptionTestData.CreateApiExplorer(new[]
            {
                ApiDescriptionTestData.CreateApiDescription(controllerType: typeof(MappedSeasonPlanController))
            });

            var builder = new ClientModelBuilder(apiExplorer, options, Array.Empty<string>());

            var client = builder.GetClientCollection().Clients.Single();

            Assert.That(client.Namespace, Is.EqualTo("Coached.Shared.ApiClients.Training.SeasonPlan"));
            Assert.That(client.Location, Is.EqualTo(Path.Combine("Training", "SeasonPlan")));
        }
    }
}

namespace Services.Accounts.Controllers
{
    public class MappedAccountsController
    {
    }
}

namespace Services.Training.Controllers.SeasonPlan
{
    public class MappedSeasonPlanController
    {
    }
}
