// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.UI;
using Rock.Attribute;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication
{
    /// <summary>
    /// Base class for components communication channels (i.e. email, sms, twitter, etc) 
    /// </summary>
    [ComponentField( "Rock.Communication.TransportContainer, Rock" )]
    public abstract class ChannelComponent : Component
    {
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public abstract ChannelControl Control { get; }

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
        /// Initializes a new instance of the <see cref="ChannelComponent" /> class.
        /// </summary>
        public ChannelComponent()
        {
            this.LoadAttributes();
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public virtual void Send( Rock.Model.Communication communication )
        {
            var transport = Transport;
            if ( transport != null )
            {
                transport.Send( communication );
            }
        }
    }

}