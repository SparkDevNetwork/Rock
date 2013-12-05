//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;

using Rock.Attribute;
using Rock.Extension;
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
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType
        {
            get
            {
                return EntityTypeCache.Read( this.GetType() );
            }
        }

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

    }

}