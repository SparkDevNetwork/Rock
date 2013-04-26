//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;

namespace Rock.Communication
{
    /// <summary>
    /// MEF Container class for Communication Channel Componenets
    /// </summary>
    public class ChannelContainer : Container<ChannelComponent, IComponentData>
    {
        private static ChannelContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ChannelContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new ChannelContainer();
                return instance;
            }
        }

        private ChannelContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static ChannelComponent GetComponent( string entityTypeName )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.TypeName == entityTypeName )
                {
                    return component;
                }
            }

            return null;
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( ChannelComponent ) )]
        protected override IEnumerable<Lazy<ChannelComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}