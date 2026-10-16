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

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests that the v1 BinaryFileDatas REST controller replaces every
    /// inherited read action that returns file bytes, so each one runs the
    /// file security check before the bytes are returned.
    /// </summary>
    /// <remarks>
    /// The controller is read by reflection because it derives from a Web API
    /// type that this project does not reference directly.
    /// </remarks>
    [TestClass]
    public class BinaryFileDatasControllerTests
    {
        private static readonly Type _controllerType = Assembly.Load( "Rock.Rest" ).GetType( "Rock.Rest.Controllers.BinaryFileDatasController", true );

        private const string ActionNameAttributeName = "System.Web.Http.ActionNameAttribute";

        private const string RouteAttributeName = "System.Web.Http.RouteAttribute";

        private const string RestActionGuidAttributeName = "Rock.SystemGuid.RestActionGuidAttribute";

        /// <summary>
        /// The read actions that return file bytes, by name and parameter types.
        /// </summary>
        private static readonly Tuple<string, Type[]>[] _readActions = new[]
        {
            Tuple.Create( "Get", Type.EmptyTypes ),
            Tuple.Create( "GetById", new[] { typeof( int ) } ),
            Tuple.Create( "Get", new[] { typeof( int ) } ),
            Tuple.Create( "GetByAttributeValue", new[] { typeof( int? ), typeof( string ), typeof( string ), typeof( bool ) } ),
            Tuple.Create( "GetDataView", new[] { typeof( int ) } ),
            Tuple.Create( "GetFollowedItems", new[] { typeof( int? ), typeof( int? ) } )
        };

        #region Helpers

        private static MethodInfo GetControllerMethod( Tuple<string, Type[]> action )
        {
            var method = _controllerType.GetMethod( action.Item1, BindingFlags.Public | BindingFlags.Instance, null, action.Item2, null );

            Assert.IsNotNull( method, $"{action.Item1} was not found." );
            Assert.AreEqual( _controllerType, method.DeclaringType, $"{method} is not overridden by BinaryFileDatasController." );

            return method;
        }

        private static MethodInfo GetBaseMethod( Tuple<string, Type[]> action )
        {
            return _controllerType.BaseType.GetMethod( action.Item1, BindingFlags.Public | BindingFlags.Instance, null, action.Item2, null );
        }

        private static string[] GetAttributeDescriptions( MethodInfo method )
        {
            return method.GetCustomAttributes( false )
                .Select( a => a.GetType().FullName == ActionNameAttributeName
                    ? $"{ActionNameAttributeName}({a.GetType().GetProperty( "Name" ).GetValue( a )})"
                    : a.GetType().FullName )
                .OrderBy( a => a )
                .ToArray();
        }

        private static bool IsFileViewAllowed( bool? parentEntityAllowsView, bool requiresViewSecurity, bool isAuthorizedToView )
        {
            var method = _controllerType.GetMethod( "IsFileViewAllowed", BindingFlags.NonPublic | BindingFlags.Static );

            Assert.IsNotNull( method, "IsFileViewAllowed was not found." );

            Func<bool> isAuthorized = () => isAuthorizedToView;

            return ( bool ) method.Invoke( null, new object[] { parentEntityAllowsView, requiresViewSecurity, isAuthorized } );
        }

        #endregion

        #region Read Action Tests

        [TestMethod]
        public void ReadActions_AreDeclaredOnBinaryFileDatasController()
        {
            foreach ( var action in _readActions )
            {
                GetControllerMethod( action );
            }
        }

        [TestMethod]
        public void ReadActions_RepeatBaseAttributes()
        {
            foreach ( var action in _readActions )
            {
                var method = GetControllerMethod( action );
                var attributes = GetAttributeDescriptions( method );

                CollectionAssert.AreEqual( GetAttributeDescriptions( GetBaseMethod( action ) ), attributes, $"{method} does not repeat the base attributes." );
                CollectionAssert.Contains( attributes, "Rock.Rest.Filters.AuthenticateAttribute", $"{method} is missing Authenticate." );
                CollectionAssert.Contains( attributes, "Rock.Rest.Filters.SecuredAttribute", $"{method} is missing Secured." );
            }
        }

        [TestMethod]
        public void ReadActions_HaveNoRouteOrRestActionGuid()
        {
            foreach ( var action in _readActions )
            {
                var method = GetControllerMethod( action );
                var attributeNames = method.GetCustomAttributes( false ).Select( a => a.GetType().FullName ).ToList();

                CollectionAssert.DoesNotContain( attributeNames, RouteAttributeName, $"{method} must use the conventional route." );
                CollectionAssert.DoesNotContain( attributeNames, RestActionGuidAttributeName, $"{method} must keep its ApiId based security." );
            }
        }

        [TestMethod]
        public void ReadActions_KeepBaseApiId()
        {
            foreach ( var action in _readActions )
            {
                var method = GetControllerMethod( action );

                Assert.AreEqual( GetBaseMethod( action ).ToString(), method.ToString(), "The ApiId would change." );
            }
        }

        #endregion

        #region File View Decision Tests

        [TestMethod]
        [DataRow( null, false, false, true, DisplayName = "No parent, type not secured" )]
        [DataRow( null, true, false, false, DisplayName = "No parent, type secured, not authorized" )]
        [DataRow( null, true, true, true, DisplayName = "No parent, type secured, authorized" )]
        [DataRow( true, true, false, true, DisplayName = "Parent allows view" )]
        [DataRow( false, false, true, false, DisplayName = "Parent denies view" )]
        public void IsFileViewAllowed_MatchesGetFile( bool? parentEntityAllowsView, bool requiresViewSecurity, bool isAuthorizedToView, bool expected )
        {
            Assert.AreEqual( expected, IsFileViewAllowed( parentEntityAllowsView, requiresViewSecurity, isAuthorizedToView ) );
        }

        #endregion
    }
}
