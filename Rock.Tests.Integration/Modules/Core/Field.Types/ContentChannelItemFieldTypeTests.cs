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

using Rock.Field;
using Rock.Field.Types;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Field.Types
{
    /// <summary>
    /// Test methods for Content Channel Item Field Types.
    /// </summary>
    [TestClass]
    public class ContentChannelItemFieldTypeTests : DatabaseTestsBase
    {
        /// <summary>
        /// Test whether the configuration values are required in order to get the edit value.
        /// </summary>
        [TestMethod]
        public void GetEditValue_AreConfigurationValuesRequired()
        {
            var configValues = new Dictionary<string, ConfigurationValue>
            {
                { ContentChannelItemFieldType.CONTENT_CHANNEL_KEY, new ConfigurationValue(string.Empty) }
            };
            var contentChannelItemFieldTypeWithoutConfigValues = new ContentChannelItemFieldType();
            var contentChannelItemFieldTypeWithConfigValues = new ContentChannelItemFieldType();
            contentChannelItemFieldTypeWithConfigValues.SetConfigurationValues( null, configValues );
            var contentChannelItemControl = new Rock.Web.UI.Controls.ContentChannelItemPicker();

            var withoutEditValue = contentChannelItemFieldTypeWithoutConfigValues.GetEditValue( contentChannelItemControl, new Dictionary<string, ConfigurationValue>() );
            var withEditValue = contentChannelItemFieldTypeWithConfigValues.GetEditValue( contentChannelItemControl, configValues );

            Assert.That.AreEqual( withoutEditValue, withEditValue );
        }
    }
}
