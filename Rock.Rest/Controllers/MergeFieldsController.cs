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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Rest.v2;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [Rock.SystemGuid.RestControllerGuid( "D2F7EDA7-CCA3-4F69-B8C5-EB2702EBC317")]
    public partial class MergeFieldsController : ApiControllerBase 
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/MergeFields/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "5827B6FD-4F4D-4642-8D04-7F7A65470888" )]
        public virtual string Get( string id )
        {
            return Rock.Web.UI.Controls.MergeFieldPicker.FormatSelectedValue( id );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="additionalFields">The additional fields.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/MergeFields/GetChildren/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "40A9FF74-3E5E-4C4A-8F11-2BBF8ACB1E51" )]
        public IQueryable<TreeViewItem> GetChildren( string id, string additionalFields )
        {
            var person = GetPerson();

            // The logic that was here was moved to the v2 API because calling GetPerson here when that new API used this method
            // caused errors due to this controller not have the Request Context 
            return ControlsController.MergeFieldPickerGetChildren( id, additionalFields, person );
        }
    }
}