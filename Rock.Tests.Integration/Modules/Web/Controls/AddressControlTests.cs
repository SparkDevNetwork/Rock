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

using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Web.Controls
{
    [TestClass]
    public class AddressControlTests : DatabaseTestsBase
    {
        [TestMethod]
        public void AddressControl_SetsDefaultValuesforCountryAndState()
        {
            var addressControl = new Rock.Web.UI.Controls.AddressControl();

            // Create an address with a Street and City only.
            var location = new Rock.Model.Location
            {
                Street1 = "100 Sesame Street",
                City = "Phoenix"
            };

            // Load the address into the control and read it back again.
            addressControl.SetValues( location );
            addressControl.GetValues( location );

            // Verify that the Country and State fields are set to the default organization values.
            Assert.AreEqual( "AZ", location.State );
            Assert.AreEqual( "US", location.Country );
        }

        [TestMethod]
        public void AddressControl_ValidationFailsIfRequiredFieldNotSupplied()
        {
            var addressControl = new Rock.Web.UI.Controls.AddressControl();

            // Set an incomplete address value.
            var location = new Rock.Model.Location
            {
                Street1 = "100 Sesame Street"
            };

            addressControl.SetValues( location );

            // Validate the contents of the control.
            List<string> messages;
            var isValid = addressControl.Validate( out messages );

            Assert.IsFalse( isValid );
            Assert.AreEqual( 1, messages.Count );
            Assert.AreEqual( "Incomplete Address. The following fields are required: City.", messages[0] );
        }

        [TestMethod]
        public void AddressControl_FilterModeDoesNotSetDefaultValues()
        {
            var addressControl = new Rock.Web.UI.Controls.AddressControl();
            addressControl.PartialAddressIsAllowed = true;

            // Create an address with a Street Only.
            var location = new Rock.Model.Location
            {
                Street1 = "100 Sesame Street"
            };

            // Load the address into the control and read it back again.
            addressControl.SetValues( location );
            addressControl.GetValues( location );

            // Make sure that the Country and State fields are not set to default values.
            Assert.AreEqual( string.Empty, location.State );
            Assert.AreEqual( string.Empty, location.Country );
        }

        [TestMethod]
        public void AddressControl_FilterModeIncompleteAddressIsValid()
        {
            var addressControl = new Rock.Web.UI.Controls.AddressControl();
            addressControl.PartialAddressIsAllowed = true;

            // Create an address with a Street Only.
            var location = new Rock.Model.Location
            {
                Street1 = "100 Sesame Street"
            };

            // Load the address into the control and read it back again.
            addressControl.SetValues( location );
            addressControl.GetValues( location );

            // Make sure that the Country and State fields are not set to default values.
            Assert.AreEqual( string.Empty, location.State );
            Assert.AreEqual( string.Empty, location.Country );
        }
    }
}
