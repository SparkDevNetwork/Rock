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
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests the v1 REST controllers that write records which have no record
    /// security of their own but hold administrator settings, such as persisted
    /// datasets and workflow triggers. Writes to these must require ADMINISTRATE
    /// on the REST controller, and the fix must not add or change any action
    /// method, since the stored REST security rules are keyed by the action
    /// method signature.
    /// </summary>
    /// <remarks>
    /// The controllers are read by reflection because they derive from a Web
    /// API type that this project does not reference directly.
    /// </remarks>
    [TestClass]
    public class ConfigurationRecordWriteTests
    {
        private static readonly Assembly _restAssembly = Assembly.Load( "Rock.Rest" );

        #region Helpers

        private static Type GetControllerType( string name )
        {
            return _restAssembly.GetType( "Rock.Rest.Controllers." + name, true );
        }

        private static void AssertDeclaresProtectedOverride( string controllerName, string methodName, Type parameterType )
        {
            var method = GetControllerType( controllerName ).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null,
                new[] { parameterType },
                null );

            Assert.IsNotNull( method, $"{controllerName} does not override {methodName}( {parameterType.Name} )." );
            Assert.IsTrue( method.IsFamily, $"{controllerName}.{methodName} must be protected." );
            Assert.AreNotEqual( method.DeclaringType, method.GetBaseDefinition().DeclaringType, $"{controllerName}.{methodName} must override the base method." );
        }

        private static string[] GetDeclaredPublicActionNames( Type type )
        {
            return type
                .GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                .Where( m => !m.IsSpecialName )
                .Select( m => m.Name )
                .OrderBy( n => n, StringComparer.Ordinal )
                .ToArray();
        }

        #endregion

        #region Shared Check

        [TestMethod]
        public void ApiController_CheckCanAdministrateController_IsProtected()
        {
            var apiControllerType = _restAssembly.GetType( "Rock.Rest.ApiController`1", true );
            var method = apiControllerType.GetMethod(
                "CheckCanAdministrateController",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null,
                Type.EmptyTypes,
                null );

            Assert.IsNotNull( method, "ApiController<T>.CheckCanAdministrateController was not found." );

            // Web API treats public methods as actions, so a public helper would add a new endpoint.
            Assert.IsTrue( method.IsFamily, "ApiController<T>.CheckCanAdministrateController must be protected." );
            Assert.IsFalse( method.IsPublic, "ApiController<T>.CheckCanAdministrateController must not be public." );
        }

        [TestMethod]
        public void ApiController_PublicActions_AreUnchanged()
        {
            var expected = new[]
            {
                "Delete",
                "DeleteAttributeValue",
                "Get",
                "Get",
                "GetByAttributeValue",
                "GetByCampus",
                "GetById",
                "GetDataView",
                "GetFollowedItems",
                "InDataView",
                "LaunchWorkflow",
                "LaunchWorkflow",
                "Patch",
                "Post",
                "Put",
                "SetAttributeValue",
                "SetContext"
            };

            var apiControllerType = _restAssembly.GetType( "Rock.Rest.ApiController`1", true );

            CollectionAssert.AreEqual( expected, GetDeclaredPublicActionNames( apiControllerType ) );
        }

        #endregion

        #region Overrides

        [TestMethod]
        public void PersistedDatasetsController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "PersistedDatasetsController", "CheckCanEdit", typeof( PersistedDataset ) );
        }

        [TestMethod]
        public void WorkflowTriggersController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "WorkflowTriggersController", "CheckCanEdit", typeof( WorkflowTrigger ) );
        }

        [TestMethod]
        public void GroupMemberWorkflowTriggersController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "GroupMemberWorkflowTriggersController", "CheckCanEdit", typeof( GroupMemberWorkflowTrigger ) );
        }

        [TestMethod]
        public void PersonAliasController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "PersonAliasController", "CheckCanEdit", typeof( PersonAlias ) );
        }

        [TestMethod]
        public void EntityTypesController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "EntityTypesController", "CheckCanEdit", typeof( EntityType ) );
        }

        #endregion

        #region Action Methods

        [TestMethod]
        public void PersistedDatasetsController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( GetControllerType( "PersistedDatasetsController" ) ) );
        }

        [TestMethod]
        public void WorkflowTriggersController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( GetControllerType( "WorkflowTriggersController" ) ) );
        }

        [TestMethod]
        public void GroupMemberWorkflowTriggersController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( GetControllerType( "GroupMemberWorkflowTriggersController" ) ) );
        }

        [TestMethod]
        public void PersonAliasController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( GetControllerType( "PersonAliasController" ) ) );
        }

        [TestMethod]
        public void EntityTypesController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( GetControllerType( "EntityTypesController" ) ) );
        }

        #endregion
    }
}
