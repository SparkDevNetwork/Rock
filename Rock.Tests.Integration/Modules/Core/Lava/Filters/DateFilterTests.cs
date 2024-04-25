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

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
{
    [TestClass]
    public class DateFilterTests : LavaIntegrationTestBase
    {
        #region Filter Tests: SundayDate

        /// <summary>
        /// Applying the filter to a Friday returns the previous Sunday if the week starts on a Sunday.
        /// </summary>
        [TestMethod]
        public void SundayDate_InputDateIsFriday_YieldsNextSunday()
        {
            // This filter returns the Sunday associated with the current week.
            // Rock considers Sunday to be the last day of the week by default, so any other day should return a future date.
            TestHelper.AssertTemplateOutputDate( "3-May-2020",
                                      "{{ '1-May-2020' | SundayDate }}" );
        }

        /// <summary>
        /// Applying the filter to a Sunday returns the same day.
        /// </summary>
        [TestMethod]
        public void SundayDate_InputDateIsSunday_YieldsSameDay()
        {
            TestHelper.AssertTemplateOutputDate( "3-May-2020",
                                      "{{ '3-May-2020' | SundayDate }}" );
        }

        /// <summary>
        /// Applying the filter to the 'Now' keyword yields the Sunday of the current week.
        /// </summary>
        [TestMethod]
        public void SundayDate_InputParameterIsNow_YieldsNextSunday()
        {
            var nextSunday = RockDateTime.Now.SundayDate();

            TestHelper.AssertTemplateOutputDate( nextSunday,
                                      "{{ 'Now' | SundayDate }}" );
        }

        #endregion
    }
}
