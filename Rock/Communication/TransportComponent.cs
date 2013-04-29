//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Extension;
using Rock.Model;

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
        /// <param name="CurrentPersonId">The current person id.</param>
        public abstract void Send( Rock.Model.Communication communication, int? CurrentPersonId );

        /// <summary>
        /// Gets the global config values.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetGlobalConfigValues()
        {
            var configValues = new Dictionary<string, object>();

            // Get all the global attribute values that begin with "Organization" or "Email"
            // TODO: We don't want to allow access to all global attribute values, as that would be a security
            // hole, but not sure limiting to those that start with "Organization" or "Email" is the best
            // solution either.
            var globalAttributeValues = new Dictionary<string, object>();
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v =>
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) ||
                    v.Key.StartsWith( "Email", StringComparison.CurrentCultureIgnoreCase ) )
                .ToList()
                .ForEach( v => globalAttributeValues.Add( v.Key, v.Value.Value ) );

            configValues.Add( "GlobalAttribute", globalAttributeValues );

            // Add any application config values as available merge objects
            var appSettingValues = new Dictionary<string, object>();
            foreach ( string key in System.Configuration.ConfigurationManager.AppSettings.AllKeys )
            {
                appSettingValues.Add( key, System.Configuration.ConfigurationManager.AppSettings[key] );
            }

            configValues.Add( "AppSetting", appSettingValues );

            // Recursively resolve any of the config values that may have other merge codes as part of their value
            foreach ( var collection in configValues.ToList() )
            {
                var collectionDictionary = collection.Value as Dictionary<string, object>;
                foreach ( var item in collectionDictionary.ToList() )
                {
                    collectionDictionary[item.Key] = ResolveConfigValue( item.Value as string, configValues );
                }
            }

            return configValues;
        }


        /// <summary>
        /// Resolves the config value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="globalAttributes">The global attributes.</param>
        /// <returns></returns>
        private string ResolveConfigValue( string value, Dictionary<string, object> configValues )
        {
            string result = value.ResolveMergeFields( configValues );

            // If anything was resolved, keep resolving until nothing changed.
            while ( result != value )
            {
                value = result;
                result = ResolveConfigValue( result, configValues );
            }

            return result;
        }

        /// <summary>
        /// Merges the values.
        /// </summary>
        /// <param name="configValues">The config values.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        protected Dictionary<string, object> MergeValues( Dictionary<string, object> configValues, CommunicationRecipient recipient )
        {
            Dictionary<string, object> mergeValues = new Dictionary<string, object>();

            configValues.ToList().ForEach( v => mergeValues.Add( v.Key, v.Value ) );

            if ( recipient != null )
            {
                if ( recipient.Person != null )
                {
                    mergeValues.Add( "Person", recipient.Person );
                }

                // Add any additional merge fields created through a report
                foreach ( var mergeField in recipient.AdditionalMergeValues )
                {
                    if ( !mergeValues.ContainsKey( mergeField.Key ) )
                    {
                        mergeValues.Add( mergeField.Key, mergeField.Value );
                    }
                }
            }

            return mergeValues;

        }
    }
   
}