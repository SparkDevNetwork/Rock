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
using System.Security.Principal;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Web;

namespace Rock.Tests.Web
{
    /// <summary>
    /// Checks that <see cref="ErrorDetailsAccess"/> does not allow exception
    /// details to be shown when it cannot confirm an active administrator.
    /// </summary>
    [TestClass]
    public class ErrorDetailsAccessTests
    {
        [TestMethod]
        public void IsActiveAdministrator_WithoutPerson_ReturnsFalse()
        {
            Assert.IsFalse( ErrorDetailsAccess.IsActiveAdministrator( ( Guid? ) null ) );
        }

        [TestMethod]
        public void IsActiveAdministrator_WhenCurrentPersonCannotBeLoaded_ReturnsFalse()
        {
            var originalPrincipal = Thread.CurrentPrincipal;

            try
            {
                // A signed in user name with no database to look it up in.
                Thread.CurrentPrincipal = new GenericPrincipal( new GenericIdentity( "admin" ), new string[0] );

                using ( TestHelper.CreateScopedRockApp() )
                {
                    Assert.IsFalse( ErrorDetailsAccess.IsActiveAdministrator() );
                }
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }
        }
    }
}
