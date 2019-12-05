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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a Assessment type
    /// </summary>
    public class AssessmentTypePicker : RockDropDownList, IAssessmentTypePicker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssessmentTypePicker"/> class.
        /// </summary>
        public AssessmentTypePicker() : base()
        {
            if ( Items.Count == 0 )
            {
                LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        internal static void LoadDropDownItems( IAssessmentTypePicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            picker.Items.Clear();

            if ( includeEmptyOption )
            {
                // add Empty option first
                picker.Items.Add( new ListItem() );
            }

            var rockContext = new RockContext();
            var assessmentTypeService = new AssessmentTypeService( rockContext );
            var assessmentTypes = assessmentTypeService.Queryable().AsNoTracking()
                .Where( at => at.IsActive )
                .OrderBy( at => at.Title )
                .ThenBy( at => at.Id )
                .ToList();

            foreach ( var assessmentType in assessmentTypes )
            {
                var li = new ListItem( assessmentType.Title, assessmentType.Id.ToString() );
                li.Selected = selectedItems.Contains( assessmentType.Id );
                picker.Items.Add( li );
            }
        }
    }

    /// <summary>
    /// Interface used by defined value pickers
    /// </summary>
    public interface IAssessmentTypePicker
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        ListItemCollection Items { get; }
    }
}