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
using System.Web.Http;

using Rock.Model;
using Rock.Pbx;
using Rock.Rest.Filters;
using Rock.Security;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller of misc utility functions that are used by Rock controls
    /// </summary>
    public class PbxController : ApiControllerBase
    {

        /// <summary>
        /// Originates the specified source phone.
        /// </summary>
        /// <param name="sourcePhone">The source phone.</param>
        /// <param name="destinationPhone">The destination phone.</param>
        /// <param name="callerId">The caller identifier.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Pbx/Originate" )]
        [HttpGet]
        [Authenticate, Secured]
        public OriginateResponse Originate( string sourcePhone, string destinationPhone, string callerId = null )
        {
            var response = new OriginateResponse();

            // get the current person
            var currentPerson = GetPerson();
            if ( currentPerson == null )
            {
                response.Success = false;
                response.Message = "You must be logged in to originate a call.";
                return response;
            }

            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( currentPerson );
            if ( pbxComponent == null )
            {
                response.Success = false;
                response.Message = "An active PBX component supporting call origination is secured to allow access.";
                return response;
            }

            string message = null;
            response.Success = pbxComponent.Originate( sourcePhone, destinationPhone, callerId, out message );
            response.Message = message;

            return response;
        }

        /// <summary>
        /// Originates a call using a person guid as the source and a specific destination phone number. This
        /// allows the system to select the source person's preferred source phone number. For instance they
        /// may currently have their mobile number selected as the origination number.
        /// </summary>
        /// <param name="sourcePersonGuid">The source person unique identifier.</param>
        /// <param name="destinationPhone">The destination phone.</param>
        /// <param name="callerId">The caller identifier.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Pbx/Originate" )]
        [HttpGet]
        [Authenticate, Secured]
        public OriginateResponse Originate( Guid sourcePersonGuid, string destinationPhone, string callerId = null )
        {
            var response = new OriginateResponse();

            // get the current person
            var currentPerson = GetPerson();
            if ( currentPerson == null )
            {
                response.Success = false;
                response.Message = "You must be logged in to originate a call.";
                return response;
            }

            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( currentPerson );
            if ( pbxComponent == null )
            {
                response.Success = false;
                response.Message = "An active PBX component supporting call origination is secured to allow access.";
                return response;
            }

            // get the source person object ( the source could be different from the current person)
            var sourcePerson = new PersonService( new Data.RockContext() ).Get( sourcePersonGuid );

            string message = null;
            response.Success = pbxComponent.Originate( sourcePerson, destinationPhone, callerId, out message );
            response.Message = message;

            return response;
        }

        /// <summary>
        /// Return object of the Originate action
        /// </summary>
        public class OriginateResponse
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="OriginateResponse"/> is success.
            /// </summary>
            /// <value>
            ///   <c>true</c> if success; otherwise, <c>false</c>.
            /// </value>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message { get; set; }
        }

        /// <summary>
        /// Gets the PBX component.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        private PbxComponent GetPbxComponent(Person currentPerson )
        {
            // check that a pbx component is active
            var pbxComponent = Rock.Pbx.PbxContainer.GetActiveComponent();
            if ( pbxComponent == null )
            {
                return null;
            }

            // check that this person is allowed to access this component
            if ( !Rock.Security.Authorization.Authorized( pbxComponent, Authorization.VIEW, currentPerson ) )
            {
                return pbxComponent;
            }

            return null;
        }
    }
}
