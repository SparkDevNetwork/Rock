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
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Integration.RockTests.Model
{
    [TestClass]
    public class GroupTypeServiceTests
    {
        #region Setup

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
               
        }

        #endregion

        [TestMethod]
        [Ignore]
        public void TestGetInactiveReasonsForGroupType()
        {
            var groupTypeService = new GroupTypeService( new RockContext() );

            var inactiveReasons = groupTypeService.GetInactiveReasonsForGroupType( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP ) );
            //Assert.IsTrue( inactiveReasons.Count == 4 );

            inactiveReasons = groupTypeService.GetInactiveReasonsForGroupType( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_GENERAL ) );
            //Assert.IsTrue( inactiveReasons.Count == 1 );

            inactiveReasons = groupTypeService.GetInactiveReasonsForGroupType( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) );
            //Assert.IsTrue( inactiveReasons.Count == 2 );

            inactiveReasons = groupTypeService.GetInactiveReasonsForGroupType( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_SERVING_TEAM ) );
            //Assert.IsTrue( inactiveReasons.Count == 3 );

        }

    }
}
