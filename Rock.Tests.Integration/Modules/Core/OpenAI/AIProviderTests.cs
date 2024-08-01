// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.AI
{
    /// <summary>
    /// Integration tests for AI Features.
    /// </summary>
    [TestClass]
    [TestCategory("Core.AI")]
    public class AIProviderTests : DatabaseTestsBase
    {
        private const string _openAiDefaultProviderGuid = "2AA26B14-94CB-4A30-9E97-C7250BA464BB";
        private const string _openAiCompatibleProviderGuid = "2A15B464-A07C-404C-9C51-6970E46C01FE";
        private const string _openAiProviderEntityTypeGuid = "8D3F25B1-4891-31AA-4FA6-365F5C808563";

        [TestMethod]
        public void AiProvider_AddNewOpenAiDefaultProvider_AddsNewInstance()
        {
            var newProvider = GetNewProviderInstance( _openAiDefaultProviderGuid.AsGuid() );
            var newName = newProvider.Name;

            var dataContext = new RockContext();
            var service = new AIProviderService( dataContext );
            service.Add( newProvider );

            dataContext.SaveChanges();

            // Retrieve the new component and verify that it has the correct properties.
            newProvider = service.Get( _openAiDefaultProviderGuid );

            Assert.That.IsNotNull( newProvider, "Expected entity not found." );
            Assert.That.AreEqual( newName, newProvider.Name );
        }

        [TestMethod]
        public void AiProvider_AddNewOpenAiCompatibleProvider_AddsNewInstance()
        {
            var newProvider = GetNewProviderInstance( _openAiCompatibleProviderGuid.AsGuid() );
            var newName = newProvider.Name;

            var dataContext = new RockContext();
            var service = new AIProviderService( dataContext );
            service.Add( newProvider );

            dataContext.SaveChanges();

            // Retrieve the new component and verify that it has the correct properties.
            newProvider = service.Get( _openAiCompatibleProviderGuid );

            Assert.That.IsNotNull( newProvider, "Expected entity not found." );
            Assert.That.AreEqual( newName, newProvider.Name );
        }

        [TestMethod]
        public void AiProvider_AddNewInstanceWithNoProviderType_FailsWithErrorNotification()
        {
            var dataContext = new RockContext();

            var newProvider = GetNewProviderInstance( _openAiDefaultProviderGuid.AsGuid() );

            newProvider.ProviderComponentEntityTypeId = null;

            var service = new AIProviderService( dataContext );
            service.Add( newProvider );

            try
            {
                dataContext.SaveChanges();

                Assert.Fail( "Validation error expected, but not encountered." );
            }
            catch (Exception ex)
            {
                // Validation exception expected.
            }
        }

        [TestMethod]
        public void AiProvider_GetActiveProviderInstance_ReturnsFirstActiveProviderByOrder()
        {
            var activeProvider = Rock.AI.Provider.AIProviderContainer.GetActiveComponent();

            Assert.That.AreEqual( true, activeProvider.IsActive );
        }

        private AIProvider GetNewProviderInstance( Guid newProviderGuid )
        {
            var newProvider = new AIProvider();

            var newName = $"Test ({newProviderGuid})";
            newProvider.Name = newName;
            newProvider.Description = "Test Provider for AI Features.";
            newProvider.Guid = newProviderGuid;
            newProvider.IsActive = true;
            newProvider.Order = 1;

            // Set the default AI Provider component.
            var openAIProviderEntityTypeId = EntityTypeCache.GetId( _openAiProviderEntityTypeGuid );

            Assert.That.IsNotNull( openAIProviderEntityTypeId, "Open AI Provider not found" );

            newProvider.ProviderComponentEntityTypeId = openAIProviderEntityTypeId;

            return newProvider;
        }

        private void AddAiProviderTestData()
        {

        }

    }
}
