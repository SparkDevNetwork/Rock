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
        /// <param name="CurrentPersonAlias">The current person alias.</param>
        public abstract void Send( Rock.Model.Communication communication, PersonAlias CurrentPersonAlias );

        public abstract void Send( EmailTemplate template, Dictionary<string, Dictionary<string, object>> recipients );

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