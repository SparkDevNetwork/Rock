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
        /// <param name="CurrentPersonId">The current person id.</param>
        public abstract void Send( Rock.Model.Communication communication, int? CurrentPersonId );

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