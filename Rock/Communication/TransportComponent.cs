//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock;
using Rock.Model;

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Base class for components that perform actions for a workflow
    /// </summary>
    public abstract class TransportComponent : Component
    {
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        public abstract bool Send( Rock.Model.Communication communication );

        public Dictionary<string, object> GetGlobalConfigValues()
        {
            var configValues = new Dictionary<string, object>();

            // Get all the global attribute values that begin with "Organization" or "Email"
            // TODO: We don't want to allow access to all global attribute values, as that would be a security
            // hole, but not sure limiting to those that start with "Organization" or "Email" is the best
            // solution either.
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v =>
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) ||
                    v.Key.StartsWith( "Email", StringComparison.CurrentCultureIgnoreCase ) )
                .ToList()
                .ForEach( v => configValues.Add( v.Key, v.Value.Value ) );

            // Add any application config values as available merge objects
            foreach ( string key in System.Configuration.ConfigurationManager.AppSettings.AllKeys )
            {
                configValues.Add( "Config_" + key, System.Configuration.ConfigurationManager.AppSettings[key] );
            }

            // Recursively resolve any of the config values that may have other merge codes as part of their value
            foreach ( var item in configValues.ToList() )
            {
                configValues[item.Key] = ResolveConfigValue( item.Value as string, configValues );
            }

            return configValues;
        }


        /// <summary>
        /// Resolves the config value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="globalAttributes">The global attributes.</param>
        /// <returns></returns>
        private string ResolveConfigValue( string value, Dictionary<string, object> globalAttributes )
        {
            string result = value.ResolveMergeFields( globalAttributes );

            // If anything was resolved, keep resolving until nothing changed.
            while ( result != value )
            {
                value = result;
                result = ResolveConfigValue( result, globalAttributes );
            }

            return result;
        }
    }
   
}