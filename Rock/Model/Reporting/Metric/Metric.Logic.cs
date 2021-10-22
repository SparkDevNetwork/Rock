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

namespace Rock.Model
{
    public partial class Metric
    {
        /// <summary>
        /// Gets the merge objects that can be used in the SourceSql
        /// </summary>
        /// <param name="runDateTime">The run date time. Note, this is the scheduled run date time, not the current datetime</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMergeObjects( DateTime runDateTime )
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "RunDateTime", runDateTime );
            mergeObjects.Add( "Metric", this );

            return mergeObjects;
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // Because a metric can belong to more than one category, security for a metric is handled a bit differently. 
            // If the user is specifically granted or denied access at the metric level, that will overrule any security defined
            // at any of the categories that metric belongs to.


            // First check for security on the metric
            bool? isAuthorized = Security.Authorization.AuthorizedForEntity( this, action, person );
            if ( isAuthorized.HasValue )
            {
                return isAuthorized.Value;
            }

            // If metric belongs to any categories, give them access if they have access to any of the categories (even if 
            // one or more denies them access). If not granted access by a category, check to see if any category denies 
            // them access.
            if ( this.MetricCategories != null )
            {
                bool? denied = null;
                foreach ( var metricCategory in this.MetricCategories )
                {
                    var categoryAuthorized = Security.Authorization.AuthorizedForEntity( metricCategory.Category, action, person, true );
                    if ( categoryAuthorized.HasValue )
                    {
                        if ( categoryAuthorized.Value )
                        {
                            return true;
                        }
                        else
                        {
                            denied = false;
                        }
                    }
                }
                if ( denied.HasValue )
                {
                    return false;
                }
            }

            return base.IsAuthorized( action, person );
        }
    }
}
