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
    /// This suite checks that the values used to look up an
    /// <see cref="InteractionDeviceType"/> are cut down to the column lengths.
    /// </summary>
    [TestClass]
    [TestCategory( "Core.Interactions.DeviceType" )]
    public class InteractionServiceDeviceTypeTests
    {
        [TestMethod]
        public void ClampDeviceTypeValue_WithLongClientType_CutsToColumnLength()
        {
            var value = new string( 'a', 32 );

            var result = InteractionService.ClampDeviceTypeValue( value, InteractionService.MaxDeviceTypeClientTypeLength );

            Assert.AreEqual( 25, result.Length );
            Assert.AreEqual( new string( 'a', 25 ), result );
        }

        [TestMethod]
        public void ClampDeviceTypeValue_WithLongApplicationAndOperatingSystem_CutsToColumnLength()
        {
            var value = new string( 'b', 250 );

            Assert.AreEqual( 100, InteractionService.ClampDeviceTypeValue( value, InteractionService.MaxDeviceTypeApplicationLength ).Length );
            Assert.AreEqual( 100, InteractionService.ClampDeviceTypeValue( value, InteractionService.MaxDeviceTypeOperatingSystemLength ).Length );
        }

        [TestMethod]
        public void ClampDeviceTypeValue_WithShortValue_IsUnchanged()
        {
            Assert.AreEqual( "Mobile", InteractionService.ClampDeviceTypeValue( "Mobile", InteractionService.MaxDeviceTypeClientTypeLength ) );
            Assert.AreEqual( string.Empty, InteractionService.ClampDeviceTypeValue( string.Empty, InteractionService.MaxDeviceTypeClientTypeLength ) );
        }

        [TestMethod]
        public void ClampDeviceTypeValue_WithExactLengthValue_IsUnchanged()
        {
            var value = new string( 'c', 25 );

            Assert.AreEqual( value, InteractionService.ClampDeviceTypeValue( value, InteractionService.MaxDeviceTypeClientTypeLength ) );
        }

        [TestMethod]
        public void ClampDeviceTypeValue_WithNull_StaysNull()
        {
            Assert.IsNull( InteractionService.ClampDeviceTypeValue( null, InteractionService.MaxDeviceTypeClientTypeLength ) );
        }

        [TestMethod]
        public void ClampDeviceTypeValue_WithValuesDifferingAfterColumnLength_ProducesSameValue()
        {
            var prefix = new string( 'd', 25 );

            var first = InteractionService.ClampDeviceTypeValue( prefix + "one", InteractionService.MaxDeviceTypeClientTypeLength );
            var second = InteractionService.ClampDeviceTypeValue( prefix + "two", InteractionService.MaxDeviceTypeClientTypeLength );

            Assert.AreEqual( first, second );
        }
    }
}
