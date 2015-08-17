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
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ReportService
    {
        /// <summary>
        /// Create a new non-persisted Report using an existing Report as a template. 
        /// </summary>
        /// <param name="reportId">The identifier of a Report to use as a template for the new Report.</param>
        /// <returns></returns>
        public Report GetNewFromTemplate( int reportId )
        {
            var existingReport = this.Queryable()
                                     .AsNoTracking()
                                     .Include( x => x.ReportFields )
                                     .FirstOrDefault( x => x.Id == reportId );

            if ( existingReport == null )
            {
                throw new Exception( string.Format( "GetNewFromTemplate method failed. Template Report ID \"{0}\" could not be found.", reportId ) );
            }

            // Deep-clone the Report and reset the properties that connect it to the permanent store.
            var newReport = (Report)( existingReport.Clone( true ) );

            newReport.Id = 0;
            newReport.Guid = Guid.NewGuid();
            newReport.ForeignId = null;

            // Reset the Report Field properties.
            foreach ( var field in newReport.ReportFields )
            {
                field.Id = 0;
                field.Guid = Guid.NewGuid();
                field.ForeignId = null;

                field.ReportId = 0;
            }

            return newReport;
        }
    }
}