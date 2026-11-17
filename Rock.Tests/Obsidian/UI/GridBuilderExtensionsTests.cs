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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Model;
using Rock.Net;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Tests.Obsidian.UI
{
    /// <summary>
    /// Tests that Obsidian list grids only send attribute columns and values
    /// for attributes the person making the request is allowed to view.
    /// </summary>
    [TestClass]
    public class GridBuilderExtensionsTests
    {
        private static readonly Person _viewer = new Person { Id = 1001 };

        [TestCleanup]
        public void TestCleanup()
        {
            new RockRequestContextAccessor().RockRequestContext = null;
        }

        #region Helpers

        /// <summary>
        /// Sets the ambient request context to one whose current person is
        /// the given person.
        /// </summary>
        /// <param name="person">The person making the request.</param>
        private static void SetRequestPerson( Person person )
        {
            var requestContext = new Mock<RockRequestContext>();

            requestContext.SetupGet( c => c.CurrentPerson ).Returns( person );

            new RockRequestContextAccessor().RockRequestContext = requestContext.Object;
        }

        /// <summary>
        /// Creates an attribute whose VIEW check is answered by the given function.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        /// <param name="canView">Returns <c>true</c> if the person may view the attribute.</param>
        /// <returns>The attribute.</returns>
        private static AttributeCache CreateAttribute( string key, Func<Person, bool> canView )
        {
            var attribute = new Mock<AttributeCache> { CallBase = true };

            attribute.Setup( a => a.IsAuthorized( It.IsAny<string>(), It.IsAny<Person>() ) )
                .Returns( ( string action, Person person ) => action == Authorization.VIEW && canView( person ) );

            typeof( AttributeCache ).GetProperty( nameof( AttributeCache.Key ) ).SetValue( attribute.Object, key );
            typeof( AttributeCache ).GetProperty( nameof( AttributeCache.Name ) ).SetValue( attribute.Object, key );

            return attribute.Object;
        }

        #endregion

        [TestMethod]
        public void AddAttributeFieldsFrom_ViewDenied_OmitsColumnDefinition()
        {
            SetRequestPerson( _viewer );

            var attributes = new[]
            {
                CreateAttribute( "Allowed", person => true ),
                CreateAttribute( "Denied", person => false )
            };

            var definition = new GridBuilder<DefinedValue>()
                .AddAttributeFieldsFrom( item => item, attributes )
                .BuildDefinition();

            CollectionAssert.AreEqual( new[] { "attr_Allowed" }, definition.AttributeFields.Select( f => f.Name ).ToArray() );
        }

        [TestMethod]
        public void AddAttributeFieldsFrom_ViewDenied_OmitsRowValue()
        {
            SetRequestPerson( _viewer );

            var attributes = new[]
            {
                CreateAttribute( "Allowed", person => true ),
                CreateAttribute( "Denied", person => false )
            };

            var data = new GridBuilder<DefinedValue>()
                .AddAttributeFieldsFrom( item => item, attributes )
                .Build( new[] { new DefinedValue() } );

            var row = data.Rows.Single();

            Assert.IsTrue( row.ContainsKey( "attr_Allowed" ) );
            Assert.IsFalse( row.ContainsKey( "attr_Denied" ) );
        }

        [TestMethod]
        public void AddAttributeFieldsFrom_ChecksTheRequestPerson()
        {
            SetRequestPerson( _viewer );

            var attributes = new[]
            {
                CreateAttribute( "ViewerOnly", person => person?.Id == _viewer.Id ),
                CreateAttribute( "OtherOnly", person => person != null && person.Id != _viewer.Id )
            };

            var definition = new GridBuilder<DefinedValue>()
                .AddAttributeFields( attributes )
                .BuildDefinition();

            CollectionAssert.AreEqual( new[] { "attr_ViewerOnly" }, definition.AttributeFields.Select( f => f.Name ).ToArray() );
        }

        [TestMethod]
        public void AddAttributeFieldsFrom_NoRequestContext_ChecksAsAnonymous()
        {
            var attributes = new[]
            {
                CreateAttribute( "Public", person => true ),
                CreateAttribute( "SignedInOnly", person => person != null )
            };

            var definition = new GridBuilder<DefinedValue>()
                .AddAttributeFieldsFrom( item => item, attributes )
                .BuildDefinition();

            CollectionAssert.AreEqual( new[] { "attr_Public" }, definition.AttributeFields.Select( f => f.Name ).ToArray() );
        }
    }
}
