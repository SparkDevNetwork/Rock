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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    public partial class AssessmentService
    {
        /// <summary>
        /// Updates the last reminder date of all pending assessments for the provided Person Alias ID.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        public void UpdateLastReminderDateForPersonAlias( int personAliasId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@lastReminderDate", DateTime.Now );
            sqlParams.Add( "@status", AssessmentRequestStatus.Pending );
            sqlParams.Add( "@personAliasId", personAliasId );

            string sql = $@"
                UPDATE [dbo].[Assessment]
                SET [LastReminderDate] = @lastReminderDate
                WHERE [Status] = @status
                    AND [PersonAliasId] = @personAliasId";

            DbService.ExecuteCommand( sql, System.Data.CommandType.Text, sqlParams, null );
        }
    }
}
