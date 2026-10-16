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

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Financial;
using Rock.Model;
using Rock.Utility;

namespace Rock.Tests.Utility
{
    /// <summary>
    /// Tests for <see cref="SensitiveAttributeHelper"/>.
    /// </summary>
    [TestClass]
    public class SensitiveAttributeHelperTests
    {
        [TestMethod]
        public void IsSensitiveAttribute_WithPasswordField_ReturnsTrue()
        {
            var configurationValues = new Dictionary<string, string> { ["ispassword"] = "True" };

            var result = SensitiveAttributeHelper.IsSensitiveAttribute( configurationValues, false, "Class", false );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithPasswordDisabled_ReturnsFalse()
        {
            var configurationValues = new Dictionary<string, string> { ["ispassword"] = "False" };

            var result = SensitiveAttributeHelper.IsSensitiveAttribute( configurationValues, false, string.Empty, false );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithComponentLevelSetting_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsSensitiveAttribute( new Dictionary<string, string>(), true, string.Empty, false );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithComponentInstanceSetting_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsSensitiveAttribute( new Dictionary<string, string>(), false, "EntityTypeId", true );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithPrefixedEntityTypeQualifierColumn_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsSensitiveAttribute( new Dictionary<string, string>(), false, "StorageEntityTypeId", true );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithNonComponentQualifierEntityType_ReturnsFalse()
        {
            var result = SensitiveAttributeHelper.IsSensitiveAttribute( new Dictionary<string, string>(), false, "EntityTypeId", false );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithNonEntityTypeQualifierColumn_ReturnsFalse()
        {
            var result = SensitiveAttributeHelper.IsSensitiveAttribute( new Dictionary<string, string>(), false, "GroupTypeId", true );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void IsSensitiveAttribute_WithPlainPersonAttribute_ReturnsFalse()
        {
            var result = SensitiveAttributeHelper.IsSensitiveAttribute( null, false, null, false );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void IsComponentEntityType_WithComponentType_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsComponentEntityType( () => typeof( GatewayComponent ), false );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsComponentEntityType_WithStorageComponentType_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsComponentEntityType( () => typeof( Rock.Storage.AssetStorage.AssetStorageComponent ), false );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsComponentEntityType_WithModelType_ReturnsFalse()
        {
            var result = SensitiveAttributeHelper.IsComponentEntityType( () => typeof( FinancialGateway ), true );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void IsComponentEntityType_WithUnresolvedNonEntityType_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsComponentEntityType( () => null, false );

            Assert.IsTrue( result );
        }

        [TestMethod]
        public void IsComponentEntityType_WithUnresolvedEntityType_ReturnsFalse()
        {
            var result = SensitiveAttributeHelper.IsComponentEntityType( () => null, true );

            Assert.IsFalse( result );
        }

        [TestMethod]
        public void IsComponentEntityType_WhenTypeLoadThrows_ReturnsTrue()
        {
            var result = SensitiveAttributeHelper.IsComponentEntityType( () => throw new System.IO.FileLoadException(), true );

            Assert.IsTrue( result );
        }
    }
}
