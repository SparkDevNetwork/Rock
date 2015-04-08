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
    public class BinaryFileTypePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileTypePicker" /> class.
        /// </summary>
        public BinaryFileTypePicker()
        {
            this.Items.Clear();
            this.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                foreach ( var item in new BinaryFileTypeService( rockContext )
                    .Queryable()
                    .OrderBy( f => f.Name )
                    .Select( a => new { a.Id, a.Name } )
                    .ToList() )
                {
                    this.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
                }
            }
        }
    }
}