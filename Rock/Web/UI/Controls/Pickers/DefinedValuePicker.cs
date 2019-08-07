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
        /// Gets or sets a value indicating whether inactive DefinedValues should be included (defaults to False)
        /// </summary> 
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return ViewState["IncludeInactive"] as bool? ?? false;
            }

            set
            {
                ViewState["IncludeInactive"] = value;
                DefinedValuePicker.LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// Gets or sets the selected defined value identifier.
        /// </summary>
        /// <value>
        /// The selected defined value identifier.
        /// </value>
        public int? SelectedDefinedValueId
        {
            get
            {
                return this.SelectedDefinedValuesId.FirstOrDefault();
            }

            set
            {
                if ( value != null )
                {
                    this.SelectedDefinedValuesId = new int[] { value.Value };
                }
                else
                {
                    this.SelectedDefinedValuesId = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected defined value Ids
        /// </summary>
        /// <value>
        /// The selected defined values identifier.
        /// </value>
        public int[] SelectedDefinedValuesId
        {
            get
            {
                EnsureChildControls();
                return this.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value ).AsIntegerList().ToArray();
            }

            set
            {
                EnsureChildControls();
                LoadDropDownItems( this, true );

                // ensure that only that the only selected items are set.
                foreach ( var item in this.Items.OfType<ListItem>() )
                {
                    item.Selected = false;
                }

                foreach ( int selectedValue in value )
                {
                    var item = this.Items.FindByValue( selectedValue.ToString() );
                    if ( item != null )
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        // if the selectedValue is not in the list, it could be that it is an Inactive item that wasn't added. If so, add it to the list;
                        var selectedDefinedValue = DefinedValueCache.Get( selectedValue );
                        if ( selectedDefinedValue != null )
                        {
                            this.Items.Add( new ListItem( this.DisplayDescriptions ? selectedDefinedValue.Description : selectedDefinedValue.Value, selectedDefinedValue.Id.ToString() ) { Selected = true } );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of the selected item in the list control, or selects the item in the list control that contains the specified value.
        /// </summary>
        public override string SelectedValue
        {
            get
            {
                return base.SelectedValue;
            }

            set
            {
                this.SelectedDefinedValuesId = value?.SplitDelimitedValues().AsIntegerList().ToArray() ?? new int[0];
                base.SelectedValue = value;
            }
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        internal static void LoadDropDownItems( IDefinedValuePicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.SelectedDefinedValuesId;

            picker.Items.Clear();

            if ( picker.DefinedTypeId.HasValue )
            {
                if ( includeEmptyOption )
                {
                    // add Empty option first
                    picker.Items.Add( new ListItem() );
                }

                var dt = DefinedTypeCache.Get( picker.DefinedTypeId.Value );
                var definedValuesList = dt?.DefinedValues
                    .Where( a => a.IsActive || picker.IncludeInactive || selectedItems.Contains( a.Id ) )
                    .OrderBy( v => v.Order ).ThenBy( v => v.Value ).ToList();

                if ( definedValuesList != null && definedValuesList.Any() )
                {
                    foreach ( var definedValue in definedValuesList )
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

        /// <summary>
        /// Gets or sets a value indicating whether inactive DefinedValues should be included
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets the selected defined values identifier.
        /// </summary>
        /// <value>
        /// The selected defined values identifier.
        /// </value>
        int[] SelectedDefinedValuesId { get; set;  }
    }
}