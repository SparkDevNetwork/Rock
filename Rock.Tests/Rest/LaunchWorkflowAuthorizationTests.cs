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
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Model;
using Rock.Security;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests the REST API check that a person may launch workflows of a
    /// workflow type before the LaunchWorkflow and WorkflowEntry endpoints
    /// start one.
    /// </summary>
    /// <remarks>
    /// The helper is read by reflection because ApiControllerBase derives
    /// from a Web API type that this project does not reference directly.
    /// </remarks>
    [TestClass]
    public class LaunchWorkflowAuthorizationTests
    {
        private static readonly Assembly _restAssembly = Assembly.Load( "Rock.Rest" );

        private static readonly Type _apiControllerBaseType = _restAssembly.GetType( "Rock.Rest.ApiControllerBase", true );

        #region Helpers

        private static MethodInfo GetCanLaunchWorkflowTypeMethod()
        {
            return _apiControllerBaseType.GetMethod( "CanLaunchWorkflowType", BindingFlags.NonPublic | BindingFlags.Static );
        }

        private static bool CanLaunchWorkflowType( ISecured workflowType, Person person )
        {
            var method = GetCanLaunchWorkflowTypeMethod();

            Assert.IsNotNull( method, "ApiControllerBase.CanLaunchWorkflowType was not found." );

            return ( bool ) method.Invoke( null, new object[] { workflowType, person } );
        }

        private static string GetParameterList( MethodInfo method )
        {
            return string.Join( ", ", method.GetParameters().Select( p => p.ParameterType.Name ) );
        }

        #endregion

        #region CanLaunchWorkflowType

        [TestMethod]
        public void CanLaunchWorkflowType_Exists_AsNonPublicStaticHelper()
        {
            var method = GetCanLaunchWorkflowTypeMethod();

            Assert.IsNotNull( method, "ApiControllerBase.CanLaunchWorkflowType was not found." );
            Assert.IsFalse( method.IsPublic, "The helper must not be public so no new REST action is created." );
            Assert.AreEqual( typeof( bool ), method.ReturnType );
        }

        [TestMethod]
        public void CanLaunchWorkflowType_NullWorkflowType_ReturnsFalse()
        {
            Assert.IsFalse( CanLaunchWorkflowType( null, new Person() ) );
        }

        [TestMethod]
        public void CanLaunchWorkflowType_PersonWithoutView_ReturnsFalse()
        {
            var person = new Person();
            var workflowType = new Mock<ISecured>();
            workflowType.Setup( t => t.IsAuthorized( It.IsAny<string>(), It.IsAny<Person>() ) ).Returns( false );

            Assert.IsFalse( CanLaunchWorkflowType( workflowType.Object, person ) );
        }

        [TestMethod]
        public void CanLaunchWorkflowType_PersonWithView_ReturnsTrueAndChecksView()
        {
            var person = new Person();
            var workflowType = new Mock<ISecured>();
            workflowType.Setup( t => t.IsAuthorized( Authorization.VIEW, person ) ).Returns( true );

            Assert.IsTrue( CanLaunchWorkflowType( workflowType.Object, person ) );

            workflowType.Verify( t => t.IsAuthorized( Authorization.VIEW, person ), Times.Once() );
            workflowType.Verify( t => t.IsAuthorized( It.Is<string>( a => a != Authorization.VIEW ), It.IsAny<Person>() ), Times.Never() );
        }

        [TestMethod]
        public void CanLaunchWorkflowType_NullPersonWhenAllUsersCanView_ReturnsTrue()
        {
            var workflowType = new Mock<ISecured>();
            workflowType.Setup( t => t.IsAuthorized( Authorization.VIEW, null ) ).Returns( true );

            Assert.IsTrue( CanLaunchWorkflowType( workflowType.Object, null ) );
        }

        #endregion

        #region Signatures

        [TestMethod]
        public void LaunchWorkflow_PublicSignatures_AreUnchanged()
        {
            var apiControllerType = _restAssembly.GetType( "Rock.Rest.ApiController`1", true );

            var signatures = apiControllerType
                .GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                .Where( m => m.Name == "LaunchWorkflow" )
                .Select( m => GetParameterList( m ) )
                .OrderBy( s => s )
                .ToList();

            var expected = new List<string>
            {
                "Int32, Guid, String, Dictionary`2",
                "Int32, Int32, String, Dictionary`2"
            };

            CollectionAssert.AreEqual( expected, signatures );

            foreach ( var method in apiControllerType.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ).Where( m => m.Name == "LaunchWorkflow" ) )
            {
                Assert.AreEqual( typeof( void ), method.ReturnType );
                Assert.AreEqual( typeof( Dictionary<string, string> ), method.GetParameters()[3].ParameterType );
            }
        }

        [TestMethod]
        public void WorkflowEntry_PublicSignature_IsUnchanged()
        {
            var workflowsControllerType = _restAssembly.GetType( "Rock.Rest.Controllers.WorkflowsController", true );

            var methods = workflowsControllerType
                .GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                .Where( m => m.Name == "WorkflowEntry" )
                .ToList();

            Assert.AreEqual( 1, methods.Count );
            Assert.AreEqual( "Int32", GetParameterList( methods[0] ) );
            Assert.AreEqual( typeof( Rock.Model.Workflow ), methods[0].ReturnType );
        }

        #endregion
    }
}
