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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a system phone number.
    /// </summary>
    public class SystemPhoneNumberPicker : RockDropDownList
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive SystemPhoneNumbers
        /// should be included (defaults to false).
        /// </summary> 
        /// <value>
        ///   <c>true</c> if inactive items should be included; otherwise, <c>false</c>.
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
                LoadItems();
            }
        }

        /// <summary>
        /// Gets or sets the selected SystemPhoneNumber identifier.
        /// </summary>
        /// <value>
        /// The selected SystemPhoneNumber identifier.
        /// </value>
        public int? SelectedSystemPhoneNumberId
        {
            get
            {
                EnsureChildControls();

                return SelectedItem?.Value?.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                LoadItems();

                // ensure that only that the only selected item are set.
                Items.OfType<ListItem>().ToList().ForEach( li => li.Selected = false );

                if ( value.HasValue )
                {
                    var item = Items.FindByValue( value.ToString() );
                    if ( item != null )
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        // If the value is not in the list, it could be that
                        // it is an Inactive item that wasn't added. If so,
                        // add it to the list.
                        var selectedSystemPhoneNumber = SystemPhoneNumberCache.Get( value.Value );
                        if ( selectedSystemPhoneNumber != null )
                        {
                            Items.Add( new ListItem {
                                Text = selectedSystemPhoneNumber.Name,
                                Value = selectedSystemPhoneNumber.Id.ToString(),
                                Selected = true
                            } );
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
            get => base.SelectedValue;
            set
            {
                SelectedSystemPhoneNumberId = value.AsIntegerOrNull();
                base.SelectedValue = value;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadItems();
            }
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        private void LoadItems()
        {
            var selectedItemId = SelectedSystemPhoneNumberId;
            var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;
            var systemPhoneNumbers = SystemPhoneNumberCache.All()
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();

            Items.Clear();

            // Add Empty option first
            Items.Add( new ListItem() );

            foreach ( var systemPhoneNumber in systemPhoneNumbers )
            {
                // Only skip items if they are not currently selected.
                if ( !selectedItemId.HasValue || selectedItemId.Value != systemPhoneNumber.Id )
                {
                    if ( !IncludeInactive && !systemPhoneNumber.IsActive )
                    {
                        continue;
                    }

                    if ( !systemPhoneNumber.IsAuthorized( Security.Authorization.VIEW, currentPerson ) )
                    {
                        continue;
                    }
                }

                var li = new ListItem( systemPhoneNumber.Name, systemPhoneNumber.Id.ToString() );

                if ( selectedItemId == systemPhoneNumber.Id )
                {
                    li.Selected = true;
                }

                Items.Add( li );
            }
        }
    }
}
