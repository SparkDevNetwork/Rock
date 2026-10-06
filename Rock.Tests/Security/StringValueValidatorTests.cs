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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.Security;
using Rock.Security;

namespace Rock.Tests.Security
{
    /// <summary>
    /// Unit tests for the <see cref="StringValueValidator"/> class.
    /// </summary>
    [TestClass]
    public class StringValueValidatorTests
    {
        [TestMethod]
        [DataRow( "<a onclick=\"x\">" )]
        [DataRow( "+'+autofocus+onfocus='x'" )]
        [DataRow( "' onfocusin='x'" )]
        [DataRow( "' onpointerover='x'" )]
        [DataRow( "' onpointerdown = 'x'" )]
        [DataRow( "' onanimationstart='x'" )]
        [DataRow( "' onwebkitanimationend='x'" )]
        [DataRow( "' ontransitionend='x'" )]
        [DataRow( "' ontouchstart='x'" )]
        [DataRow( "' onscrollend='x'" )]
        [DataRow( "' ONPOINTEROVER='x'" )]
        public void Validate_WithEventHandlerAttribute_Throws( string value )
        {
            Assert.ThrowsExactly<PropertyValidationException>( () =>
            {
                StringValueValidator.Validate( value, StringValidationRule.EventHandlerAttributes, typeof( object ), "Value" );
            } );
        }

        [TestMethod]
        [DataRow( "We are online = always available." )]
        [DataRow( "Turn the focus = on." )]
        [DataRow( "pointerover='x'" )]
        [DataRow( "https://www.facebook.com/onpointerover" )]
        public void Validate_WithoutEventHandlerAttribute_DoesNotThrow( string value )
        {
            StringValueValidator.Validate( value, StringValidationRule.EventHandlerAttributes, typeof( object ), "Value" );
        }
    }
}
