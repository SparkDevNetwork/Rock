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
    /// Control that can be used to select a step type from a particular pre-configured step program.
    /// </summary>
    public class StepTypePicker : RockDropDownList, IStepTypePicker
    {
        /// <summary>
        /// Gets or sets the step program identifier ( Required )
        /// </summary>
        public int? StepProgramId
        {
            get
            {
                return _stepProgramId;
            }

            set
            {
                _stepProgramId = value;
                StepTypePicker.LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// The step program identifier
        /// </summary>
        private int? _stepProgramId;

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        internal static void LoadDropDownItems( IStepTypePicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            picker.Items.Clear();

            if ( !picker.StepProgramId.HasValue )
            {
                return;
            }

            if ( includeEmptyOption )
            {
                // add Empty option first
                picker.Items.Add( new ListItem() );
            }

            var stepTypeService = new StepTypeService( new RockContext() );
            var stepTypes = stepTypeService.Queryable().AsNoTracking()
                .Where( st =>
                    st.StepProgramId == picker.StepProgramId.Value &&
                    st.IsActive )
                .OrderBy( st => st.Order )
                .ThenBy( st => st.Name )
                .ToList();

            foreach ( var stepType in stepTypes )
            {
                var li = new ListItem( stepType.Name, stepType.Id.ToString() );
                li.Selected = selectedItems.Contains( stepType.Id );
                picker.Items.Add( li );
            }            
        }
    }

    /// <summary>
    /// Interface used by defined value pickers
    /// </summary>
    public interface IStepTypePicker
    {
        /// <summary>
        /// Gets or sets the step program identifier.
        /// </summary>
        int? StepProgramId { get; set; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        ListItemCollection Items { get; }
    }
}