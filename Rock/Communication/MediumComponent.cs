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

using Rock.Attribute;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication
{
    /// <summary>
    /// Base class for components communication mediums (i.e. email, sms, twitter, etc) 
    /// </summary>
    [ComponentField( "Rock.Communication.TransportContainer, Rock", "Transport Container", "", false, "", "", 1 )]
    public abstract class MediumComponent : Component
    {
        /// <summary>
        /// Gets the transport.
        /// </summary>
        /// <value>
        /// The transport.
        /// </value>
        public TransportComponent Transport
        {
            get
            {
                Guid entityTypeGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "TransportContainer" ), out entityTypeGuid ) )
                {
                    foreach ( var serviceEntry in TransportContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        var entityType = EntityTypeCache.Read( component.GetType() );
                        if ( entityType != null && entityType.Guid.Equals( entityTypeGuid ) )
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediumComponent" /> class.
        /// </summary>
        public MediumComponent() : base( false )
        {
            this.LoadAttributes();
        }

        /// <summary>
        /// Gets the type of the communication.
        /// </summary>
        /// <value>
        /// The type of the communication.
        /// </value>
        public abstract CommunicationType CommunicationType { get; }

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public abstract MediumControl GetControl( bool useSimpleMode );

        /// <summary>
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="errorMessages">The error messages.</param>
        public virtual void Send( RockMessage rockMessage, out List<string> errorMessages )
        {
            if ( this.IsActive )
            {
                // Get the Medium's Entity Type Id
                int mediumEntityTypeId = EntityTypeCache.Read( this.GetType() ).Id;

                // Add the Medium's settings as attributes for the Transport to use.
                var mediumAttributes = new Dictionary<string, string>();
                foreach ( var attr in this.Attributes.Select( a => a.Value ) )
                {
                    string value = this.GetAttributeValue( attr.Key );
                    if ( value.IsNotNullOrWhitespace() )
                    {
                        mediumAttributes.Add( attr.Key, GetAttributeValue( attr.Key ) );
                    }
                }

                // Use the transport to send communication
                var transport = Transport;
                if ( transport != null && transport.IsActive )
                {
                    transport.Send( rockMessage, mediumEntityTypeId, mediumAttributes, out errorMessages );
                }
                else
                {
                    errorMessages = new List<string> { "Invalid or Inactive Transport." };
                }
            }
            else
            {
                errorMessages = new List<string> { "Inactive Medium." };
            }
        }
        
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public virtual void Send( Model.Communication communication )
        {
            if ( this.IsActive )
            {
                // Get the Medium's Entity Type Id
                int mediumEntityTypeId = EntityTypeCache.Read( this.GetType() ).Id;

                // Add the Medium's settings as attributes for the Transport to use.
                var mediumAttributes = new Dictionary<string, string>();
                foreach ( var attr in this.Attributes.Select( a => a.Value ) )
                {
                    string value = this.GetAttributeValue( attr.Key );
                    if ( value.IsNotNullOrWhitespace() )
                    {
                        mediumAttributes.Add( attr.Key, GetAttributeValue( attr.Key ) );
                    }
                }

                // Use the transport to send communication
                var transport = Transport;
                if ( transport != null && transport.IsActive )
                {
                    transport.Send( communication, mediumEntityTypeId, mediumAttributes );
                }
            }
        }

        #region Obsolete 

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [Obsolete("The GetCommunication now creates the HTML Preview directly")]
        public abstract string GetHtmlPreview( Model.Communication communication, Person person );

        /// <summary>
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        [Obsolete( "The CommunicationDetail block now creates the details" )]
        public abstract string GetMessageDetails( Model.Communication communication );

        /// <summary>
        /// Gets a value indicating whether [supports bulk communication].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports bulk communication]; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "All mediums now support bulk communications")]
        public abstract bool SupportsBulkCommunication
        {
            get;
        }

        #endregion

    }

}