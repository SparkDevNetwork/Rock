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
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class BinaryFilePicker : RockDropDownList
    {
        /// <summary>
        /// Gets or sets the binary file type id.
        /// </summary>
        /// <value>
        /// The binary file type id.
        /// </value>
        public int? BinaryFileTypeId
        {
            get 
            { 
                return ViewState["BinaryFileTypeId"] as int?; 
            }
            set
            {
                ViewState["BinaryFileTypeId"] = value;

                this.Items.Clear();
                this.DataTextField = "FileName";
                this.DataValueField = "Id";
                this.DataSource = new BinaryFileService( new RockContext() )
                    .Queryable()
                    .Where( f => f.BinaryFileTypeId == value && !f.IsTemporary )
                    .OrderBy( f => f.FileName )
                    .Select( f => new { f.FileName, f.Id } )
                    .ToList();
                this.DataBind();
            }
        }
    }
}