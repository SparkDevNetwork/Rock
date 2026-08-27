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
    /// Tests for the exception grouping helpers on <see cref="ExceptionLogService"/>.
    /// </summary>
    [TestClass]
    public class ExceptionLogServiceTests
    {
        /// <summary>
        /// The grouping prefix length must match the LEFT( [Description], n ) used by the ExceptionGroupKey computed
        /// column, which is defined in the AddExceptionLogExceptionGroupKey migration. Changing one without the other
        /// would make the displayed description disagree with what the database groups by.
        /// </summary>
        [TestMethod]
        public void DescriptionGroupingPrefixLength_MatchesExceptionGroupKeyDefinition()
        {
            Assert.AreEqual( 255, ExceptionLogService.DescriptionGroupingPrefixLength );
        }

        /// <summary>
        /// The description portion of the key is everything after the exception type and the pipe separator. The
        /// key is built in SQL as ISNULL( [ExceptionType], '' ) + '|' + LEFT( [Description], 255 ), so a null type
        /// produces a key that starts with the separator and a null description produces a key that ends with it.
        /// </summary>
        [TestMethod]
        [DataRow( "System.NullReferenceException|Object reference not set to an instance of an object.", "System.NullReferenceException", "Object reference not set to an instance of an object." )]
        [DataRow( "System.Exception|Message with a | pipe in it", "System.Exception", "Message with a | pipe in it" )]
        [DataRow( "Type|With|Pipes|Description", "Type|With|Pipes", "Description" )]
        [DataRow( "|Exception without a type", null, "Exception without a type" )]
        [DataRow( "|Exception with an empty type", "", "Exception with an empty type" )]
        [DataRow( "System.Exception|", "System.Exception", "" )]
        [DataRow( "|", null, "" )]
        [DataRow( null, "System.Exception", null )]
        public void GetDescriptionFromExceptionGroupKey_ReturnsDescriptionPortion( string exceptionGroupKey, string exceptionType, string expectedDescription )
        {
            var description = ExceptionLogService.GetDescriptionFromExceptionGroupKey( exceptionGroupKey, exceptionType );

            Assert.AreEqual( expectedDescription, description );
        }

        /// <summary>
        /// A description that was cut at the grouping prefix length is returned in full, without any further truncation.
        /// </summary>
        [TestMethod]
        public void GetDescriptionFromExceptionGroupKey_ReturnsFullPrefixLengthDescription()
        {
            var exceptionType = "System.Exception";
            var descriptionPrefix = new string( 'x', ExceptionLogService.DescriptionGroupingPrefixLength );
            var exceptionGroupKey = exceptionType + "|" + descriptionPrefix;

            var description = ExceptionLogService.GetDescriptionFromExceptionGroupKey( exceptionGroupKey, exceptionType );

            Assert.AreEqual( descriptionPrefix, description );
        }
    }
}
