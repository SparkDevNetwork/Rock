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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Tests for <see cref="PhoneNumber"/> logic that does not need a database.
    /// </summary>
    [TestClass]
    public class PhoneNumberTests
    {
        #region SetNumberAndMessagingEnabled

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithUnchangedNumberAndNoChoice_KeepsStoredValue()
        {
            var phoneNumber = new PhoneNumber { CountryCode = "1", Number = "6235550100", IsMessagingEnabled = true };

            phoneNumber.SetNumberAndMessagingEnabled( "1", "6235550100", null );

            Assert.IsTrue( phoneNumber.IsMessagingEnabled );
        }

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithFormattingOnlyChangeAndNoChoice_KeepsStoredValue()
        {
            var phoneNumber = new PhoneNumber { CountryCode = "1", Number = "6235550100", IsMessagingEnabled = true };

            phoneNumber.SetNumberAndMessagingEnabled( "+1", "(623) 555-0100", null );

            Assert.IsTrue( phoneNumber.IsMessagingEnabled );
            Assert.AreEqual( "6235550100", phoneNumber.Number );
            Assert.AreEqual( "1", phoneNumber.CountryCode );
        }

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithChangedNumberAndNoChoice_TurnsMessagingOff()
        {
            var phoneNumber = new PhoneNumber { CountryCode = "1", Number = "6235550100", IsMessagingEnabled = true };

            phoneNumber.SetNumberAndMessagingEnabled( "1", "6235550199", null );

            Assert.IsFalse( phoneNumber.IsMessagingEnabled );
            Assert.AreEqual( "6235550199", phoneNumber.Number );
        }

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithChangedCountryCodeAndNoChoice_TurnsMessagingOff()
        {
            var phoneNumber = new PhoneNumber { CountryCode = "1", Number = "6235550100", IsMessagingEnabled = true };

            phoneNumber.SetNumberAndMessagingEnabled( "44", "6235550100", null );

            Assert.IsFalse( phoneNumber.IsMessagingEnabled );
        }

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithChangedNumberAndSmsChosen_TurnsMessagingOn()
        {
            var phoneNumber = new PhoneNumber { CountryCode = "1", Number = "6235550100", IsMessagingEnabled = false };

            phoneNumber.SetNumberAndMessagingEnabled( "1", "6235550199", true );

            Assert.IsTrue( phoneNumber.IsMessagingEnabled );
        }

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithUnchangedNumberAndExplicitOff_TurnsMessagingOff()
        {
            var phoneNumber = new PhoneNumber { CountryCode = "1", Number = "6235550100", IsMessagingEnabled = true };

            phoneNumber.SetNumberAndMessagingEnabled( "1", "6235550100", false );

            Assert.IsFalse( phoneNumber.IsMessagingEnabled );
        }

        [TestMethod]
        public void SetNumberAndMessagingEnabled_WithNewNumberAndNoChoice_LeavesMessagingOff()
        {
            var phoneNumber = new PhoneNumber();

            phoneNumber.SetNumberAndMessagingEnabled( "1", "6235550100", null );

            Assert.IsFalse( phoneNumber.IsMessagingEnabled );
        }

        #endregion SetNumberAndMessagingEnabled
    }
}
