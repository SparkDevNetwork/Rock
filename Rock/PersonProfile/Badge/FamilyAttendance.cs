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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Model;
using Rock.Data;
using System.Collections.Generic;
using System.Data;

namespace Rock.PersonProfile.Badge
{
    // TODO: Update to return actual data

    /// <summary>
    /// FamilyAttendance Badge
    /// </summary>
    [Description( "Family Attendance Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Attendance" )]
    public class FamilyAttendance : BadgeComponent
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override System.Collections.Generic.Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = base.AttributeValueDefaults;
                defaults["Order"] = "7";
                return defaults;
            }
        }

        public override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("Hi");
            Service service = new Service();

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PersonId", Person.Id);

            var results = service.GetDataSet("spCheckin_BadgeAttendance", System.Data.CommandType.StoredProcedure, parameters);

            if (results.Tables.Count > 0)
            {
                foreach (DataRow row in results.Tables[0].Rows)
                {
                    //writer.Write("<br> - " + row["AttendanceCount"]);
                }
            }
        }


    }
}
