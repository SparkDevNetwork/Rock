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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

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
        #region DescriptionGroupingPrefixLength

        /// <summary>
        /// The grouping prefix length must match the LEFT( [Description], n ) hashed into the ExceptionGroupHash
        /// computed column, which is defined in the AddExceptionLogExceptionGroupHash migration. Changing one
        /// without the other would make the description the Exception List block displays disagree with what the
        /// database groups by, silently.
        /// </summary>
        [TestMethod]
        public void DescriptionGroupingPrefixLength_MatchesExceptionGroupHashDefinition()
        {
            Assert.AreEqual( 255, ExceptionLogService.DescriptionGroupingPrefixLength );
        }

        #endregion DescriptionGroupingPrefixLength

        #region GetDisplayDescription

        /// <summary>
        /// A description shorter than the prefix length was read whole, so it is displayed as it is.
        /// </summary>
        [TestMethod]
        [DataRow( "Object reference not set to an instance of an object.", DisplayName = "Typical description" )]
        [DataRow( "Trailing ellipsis of its own...", DisplayName = "Description that already ends in an ellipsis" )]
        public void GetDisplayDescription_WithDescriptionShorterThanPrefixLength_ReturnsDescriptionUnchanged( string descriptionPrefix )
        {
            var result = ExceptionLogService.GetDisplayDescription( descriptionPrefix );

            Assert.AreEqual( descriptionPrefix, result );
        }

        /// <summary>
        /// Description is nullable, and SQL Server's SUBSTRING of a NULL description is NULL, so the block can hand
        /// this method a null. It must come back as null rather than as a bare ellipsis.
        /// </summary>
        [TestMethod]
        public void GetDisplayDescription_WithNullDescription_ReturnsNull()
        {
            var result = ExceptionLogService.GetDisplayDescription( null );

            Assert.IsNull( result );
        }

        /// <summary>
        /// One character short of the prefix length is the longest description that is known to have been read
        /// whole, so it is the boundary that must not gain an ellipsis.
        /// </summary>
        [TestMethod]
        public void GetDisplayDescription_WithDescriptionOneCharacterShorterThanPrefixLength_ReturnsDescriptionUnchanged()
        {
            var descriptionPrefix = new string( 'x', ExceptionLogService.DescriptionGroupingPrefixLength - 1 );

            var result = ExceptionLogService.GetDisplayDescription( descriptionPrefix );

            Assert.AreEqual( descriptionPrefix, result );
        }

        /// <summary>
        /// The prefix length is the most that is ever read, so reaching it means the description was cut and the
        /// ellipsis tells the reader there is more. A description of exactly this length was not cut and gets an
        /// ellipsis it does not need; the two cases are indistinguishable from the prefix alone, which is the
        /// accepted trade-off this test pins down.
        /// </summary>
        [TestMethod]
        public void GetDisplayDescription_WithDescriptionAtPrefixLength_AppendsEllipsis()
        {
            var descriptionPrefix = new string( 'x', ExceptionLogService.DescriptionGroupingPrefixLength );

            var result = ExceptionLogService.GetDisplayDescription( descriptionPrefix );

            Assert.AreEqual( descriptionPrefix + "...", result );
        }

        #endregion GetDisplayDescription

        #region ExceptionGroupHash Mapping

        /// <summary>
        /// ExceptionGroupHash is a computed column, so Entity Framework has to be told to leave it out of INSERT and
        /// UPDATE statements. Losing this attribute would not fail a build or show up on a read: it would make every
        /// attempt to write an ExceptionLog fail, which means Rock would silently stop being able to record any
        /// exception at all.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_IsMappedAsADatabaseComputedColumn()
        {
            var property = typeof( ExceptionLog ).GetProperty( nameof( ExceptionLog.ExceptionGroupHash ) );

            Assert.IsNotNull( property, $"{nameof( ExceptionLog )}.{nameof( ExceptionLog.ExceptionGroupHash )} was not found." );

            var databaseGenerated = property.GetCustomAttributes<DatabaseGeneratedAttribute>().SingleOrDefault();

            Assert.IsNotNull( databaseGenerated, $"{nameof( ExceptionLog.ExceptionGroupHash )} must be marked with [DatabaseGenerated] so Entity Framework does not write to the computed column." );
            Assert.AreEqual( DatabaseGeneratedOption.Computed, databaseGenerated.DatabaseGeneratedOption );
        }

        #endregion ExceptionGroupHash Mapping
    }
}
