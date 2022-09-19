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
using System.ComponentModel.DataAnnotations.Schema;
using Rock.Lava;

namespace Rock.Model
{
    public partial class ServiceJobHistory
    {
        /// <summary>
        /// Gets the status message as HTML.
        /// </summary>
        /// <value>
        /// The status message as HTML.
        /// </value>
        [LavaVisible]
        [NotMapped]
        public string StatusMessageAsHtml
        {
            get
            {
                return StatusMessage.ConvertCrLfToHtmlBr();
            }
        }

        /// <summary>
        /// Gets the job duration in seconds.
        /// </summary>
        /// <value>
        /// The job duration in seconds.
        /// </value>
        [LavaVisible]
        [NotMapped]
        public int? DurationSeconds
        {
            get
            {
                if ( StartDateTime == null || StopDateTime == null )
                {
                    return null;
                }

                return ( int ) ( ( TimeSpan ) ( StopDateTime - StartDateTime ) ).TotalSeconds;
            }
        }
    }
}
