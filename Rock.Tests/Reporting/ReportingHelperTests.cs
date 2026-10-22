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

using Rock.Reporting;

namespace Rock.Tests.Reporting
{
    /// <summary>
    /// Tests for <see cref="ReportingHelper"/>.
    /// </summary>
    [TestClass]
    public class ReportingHelperTests
    {
        #region GetSqlSafePageParameters

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "TH160 Group" )]
        [DataRow( "  12 " )]
        [DataRow( "12" )]
        [DataRow( "O-Brien" )]
        [DataRow( "-" )]
        [DataRow( "a/b" )]
        [DataRow( "/" )]
        [DataRow( "a*b" )]
        [DataRow( "*" )]
        [DataRow( "x*/" )]
        [DataRow( "- -" )]
        [DataRow( "%27" )]
        [DataRow( "O\u2019Brien" )]
        [DataRow( "Zo\u00eb \u65e5\u672c\u8a9e \u00fcber" )]
        [DataRow( "2B6E4E0A-9C0B-4F1E-8B4D-1C2D3E4F5A6B" )]
        [DataRow( "1,2,3" )]
        public void GetSqlSafePageParameters_WithSafeValue_ReturnsValueUnchanged( string value )
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new[] { new KeyValuePair<string, string>( "Value", value ) } );

            Assert.AreEqual( value, result["Value"] );
        }

        [TestMethod]
        [DataRow( "O'Brien" )]
        [DataRow( "'" )]
        [DataRow( "TH160'" )]
        [DataRow( "a;b" )]
        [DataRow( ";" )]
        [DataRow( "x--y" )]
        [DataRow( "--" )]
        [DataRow( "TH160--" )]
        [DataRow( "x/*y" )]
        [DataRow( "/*" )]
        [DataRow( "x' OR 1=1 OR '1'='1" )]
        [DataRow( "O\u02BCBrien" )]
        [DataRow( "O\uFF07Brien" )]
        [DataRow( "O\u02B9Brien" )]
        [DataRow( "O\u2032Brien" )]
        [DataRow( "x\u02BC OR 1=1 OR \u02BC1\u02BC=\u02BC1" )]
        public void GetSqlSafePageParameters_WithUnsafeValue_ReturnsEmptyString( string value )
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new[] { new KeyValuePair<string, string>( "Value", value ) } );

            Assert.AreEqual( string.Empty, result["Value"] );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_WithMixedValues_OnlyBlanksUnsafeValues()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new Dictionary<string, string>
            {
                ["GroupId"] = "12",
                ["GroupName"] = "TH160'",
                ["Search"] = "a;b"
            } );

            Assert.AreEqual( 3, result.Count );
            Assert.AreEqual( "12", result["GroupId"] );
            Assert.AreEqual( string.Empty, result["GroupName"] );
            Assert.AreEqual( string.Empty, result["Search"] );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_KeyLookup_IsCaseInsensitive()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new[] { new KeyValuePair<string, string>( "GroupId", "12" ) } );

            Assert.AreEqual( "12", result["groupid"] );
            Assert.AreEqual( "12", result["GROUPID"] );
            Assert.IsTrue( result.ContainsKey( "groupId" ) );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_PreservesKeySpelling()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new[] { new KeyValuePair<string, string>( "GroupId", "12" ) } );

            CollectionAssert.AreEqual( new[] { "GroupId" }, new List<string>( result.Keys ) );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_UsesOrdinalIgnoreCaseComparer()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new Dictionary<string, string>() );

            Assert.AreSame( StringComparer.OrdinalIgnoreCase, result.Comparer );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_WithKeysDifferingOnlyByCase_KeepsOneEntryWithLastValue()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new[]
            {
                new KeyValuePair<string, string>( "GroupId", "12" ),
                new KeyValuePair<string, string>( "groupid", "13" )
            } );

            Assert.AreEqual( 1, result.Count );
            Assert.AreEqual( "13", result["GroupId"] );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_WithEmptyInput_ReturnsEmptyDictionary()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( new Dictionary<string, string>() );

            Assert.IsNotNull( result );
            Assert.AreEqual( 0, result.Count );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_WithNullInput_ReturnsEmptyDictionary()
        {
            var result = ReportingHelper.GetSqlSafePageParameters( null );

            Assert.IsNotNull( result );
            Assert.AreEqual( 0, result.Count );
        }

        [TestMethod]
        public void GetSqlSafePageParameters_ReturnsNewDictionary()
        {
            var source = new Dictionary<string, string>
            {
                ["GroupId"] = "12"
            };

            var result = ReportingHelper.GetSqlSafePageParameters( source );
            result["GroupId"] = "99";

            Assert.AreEqual( "12", source["GroupId"] );
        }

        #endregion
    }
}
