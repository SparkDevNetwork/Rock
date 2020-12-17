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
using System.Threading.Tasks;
using System.Web;
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
    public abstract class MediumComponent : Component, IAsyncMediumComponent
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
                        var entityType = EntityTypeCache.Get( component.GetType() );
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
                int mediumEntityTypeId = EntityTypeCache.Get( this.GetType() ).Id;

                // Add the Medium's settings as attributes for the Transport to use.
                var mediumAttributes = GetMediumAttributes();

                // If there have not been any EnabledLavaCommands explicitly set, then use the global defaults.
                if ( rockMessage.EnabledLavaCommands == null )
                {
                    rockMessage.EnabledLavaCommands = GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );
                }

                if ( rockMessage.CurrentPerson == null )
                {
                    rockMessage.CurrentPerson = HttpContext.Current?.Items["CurrentPerson"] as Person;
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
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <returns></returns>
        public virtual async Task<SendMessageResult> SendAsync( RockMessage rockMessage )
        {
            if ( this.IsActive )
            {
                // Get the Medium's Entity Type Id
                int mediumEntityTypeId = EntityTypeCache.Get( this.GetType() ).Id;

                // Add the Medium's settings as attributes for the Transport to use.
                var mediumAttributes = GetMediumAttributes();

                // If there have not been any EnabledLavaCommands explicitly set, then use the global defaults.
                if ( rockMessage.EnabledLavaCommands == null )
                {
                    rockMessage.EnabledLavaCommands = GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );
                }

                // Use the transport to send communication
                var transport = Transport;
                if ( transport != null && transport.IsActive )
                {
                    var asyncTransport = transport as IAsyncTransport;

                    if ( asyncTransport == null )
                    {
                        var messageResult = new SendMessageResult
                        {
                            MessagesSent = transport.Send( rockMessage, mediumEntityTypeId, mediumAttributes, out var errorMessage ) ? rockMessage.GetRecipients().Count : 0
                        };
                        return await Task.FromResult( messageResult );
                    }
                    else
                    {
                        return await asyncTransport.SendAsync( rockMessage, mediumEntityTypeId, mediumAttributes ).ConfigureAwait( false );
                    }
                }
                else
                {
                    return new SendMessageResult
                    {
                        Errors = new List<string> { "Invalid or Inactive Transport." }
                    };
                }
            }
            else
            {
                return new SendMessageResult
                {
                    Errors = new List<string> { "Inactive Medium." }
                };
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
                int mediumEntityTypeId = EntityTypeCache.Get( this.GetType() ).Id;

                // Add the Medium's settings as attributes for the Transport to use.
                var mediumAttributes = GetMediumAttributes();

                // Use the transport to send communication
                if ( Transport != null && Transport.IsActive )
                {
                    Transport.Send( communication, mediumEntityTypeId, mediumAttributes );
                }
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public virtual async Task SendAsync( Model.Communication communication )
        {
            if ( this.IsActive )
            {
                // Get the Medium's Entity Type Id
                int mediumEntityTypeId = EntityTypeCache.Get( this.GetType() ).Id;

                // Add the Medium's settings as attributes for the Transport to use.
                var mediumAttributes = GetMediumAttributes();

                // Use the transport to send communication
                var transport = Transport;
                if ( transport != null && transport.IsActive )
                {
                    var asyncTransport = transport as IAsyncTransport;

                    if ( asyncTransport == null )
                    {
                        transport.Send( communication, mediumEntityTypeId, mediumAttributes );
                    }
                    else
                    {
                        await asyncTransport.SendAsync( communication, mediumEntityTypeId, mediumAttributes ).ConfigureAwait( false );
                    }
                }
            }
        }

        private Dictionary<string, string> GetMediumAttributes()
        {
            var mediumAttributes = new Dictionary<string, string>();
            foreach ( var attr in this.Attributes.Select( a => a.Value ) )
            {
                string value = this.GetAttributeValue( attr.Key );
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    mediumAttributes.Add( attr.Key, GetAttributeValue( attr.Key ) );
                }
            }

            return mediumAttributes;
        }
    }
}