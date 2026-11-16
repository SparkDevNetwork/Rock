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

using Rock.Security;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests that the v2 REST controllers for records that carry student grades
    /// only allow callers granted the unrestricted read or write actions.
    /// </summary>
    /// <remarks>
    /// The controllers are read by reflection because they derive from a Web
    /// API type that this project does not reference directly.
    /// </remarks>
    [TestClass]
    public class LearningGradeControllersSecurityTests
    {
        private static readonly Assembly _restAssembly = Assembly.Load( "Rock.Rest" );

        private const string SecuredAttributeName = "Rock.Rest.Filters.SecuredAttribute";

        /// <summary>
        /// Search by key actions always enforce entity search security, so they keep the standard read action.
        /// </summary>
        private static readonly string[] _searchByKeyActions = new[] { "GetSearchByKey", "PostSearchByKey" };

        [DataTestMethod]
        [DataRow( "Rock.Rest.v2.Models.LearningClassActivityCompletionsController" )]
        [DataRow( "Rock.Rest.v2.Models.LearningParticipantsController" )]
        public void Actions_RequireUnrestrictedSecurityAction( string controllerTypeName )
        {
            var controllerType = _restAssembly.GetType( controllerTypeName, true );
            var actions = controllerType.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                .Where( m => !_searchByKeyActions.Contains( m.Name ) )
                .ToList();

            Assert.IsTrue( actions.Any(), $"{controllerTypeName} has no actions." );

            foreach ( var action in actions )
            {
                var securedAttribute = action.GetCustomAttributes( false )
                    .FirstOrDefault( a => a.GetType().FullName == SecuredAttributeName );

                Assert.IsNotNull( securedAttribute, $"{action.Name} is missing Secured." );

                var securityAction = ( string ) securedAttribute.GetType().GetProperty( "SecurityAction" ).GetValue( securedAttribute );
                var isUnrestricted = securityAction == Authorization.EXECUTE_UNRESTRICTED_READ
                    || securityAction == Authorization.EXECUTE_UNRESTRICTED_WRITE;

                Assert.IsTrue( isUnrestricted, $"{controllerTypeName}.{action.Name} is secured with {securityAction}." );
            }
        }
    }
}
