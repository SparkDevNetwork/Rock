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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Lava.Blocks
{
    [TestClass]
    public class RockEntityBlockTests
    {
        [TestMethod]
        public void SelectExpressionWithGuidToString_ConvertsToString()
        {
            var template = "{% campus where:'IsActive == true' select:'new ( Id, Guid.ToString() AS Guid, Name )' sort:'Name' securityenabled:'false' %}{{ campusItems[0].Guid }}{% endcampus %}";
            var campus = new Campus
            {
                Id = 1,
                IsActive = true,
                Name = "Test Campus",
                Guid = new Guid( "B715C9F7-1E5B-4F8A-BDCD-9C3B2E1F0A6B" ),
            };

            using ( var rockApp = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = rockApp.App.CreateRockContext();

                EntityTypeCache.Get<Campus>( true, rockContext );
                rockContext.Set<Campus>().Add( campus );

                var engines = LavaUnitTestHelper.CurrentInstance.CreateActiveTestEngines();

                foreach ( var engine in engines )
                {
                    LavaService.SetCurrentEngine( engine );
                    var result = template.ResolveMergeFields( new Dictionary<string, object>(), "all" );

                    // This verifies that the `Guid.ToString()` part of the
                    // select statement worked.
                    Assert.AreEqual( campus.Guid.ToString(), result );
                }
            }
        }
    }
}
