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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Rock.Model;
using Assert = NUnit.Framework.Assert;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Unit tests for the Rock.Model.PageContext class
    /// </summary>
    [TestFixture]
    public class PageContextTests
    {
        /// <summary>
        /// Tess for the CopyPropertyFrom method
        /// </summary>
        [TestClass]
        public class TheCopyPropertiesFromMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a PageContext object, resulting in a new PageContext.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageContext" )]
            public void ShouldCopyEntity()
            {
                var pageContext = new PageContext { Guid = Guid.NewGuid() };
                var result = pageContext.Clone( false );
                Assert.AreEqual( result.Guid, pageContext.Guid );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheToJsonMethod
        {
            /// <summary>
            /// Should serialize a PageContext into a non-empty string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageContext" )]
            public void ShouldNotBeEmpty()
            {
                var pageContext = new PageContext { Guid = Guid.NewGuid() };
                var result = pageContext.ToJson();
                Assert.IsNotEmpty( result );
            }

            /// <summary>
            /// Shoulds serialize a PageContext into a JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageContext" )]
            public void ShouldExportAsJson()
            {
                var guid = Guid.NewGuid();
                var pageContext = new PageContext
                    {
                        Guid = guid
                    };

                var result = pageContext.ToJson();
                var key = string.Format( "\"Guid\": \"{0}\"", guid );
                Assert.Greater( result.IndexOf( key ), -1, string.Format( "'{0}' was not found in '{1}'.", key, result ) );
            }
        }

        /// <summary>
        /// Tests for the FromJson method
        /// </summary>
        [TestClass]
        public class TheFromJsonMethod
        {
            /// <summary>
            /// Should take a JSON string and copy its contents to a Rock.Model.PageContext instance
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageContext" )]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new PageContext
                {
                    Guid = Guid.NewGuid(),
                    IsSystem = false
                };

                var json = obj.ToJson();
                var pageContext = PageContext.FromJson( json );
                Assert.AreEqual( obj.Guid, pageContext.Guid );
                Assert.AreEqual( obj.IsSystem, pageContext.IsSystem );
            }
        }
    }
}
