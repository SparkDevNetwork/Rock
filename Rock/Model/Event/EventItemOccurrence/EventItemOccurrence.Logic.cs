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
using System.ComponentModel.DataAnnotations.Schema;

namespace Rock.Model
{
    public partial class EventItemOccurrence
    {
        #region Properties

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <value>
        /// The next start date time.
        /// </value>
        [NotMapped]
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                if ( Schedule != null )
                {
                    return Schedule.GetNextStartDateTime( RockDateTime.Now );
                }

                return null;
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Gets the start times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<DateTime> GetStartTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            if ( Schedule != null )
            {
                return Schedule.GetScheduledStartTimes( beginDateTime, endDateTime );
            }
            else
            {
                return new List<DateTime>();
            }
        }

        /// <summary>
        /// Gets the first start date time.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetFirstStartDateTime()
        {
            if ( Schedule != null )
            {
                return Schedule.GetFirstStartDateTime();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.EventItem != null )
            {
                return string.Format( "{0} ({1})", this.EventItem.Name,
                    this.Campus != null ? this.Campus.Name : "All Campuses" );
            }

            return base.ToString();
        }

        #endregion
    }
}
