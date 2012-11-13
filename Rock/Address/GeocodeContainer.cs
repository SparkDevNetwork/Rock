//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using Rock.Extension;

namespace Rock.Address
{
    /// <summary>
    /// Singleton class that uses MEF to load and cache all of the GeocodeComponent classes
    /// </summary>
    public class GeocodeContainer : ContainerManaged<GeocodeComponent, IComponentData>
    {
        #region Singleton Support

        private static GeocodeContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static GeocodeContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new GeocodeContainer();
                return instance;
            }
        }

        private GeocodeContainer()
        {
            Refresh();
        }

        #endregion

        #region MEF Support

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( GeocodeComponent ) )]
        protected override IEnumerable<Lazy<GeocodeComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore

        #endregion
    }
}