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

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select multiple system phone numbers.
    /// </summary>
    public class SystemPhoneNumbersPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemPhoneNumbersPicker"/> class.
        /// </summary>
        public SystemPhoneNumbersPicker()
            : base()
        {
            RepeatDirection = RepeatDirection.Horizontal;
            LoadItems();
        }

        /// <summary>
        /// Gets or sets the selected System Phone Number Identifiers.
        /// </summary>
        /// <value>
        /// The selected System Phone Number identifiers.
        /// </value>
        public int[] SelectedSystemPhoneNumberIds
        {
            get
            {
                EnsureChildControls();
                return Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value ).AsIntegerList().ToArray();
            }
            set
            {
                EnsureChildControls();
                LoadItems();

                // ensure that only that the only selected items are set.
                foreach ( var item in Items.OfType<ListItem>() )
                {
                    item.Selected = false;
                }

                foreach ( int selectedValue in value )
                {
                    var item = Items.FindByValue( selectedValue.ToString() );
                    if ( item != null )
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        // if the selectedValue is not in the list, it could be
                        // that it is an Inactive item that wasn't added. If so,
                        // add it to the list;
                        var systemPhoneNumber = SystemPhoneNumberCache.Get( selectedValue );
                        if ( systemPhoneNumber != null )
                        {
                            Items.Add( new ListItem( systemPhoneNumber.Name, systemPhoneNumber.Id.ToString() ) { Selected = true } );
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
                SelectedSystemPhoneNumberIds = value?.SplitDelimitedValues().AsIntegerList().ToArray() ?? new int[0];
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
                LoadItems();
            }
        }

        /// <summary>
        /// Loads the items into the checkbox list.
        /// </summary>
        private void LoadItems()
        {
            var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;
            var systemPhoneNumbers = SystemPhoneNumberCache.All()
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();

            Items.Clear();

            foreach ( var systemPhoneNumber in systemPhoneNumbers )
            {
                // Only skip items if they are not currently selected.
                if ( !SelectedSystemPhoneNumberIds.Contains( systemPhoneNumber.Id ) )
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

                Items.Add( new ListItem( systemPhoneNumber.Name, systemPhoneNumber.Id.ToString() ) );
            }
        }
    }
}