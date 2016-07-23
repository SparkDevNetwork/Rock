﻿// <copyright>
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
    /// Unit tests for the Rock.Model.PageRoute class
    /// </summary>
    [TestFixture]
    public class PageRouteTests
    {
        /// <summary>
        /// Tests for the CopyPropertiesFrom method
        /// </summary>
        [TestClass]
        public class TheCopyPropertiesFromMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a PageRoute object, resulting in a new PageRoute.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageRoute" )]
            public void ShouldCopyEntity()
            {
                var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
                var result = pageRoute.Clone( false );
                Assert.AreEqual( result.Guid, pageRoute.Guid );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheToJsonMethod
        {
            /// <summary>
            /// Should serialize a PageRoute into a non-empty string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageRoute" )]
            public void ShouldNotBeEmpty()
            {
                var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
                dynamic result = pageRoute.ToJson();
                Assert.IsNotEmpty( result );
            }

            /// <summary>
            /// Shoulds serialize a PageRoute into a JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageRoute" )]
            public void ShouldExportAsJson()
            {
                var guid = Guid.NewGuid();
                var pageRoute = new PageRoute
                    {
                        Guid = guid
                    };

                var result = pageRoute.ToJson();
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
            /// Should take a JSON string and copy its contents to a Rock.Model.PageRoute instance
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.PageRoute" )]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new PageRoute
                    {
                        Route = "/some/path",
                        IsSystem = true
                    };

                var json = obj.ToJson();
                var pageRoute = PageRoute.FromJson( json );
                Assert.AreEqual( obj.Route, pageRoute.Route );
                Assert.AreEqual( obj.IsSystem, pageRoute.IsSystem );
            }
        }
    }
}
