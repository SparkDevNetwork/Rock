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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DefinedValuePicker : RockDropDownList, IDefinedValuePicker
    {
        /// <summary>
        /// Gets or sets the defined type identifier ( Required )
        /// </summary>
        /// <value>
        /// The defined type identifier.
        /// </value>
        public int? DefinedTypeId
        {
            get
            {
                return _definedTypeId;
            }

            set
            {
                _definedTypeId = value;
                DefinedValuePicker.LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// The _defined type identifier
        /// </summary>
        private int? _definedTypeId;

        /// <summary>
        /// Defined value descriptions will be displayed instead of the values (defaults to false)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display descriptions]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayDescriptions { get; set; }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        internal static void LoadDropDownItems( IDefinedValuePicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            picker.Items.Clear();

            if ( picker.DefinedTypeId.HasValue )
            {
                if ( includeEmptyOption )
                {
                    // add Empty option first
                    picker.Items.Add( new ListItem() );
                }

                var dt = DefinedTypeCache.Read( picker.DefinedTypeId.Value );
                if ( dt.DefinedValues.Any() )
                {
                    foreach ( var definedValue in dt.DefinedValues.OrderBy( v => v.Order ).ThenBy( v => v.Value ) )
                    {
                        var li = new ListItem( picker.DisplayDescriptions ? definedValue.Description : definedValue.Value, definedValue.Id.ToString() );
                        li.Selected = selectedItems.Contains( definedValue.Id );
                        picker.Items.Add( li );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Interface used by defined value pickers
    /// </summary>
    public interface IDefinedValuePicker 
    {
        /// <summary>
        /// Gets or sets the defined type identifier.
        /// </summary>
        /// <value>
        /// The defined type identifier.
        /// </value>
        int? DefinedTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display descriptions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display descriptions]; otherwise, <c>false</c>.
        /// </value>
        bool DisplayDescriptions { get; set; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        ListItemCollection Items { get; }
    }
}