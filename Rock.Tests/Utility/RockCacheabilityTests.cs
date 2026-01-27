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

using System;
using System.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Utility;

namespace Rock.Tests.Utility
{
    [TestClass]
    public class RockCacheabilityTests
    {
        [TestMethod]
        public void RockCacheablityTypeSettingToNonAgeTypeShouldClearAgeProperties()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Public,
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 }
            };

            Assert.IsNotNull( cache.SharedMaxAge );
            Assert.IsNotNull( cache.MaxAge );

            cache.RockCacheablityType = RockCacheablityType.NoCache;

            Assert.IsNull( cache.SharedMaxAge );
            Assert.IsNull( cache.MaxAge );

            cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Public,
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 }
            };

            Assert.IsNotNull( cache.SharedMaxAge );
            Assert.IsNotNull( cache.MaxAge );

            cache.RockCacheablityType = RockCacheablityType.NoStore;

            Assert.IsNull( cache.SharedMaxAge );
            Assert.IsNull( cache.MaxAge );
        }

        [TestMethod]
        public void RockCacheablityTypeSettingToAgeTypeShouldLeaveAgeProperties()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Private,
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 }
            };

            Assert.IsNotNull( cache.SharedMaxAge );
            Assert.IsNotNull( cache.MaxAge );

            cache.RockCacheablityType = RockCacheablityType.Public;

            Assert.IsNotNull( cache.SharedMaxAge );
            Assert.IsNotNull( cache.MaxAge );

            cache.RockCacheablityType = RockCacheablityType.Private;

            Assert.IsNotNull( cache.SharedMaxAge );
            Assert.IsNotNull( cache.MaxAge );
        }

        [TestMethod]
        public void MaxAgeShouldSetWhenNoRockCacheablityOptionSet()
        {
            var cache = new RockCacheability
            {
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
            };

            Assert.IsNotNull( cache.MaxAge );

            cache.RockCacheablityType = RockCacheablityType.Private;

            Assert.IsNotNull( cache.MaxAge );

            cache = new RockCacheability
            {
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
            };

            Assert.IsNotNull( cache.MaxAge );

            cache.RockCacheablityType = RockCacheablityType.Public;

            Assert.IsNotNull( cache.MaxAge );

        }

        [TestMethod]
        public void MaxAgeShouldSetWhenAgeSupportedRockCacheablityOptionSet()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Private
            };

            Assert.IsNull( cache.MaxAge );

            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNotNull( cache.MaxAge );

            cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Public
            };

            Assert.IsNull( cache.MaxAge );

            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNotNull( cache.MaxAge );
        }

        [TestMethod]
        public void MaxAgeShouldNotSetWhenNonAgeSupportedRockCacheablityOptionSet()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoCache
            };

            Assert.IsNull( cache.MaxAge );

            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNull( cache.MaxAge );

            cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoStore
            };

            Assert.IsNull( cache.MaxAge );

            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNull( cache.MaxAge );
        }

        [TestMethod]
        public void SharedMaxAgeShouldSetWhenNoRockCacheablityOptionSet()
        {
            var cache = new RockCacheability
            {
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
            };

            Assert.IsNotNull( cache.SharedMaxAge );

            cache.RockCacheablityType = RockCacheablityType.Private;

            Assert.IsNotNull( cache.SharedMaxAge );

            cache = new RockCacheability
            {
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 10 },
            };

            Assert.IsNotNull( cache.SharedMaxAge );

            cache.RockCacheablityType = RockCacheablityType.Public;

            Assert.IsNotNull( cache.SharedMaxAge );
        }

        [TestMethod]
        public void SharedMaxAgeShouldSetWhenAgeSupportedRockCacheablityOptionSet()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Private
            };

            Assert.IsNull( cache.SharedMaxAge );

            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNotNull( cache.SharedMaxAge );

            cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Public
            };

            Assert.IsNull( cache.SharedMaxAge );

            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNotNull( cache.SharedMaxAge );
        }

        [TestMethod]
        public void SharedMaxAgeShouldNotSetWhenNonAgeSupportedRockCacheablityOptionSet()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoCache
            };

            Assert.IsNull( cache.SharedMaxAge );

            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNull( cache.SharedMaxAge );

            cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoStore
            };

            Assert.IsNull( cache.SharedMaxAge );

            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Days, Value = 5 };

            Assert.IsNull( cache.SharedMaxAge );
        }

        [TestMethod]
        public void OptionSupportsAgeShouldReturnCorrectly()
        {
            var cache = new RockCacheability();
            Assert.IsFalse( cache.OptionSupportsAge( RockCacheablityType.NoCache ) );
            Assert.IsFalse( cache.OptionSupportsAge( RockCacheablityType.NoStore ) );
            Assert.IsTrue( cache.OptionSupportsAge( RockCacheablityType.Private ) );
            Assert.IsTrue( cache.OptionSupportsAge( RockCacheablityType.Public ) );
        }

        [TestMethod]
        public void ToStringShouldReturnCorrectly()
        {
            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoStore
            };

            Assert.AreEqual( "no-store", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.NoCache;
            Assert.AreEqual( "no-cache", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Public;
            Assert.AreEqual( "public", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Private;
            Assert.AreEqual( "private", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Public;
            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            Assert.AreEqual( "public,max-age=3600", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Private;
            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            Assert.AreEqual( "private,max-age=3600", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Public;
            cache.MaxAge = null;
            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            Assert.AreEqual( "public,s-maxage=3600", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Private;
            cache.MaxAge = null;
            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            Assert.AreEqual( "private,s-maxage=3600", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Public;
            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            Assert.AreEqual( "public,max-age=3600,s-maxage=3600", cache.ToString() );

            cache.RockCacheablityType = RockCacheablityType.Private;
            cache.MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            cache.SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 3600 };
            Assert.AreEqual( "private,max-age=3600,s-maxage=3600", cache.ToString() );
        }

        [TestMethod]
        public void SetupHttpCachePolicySetNoStoreOptionCorrectly()
        {
            var cachePolicy = ReflectionHelper.CreateInstance<HttpCachePolicy>();

            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoStore
            };

            cache.SetupHttpCachePolicy( cachePolicy );

            var noStore = ReflectionHelper.GetFieldValue<bool>( cachePolicy, "_noStore" );
            Assert.IsTrue( noStore );
        }

        [TestMethod]
        public void SetupHttpCachePolicySetNoCacheOptionCorrectly()
        {
            var cachePolicy = ReflectionHelper.CreateInstance<HttpCachePolicy>();

            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.NoCache
            };

            cache.SetupHttpCachePolicy( cachePolicy );

            var noStore = ReflectionHelper.GetFieldValue<bool>( cachePolicy, "_noStore" );
            Assert.IsFalse( noStore );

            var cacheability = ReflectionHelper.GetFieldValue<HttpCacheability>( cachePolicy, "_cacheability" );
            Assert.AreEqual( HttpCacheability.NoCache, cacheability );
        }

        [TestMethod]
        public void SetupHttpCachePolicySetPublicOptionCorrectly()
        {
            var cachePolicy = ReflectionHelper.CreateInstance<HttpCachePolicy>();

            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Public
            };

            cache.SetupHttpCachePolicy( cachePolicy );

            var noStore = ReflectionHelper.GetFieldValue<bool>( cachePolicy, "_noStore" );
            Assert.IsFalse( noStore );

            var cacheability = ReflectionHelper.GetFieldValue<HttpCacheability>( cachePolicy, "_cacheability" );
            Assert.AreEqual( HttpCacheability.Public, cacheability );
        }

        [TestMethod]
        public void SetupHttpCachePolicySetPublicOptionWithMaxAgesCorrectly()
        {
            var cachePolicy = ReflectionHelper.CreateInstance<HttpCachePolicy>();

            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Public,
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 120 },
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 240 }
            };

            cache.SetupHttpCachePolicy( cachePolicy );

            var noStore = ReflectionHelper.GetFieldValue<bool>( cachePolicy, "_noStore" );
            Assert.IsFalse( noStore );

            var cacheability = ReflectionHelper.GetFieldValue<HttpCacheability>( cachePolicy, "_cacheability" );
            Assert.AreEqual( HttpCacheability.Public, cacheability );

            var maxAge = ReflectionHelper.GetFieldValue<TimeSpan>( cachePolicy, "_maxAge" );
            Assert.AreEqual( new TimeSpan( 0, 0, 120 ), maxAge );

            var sharedMaxAge = ReflectionHelper.GetFieldValue<TimeSpan>( cachePolicy, "_proxyMaxAge" );
            Assert.AreEqual( new TimeSpan( 0, 0, 240 ), sharedMaxAge );
        }

        [TestMethod]
        public void SetupHttpCachePolicySetPrivateOptionCorrectly()
        {
            var cachePolicy = ReflectionHelper.CreateInstance<HttpCachePolicy>();

            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Private
            };

            cache.SetupHttpCachePolicy( cachePolicy );

            var noStore = ReflectionHelper.GetFieldValue<bool>( cachePolicy, "_noStore" );
            Assert.IsFalse( noStore );

            var cacheability = ReflectionHelper.GetFieldValue<HttpCacheability>( cachePolicy, "_cacheability" );
            Assert.AreEqual( HttpCacheability.Private, cacheability );
        }

        [TestMethod]
        public void SetupHttpCachePolicySetPrivateOptionWithMaxAgesCorrectly()
        {
            var cachePolicy = ReflectionHelper.CreateInstance<HttpCachePolicy>();

            var cache = new RockCacheability
            {
                RockCacheablityType = RockCacheablityType.Private,
                MaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 120 },
                SharedMaxAge = new TimeInterval { Unit = TimeIntervalUnit.Seconds, Value = 240 }
            };

            cache.SetupHttpCachePolicy( cachePolicy );

            var noStore = ReflectionHelper.GetFieldValue<bool>( cachePolicy, "_noStore" );
            Assert.IsFalse( noStore );

            var cacheability = ReflectionHelper.GetFieldValue<HttpCacheability>( cachePolicy, "_cacheability" );
            Assert.AreEqual( HttpCacheability.Private, cacheability );

            var maxAge = ReflectionHelper.GetFieldValue<TimeSpan>( cachePolicy, "_maxAge" );
            Assert.AreEqual( new TimeSpan( 0, 0, 120 ), maxAge );

            var sharedMaxAge = ReflectionHelper.GetFieldValue<TimeSpan>( cachePolicy, "_proxyMaxAge" );
            Assert.AreEqual( new TimeSpan( 0, 0, 240 ), sharedMaxAge );
        }
    }
}
