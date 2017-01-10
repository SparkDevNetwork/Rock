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
using NUnit.Framework;
using Rock.Model;
using Assert = NUnit.Framework.Assert;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Unit tests for the Rock.Model.HtmlContent class
    /// </summary>
    [TestFixture]
    public class HtmlContentTests
    {
        /// <summary>
        /// Tests for the CopyPropertyFrom method
        /// </summary>
        [TestClass]
        public class TheCopyPropertiesFromMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a HtmlContent object, resulting in a new HtmlContent.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.HtmlContent" )]
            public void ShouldCopyEntity()
            {
                var html = new HtmlContent { Content = "Foo" };
                var result = html.Clone( false );
                Assert.AreEqual( result.Content, html.Content );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheToJsonMethod
        {
            /// <summary>
            /// Should serialize a HtmlContent into a non-empty string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.HtmlContent" )]
            public void ShouldNotBeEmpty()
            {
                var html = new HtmlContent { Content = "Foo" };
                var result = html.ToJson();
                Assert.IsNotEmpty( result );
            }

            /// <summary>
            /// Shoulds serialize a HtmlContent into a JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.HtmlContent" )]
            public void ShouldExportAsJson()
            {
                var html = new HtmlContent
                    {
                        Content = "Foo"
                    };

                var result = html.ToJson();
                const string key = "\"Content\": \"Foo\"";
                Assert.Greater( result.IndexOf( key ), -1, string.Format( "'{0}' was not found in '{1}'.", key, result ) );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheFromJsonMethod
        {
            /// <summary>
            /// Should take a JSON string and copy its contents to a Rock.Model.HtmlContent instance
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.HtmlContent" )]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new HtmlContent
                    {
                        EntityValue = "Some Value",
                        IsApproved = true
                    };

                var json = obj.ToJson();
                var htmlContent = HtmlContent.FromJson( json );
                Assert.AreEqual( obj.EntityValue, htmlContent.EntityValue );
                Assert.AreEqual( obj.IsApproved, htmlContent.IsApproved );
            }
        }
    }
}
