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
using System.Linq.Dynamic;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DefinedValuesPickerEnhanced : RockListBox, IDefinedValuePicker
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
                DefinedValuePicker.LoadDropDownItems( this, false );
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
                DefinedValuePicker.LoadDropDownItems( this, false );

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
                DefinedValuePicker.LoadDropDownItems( this, false );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// Overriding this to remove the required field validator that the parent class RockListBox does by default. RFV can only validate controls with user input and not the composite control.
        /// RFV gets created on the list box.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            string idOfControlToRemove = this.ID + "_rfv";
            var control = Controls.Cast<System.Web.UI.Control>().Where( c => c.ID == idOfControlToRemove ).FirstOrDefault();
            Controls.Remove( control );
        }
    }
}