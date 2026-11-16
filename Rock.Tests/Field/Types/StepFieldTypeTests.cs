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

using Rock.Field.Types;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Test methods for the Step field type.
    /// </summary>
    [TestClass]
    public class StepFieldTypeTests
    {
        /// <summary>
        /// The edit control is a text box holding the Step's Guid, so the
        /// public edit value must be the stored Guid and not the display text.
        /// </summary>
        [TestMethod]
        public void GetPublicEditValue_ReturnsPrivateValue()
        {
            var fieldType = new StepFieldType();
            var privateValue = "A7168000-0000-4000-8000-000000000001";

            var publicEditValue = fieldType.GetPublicEditValue( privateValue, new Dictionary<string, string>() );

            Assert.AreEqual( privateValue, publicEditValue );
        }

        /// <summary>
        /// Saving the edit form must store the same Guid that was sent to it.
        /// </summary>
        [TestMethod]
        public void GetPrivateEditValue_RoundTripsPublicEditValue()
        {
            var fieldType = new StepFieldType();
            var privateValue = "A7168000-0000-4000-8000-000000000001";
            var configurationValues = new Dictionary<string, string>();

            var publicEditValue = fieldType.GetPublicEditValue( privateValue, configurationValues );
            var savedValue = fieldType.GetPrivateEditValue( publicEditValue, configurationValues );

            Assert.AreEqual( privateValue, savedValue );
        }
    }
}
