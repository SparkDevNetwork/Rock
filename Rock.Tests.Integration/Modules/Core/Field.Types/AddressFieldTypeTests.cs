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
    [TestClass]
    public class AddressFieldTypeTests : DatabaseTestsBase
    {
        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetEditValue_ShouldReturnLocationGuidForCompleteAddressFields( bool isRequired )
        {
            var addressFieldType = new AddressFieldType();
            var addressControl = new Rock.Web.UI.Controls.AddressControl();

            /* The IsRequired setting on AddressControl should not affect how GetEditValue works.
            IsRequired should be enforced by UI, not GetEditValue. */
            addressControl.Required = isRequired;

            addressControl.Street1 = "100 Sesame Street";
            addressControl.City = "New York";
            addressControl.State = "NY";
            addressControl.PostalCode = "12345";
            addressControl.Country = "US";
            var configurationValues = new Dictionary<string, ConfigurationValue>();
            var editValue = addressFieldType.GetEditValue( addressControl, configurationValues );
            var locationGuid = editValue.AsGuidOrNull();
            Assert.That.IsNotNull( locationGuid );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetEditValue_ShouldReturnLocationNullForBlankAddressField(bool isRequired)
        {
            var addressFieldType = new AddressFieldType();
            var addressControl = new Rock.Web.UI.Controls.AddressControl();

            /* The IsRequired setting on AddressControl should not affect how GetEditValue works.
            IsRequired should be enforced by UI, not GetEditValue. */
            addressControl.Required = isRequired;

            addressControl.Street1 = "";
            addressControl.City = "";
            addressControl.State = "";
            addressControl.PostalCode = "";
            addressControl.Country = "";
            var configurationValues = new Dictionary<string, ConfigurationValue>();
            var editValue = addressFieldType.GetEditValue( addressControl, configurationValues );
            var locationGuid = editValue.AsGuidOrNull();
            Assert.That.IsNull( locationGuid );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void GetEditValue_ShouldReturnLocationNullForIncompleteAddressField( bool isRequired )
        {
            /*
            A partial address (missing values for some of the minimal fields) should not
            attempt to find/create a Location Record, and therefore should not return
            Location.Guid. In other words, GetEditValue should not enforce validation rules.
            This will allow the the UI should show a Validation Message instead.
            */

            var addressFieldType = new AddressFieldType();
            var addressControl = new Rock.Web.UI.Controls.AddressControl();

            /* The IsRequired setting on AddressControl should not affect how GetEditValue works.
            IsRequired should be enforced by UI, not GetEditValue. */
            addressControl.Required = isRequired;

            addressControl.Street1 = "100 Sesame Street";
            addressControl.City = "";
            addressControl.State = "";
            addressControl.PostalCode = "";
            addressControl.Country = "";
            var configurationValues = new Dictionary<string, ConfigurationValue>();
            var editValue = addressFieldType.GetEditValue( addressControl, configurationValues );
            var locationGuid = editValue.AsGuidOrNull();
            Assert.That.IsNull( locationGuid );
        }

        [TestMethod]
        [DataRow( "00000000-0000-0000-0000-000000000000" )]
        [DataRow( null )]
        [DataRow( "1234" )]
        [DataRow( "not_a_guid" )]
        public void GetTextValue_ShouldReturnEmptyStringForInvalidLocation( string locationGuidString )
        {
            var addressFieldType = new AddressFieldType();
            var configurationValues = new Dictionary<string, string>();

            var textValue = addressFieldType.GetTextValue( locationGuidString, configurationValues );

            Assert.That.AreEqual( string.Empty, textValue );
        }

        [TestMethod]
        public void GetTextValue_ShouldReturnTextValueForValidLocation()
        {
            var addressFieldType = new AddressFieldType();
            var configurationValues = new Dictionary<string, string>();

            var mainCampusLocationGuid = "17e57ea8-d942-485b-8b61-233589aac631";
            var textValue = addressFieldType.GetTextValue( mainCampusLocationGuid, configurationValues );

            Assert.That.AreEqualIgnoreWhitespace( "3120 W Cholla St Phoenix, AZ 85029-4113", textValue );
        }
    }
}
