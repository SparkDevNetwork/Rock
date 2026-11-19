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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field;
using Rock.Field.Types;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="DataViewsFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class DataViewsFieldTypeTests
    {
        private const string ObsidianJson = "{\"value\":\"B0A6D5E3-6B3A-4F0A-9E57-2D3B6C0D1A11\",\"text\":\"Person\"}";

        #region Public Configuration Values

        [TestMethod]
        [DataRow( ConfigurationValueUsage.Edit )]
        [DataRow( ConfigurationValueUsage.Configure )]
        public void GetPublicConfigurationValues_EmptyEntityTypeName_IsRemovedWithoutEntityType( ConfigurationValueUsage usage )
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var config = new DataViewsFieldType().GetPublicConfigurationValues( new Dictionary<string, string> { ["entityTypeName"] = "" }, usage, null );

                Assert.IsFalse( config.ContainsKey( "entityTypeName" ) );
                Assert.IsFalse( config.ContainsKey( "entityType" ) );
            }
        }

        [TestMethod]
        [DataRow( ConfigurationValueUsage.Edit )]
        [DataRow( ConfigurationValueUsage.Configure )]
        public void GetPublicConfigurationValues_ExistingEntityType_IsKeptAndWins( ConfigurationValueUsage usage )
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var privateConfig = new Dictionary<string, string>
                {
                    ["entityType"] = ObsidianJson,
                    ["entityTypeName"] = "Rock.Model.Group"
                };

                var config = new DataViewsFieldType().GetPublicConfigurationValues( privateConfig, usage, null );

                Assert.AreEqual( ObsidianJson, config["entityType"] );
                Assert.IsFalse( config.ContainsKey( "entityTypeName" ) );
            }
        }

        [TestMethod]
        public void GetPublicConfigurationValues_ExistingEntityTypeOnly_IsPassedThrough()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var config = new DataViewsFieldType().GetPublicConfigurationValues( new Dictionary<string, string> { ["entityType"] = ObsidianJson }, ConfigurationValueUsage.Edit, null );

                Assert.AreEqual( ObsidianJson, config["entityType"] );
                Assert.IsFalse( config.ContainsKey( "entityTypeName" ) );
            }
        }

        [TestMethod]
        public void GetPublicConfigurationValues_ViewUsage_IsNotConverted()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var config = new DataViewsFieldType().GetPublicConfigurationValues( new Dictionary<string, string> { ["entityTypeName"] = "Rock.Model.Person" }, ConfigurationValueUsage.View, null );

                Assert.AreEqual( "Rock.Model.Person", config["entityTypeName"] );
                Assert.IsFalse( config.ContainsKey( "entityType" ) );
            }
        }

        #endregion

        #region Private Configuration Values

        [TestMethod]
        [DataRow( "{}" )]
        [DataRow( "null" )]
        [DataRow( "" )]
        [DataRow( "{\"value\":\"\",\"text\":\"\"}" )]
        public void GetPrivateConfigurationValues_EmptyEntityType_StoresEmptyEntityTypeName( string entityType )
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var config = new DataViewsFieldType().GetPrivateConfigurationValues( new Dictionary<string, string> { ["entityType"] = entityType } );

                Assert.AreEqual( string.Empty, config["entityTypeName"] );
                Assert.IsFalse( config.ContainsKey( "entityType" ) );
            }
        }

        [TestMethod]
        public void GetPrivateConfigurationValues_NoEntityType_LeavesEntityTypeNameUntouched()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var config = new DataViewsFieldType().GetPrivateConfigurationValues( new Dictionary<string, string> { ["entityTypeName"] = "Rock.Model.Person" } );

                Assert.AreEqual( "Rock.Model.Person", config["entityTypeName"] );
                Assert.IsFalse( config.ContainsKey( "entityType" ) );
            }
        }

        #endregion
    }
}
