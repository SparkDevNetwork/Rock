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
using System.Web.Http;

using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class NotesController
    {
        /// <summary>
        /// Returns Note depending on if the current person has rights to edit that note
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <returns></returns>
        [Authenticate]
        // NOTE: Intentionally don't use the [Secured] attribute on this Controller method since security depends on which Note they are attempting to Edit
        [HttpGet]
        [System.Web.Http.Route( "api/Notes/GetNoteEditData" )]
        public Rock.Model.Note GetNoteEditData( int noteId )
        {
            // Enable proxy creation since Note security may need to lazy load some child properties (for example, note.CreatedByPersonAlias)
            SetProxyCreation( true );

            var noteData = this.Get( noteId );
            
            // So, we'll manually check security here instead
            CheckCanEdit( noteData );
            return noteData;
        }
    }
}
