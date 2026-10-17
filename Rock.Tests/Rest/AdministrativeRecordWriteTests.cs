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
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests the v1 REST controllers that write records the Rock UI treats as
    /// administrator settings: blocks, attribute qualifiers, REST actions and
    /// REST controllers. Writes to these must require ADMINISTRATE, and the
    /// fix must not add or change any action method, since the stored REST
    /// security rules are keyed by the action method signature.
    /// </summary>
    /// <remarks>
    /// The controllers are read by reflection because they derive from a Web
    /// API type that this project does not reference directly.
    /// </remarks>
    [TestClass]
    public class AdministrativeRecordWriteTests
    {
        private static readonly Assembly _restAssembly = Assembly.Load( "Rock.Rest" );

        #region Helpers

        private static Type GetControllerType( string name )
        {
            return _restAssembly.GetType( "Rock.Rest.Controllers." + name, true );
        }

        private static bool HasSingleBlockParent( int? pageId, int? layoutId, int? siteId )
        {
            var method = GetControllerType( "BlocksController" ).GetMethod( "HasSingleBlockParent", BindingFlags.NonPublic | BindingFlags.Static );

            Assert.IsNotNull( method, "BlocksController.HasSingleBlockParent was not found." );

            return ( bool ) method.Invoke( null, new object[] { pageId, layoutId, siteId } );
        }

        private static void AssertDeclaresProtectedOverride( string controllerName, string methodName, Type parameterType )
        {
            var method = GetControllerType( controllerName ).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null,
                new[] { parameterType },
                null );

            Assert.IsNotNull( method, $"{controllerName} does not override {methodName}( {parameterType.Name} )." );
            Assert.IsTrue( method.IsFamily, $"{controllerName}.{methodName} must be protected." );
            Assert.AreNotEqual( method.DeclaringType, method.GetBaseDefinition().DeclaringType, $"{controllerName}.{methodName} must override the base method." );
        }

        private static string[] GetDeclaredPublicActionNames( string controllerName )
        {
            return GetControllerType( controllerName )
                .GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                .Where( m => !m.IsSpecialName )
                .Select( m => m.Name )
                .OrderBy( n => n )
                .ToArray();
        }

        #endregion

        #region Block Placement

        [TestMethod]
        public void HasSingleBlockParent_PageOnly_IsTrue()
        {
            Assert.IsTrue( HasSingleBlockParent( 1, null, null ) );
        }

        [TestMethod]
        public void HasSingleBlockParent_LayoutOnly_IsTrue()
        {
            Assert.IsTrue( HasSingleBlockParent( null, 2, null ) );
        }

        [TestMethod]
        public void HasSingleBlockParent_SiteOnly_IsTrue()
        {
            Assert.IsTrue( HasSingleBlockParent( null, null, 3 ) );
        }

        [TestMethod]
        public void HasSingleBlockParent_NoParent_IsFalse()
        {
            Assert.IsFalse( HasSingleBlockParent( null, null, null ) );
        }

        [TestMethod]
        public void HasSingleBlockParent_TwoParents_IsFalse()
        {
            Assert.IsFalse( HasSingleBlockParent( 1, 2, null ) );
        }

        [TestMethod]
        public void HasSingleBlockParent_ThreeParents_IsFalse()
        {
            Assert.IsFalse( HasSingleBlockParent( 1, 2, 3 ) );
        }

        #endregion

        #region Overrides

        [TestMethod]
        public void BlocksController_OverridesCheckCanEditAfterUpdate()
        {
            AssertDeclaresProtectedOverride( "BlocksController", "CheckCanEditAfterUpdate", typeof( Block ) );
        }

        [TestMethod]
        public void AttributeQualifiersController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "AttributeQualifiersController", "CheckCanEdit", typeof( AttributeQualifier ) );
        }

        [TestMethod]
        public void AttributeQualifiersController_OverridesCheckCanEditAfterUpdate()
        {
            AssertDeclaresProtectedOverride( "AttributeQualifiersController", "CheckCanEditAfterUpdate", typeof( AttributeQualifier ) );
        }

        [TestMethod]
        public void RestActionsController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "RestActionsController", "CheckCanEdit", typeof( RestAction ) );
        }

        [TestMethod]
        public void RestActionsController_OverridesCheckCanEditAfterUpdate()
        {
            AssertDeclaresProtectedOverride( "RestActionsController", "CheckCanEditAfterUpdate", typeof( RestAction ) );
        }

        [TestMethod]
        public void RestControllersController_OverridesCheckCanEdit()
        {
            AssertDeclaresProtectedOverride( "RestControllersController", "CheckCanEdit", typeof( RestController ) );
        }

        #endregion

        #region Action Methods

        [TestMethod]
        public void BlocksController_PublicActions_AreUnchanged()
        {
            var expected = new[] { "BlockAction", "BlockActionAsPost", "Delete", "Move", "Post" }.OrderBy( n => n ).ToArray();

            CollectionAssert.AreEqual( expected, GetDeclaredPublicActionNames( "BlocksController" ) );
        }

        [TestMethod]
        public void BlocksController_PostOverride_KeepsBaseApiId()
        {
            var controllerType = GetControllerType( "BlocksController" );
            var post = controllerType.GetMethod( "Post", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNotNull( post, "BlocksController does not override Post." );

            Assert.AreNotEqual( controllerType, post.GetBaseDefinition().DeclaringType, "BlocksController.Post must override the base Post." );

            // The ApiId of a REST action is built from MethodInfo.ToString(), so the
            // override must match the base action for stored REST security to stay attached.
            var basePost = controllerType.BaseType.GetMethod( "Post", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNotNull( basePost );
            Assert.AreEqual( basePost.ToString(), post.ToString() );
            Assert.IsNull( post.GetCustomAttribute<Rock.SystemGuid.RestActionGuidAttribute>( false ) );
        }

        [TestMethod]
        public void BlocksController_DeleteOverride_KeepsBaseApiId()
        {
            var controllerType = GetControllerType( "BlocksController" );
            var delete = controllerType.GetMethod( "Delete", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNotNull( delete, "BlocksController does not override Delete." );

            Assert.AreNotEqual( controllerType, delete.GetBaseDefinition().DeclaringType, "BlocksController.Delete must override the base Delete." );

            var baseDelete = controllerType.BaseType.GetMethod( "Delete", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNotNull( baseDelete );
            Assert.AreEqual( baseDelete.ToString(), delete.ToString() );
            Assert.IsNull( delete.GetCustomAttribute<Rock.SystemGuid.RestActionGuidAttribute>( false ) );
        }

        [TestMethod]
        public void AttributeQualifiersController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( "AttributeQualifiersController" ) );
        }

        [TestMethod]
        public void RestActionsController_PublicActions_AreUnchanged()
        {
            CollectionAssert.AreEqual( new string[0], GetDeclaredPublicActionNames( "RestActionsController" ) );
        }

        [TestMethod]
        public void RestControllersController_PublicActions_AreUnchanged()
        {
            var expected = new[] { "EnsureRestControllers", "RestControllerNames" };

            CollectionAssert.AreEqual( expected, GetDeclaredPublicActionNames( "RestControllersController" ) );
        }

        #endregion

        #region Inline Attribute Qualifiers

        [TestMethod]
        public void AttributeWithInlineQualifiers_IsRejected()
        {
            var apiControllerBaseType = _restAssembly.GetType( "Rock.Rest.ApiControllerBase", true );
            var method = apiControllerBaseType.GetMethod( "GetUntrackedRelatedEntityPropertyName", BindingFlags.NonPublic | BindingFlags.Static );

            Assert.IsNotNull( method, "ApiControllerBase.GetUntrackedRelatedEntityPropertyName was not found." );

            var attribute = new Rock.Model.Attribute();
            attribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "values", Value = "A,B" } );

            var propertyName = ( string ) method.Invoke( null, new object[] { attribute, null, null } );

            Assert.AreEqual( nameof( Rock.Model.Attribute.AttributeQualifiers ), propertyName );
        }

        [TestMethod]
        public void AttributesController_DoesNotAllowInlineQualifiers()
        {
            var method = GetControllerType( "AttributesController" ).GetMethod(
                "IsRelatedEntityCollectionAllowed",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNull( method, "AttributesController must not allow related records to be posted inline." );
        }

        #endregion
    }
}
