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

using System.Web.UI.WebControls;
using Rock.Badge;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Badge Component Picker
    /// </summary>
    public class BadgeComponentPicker : ComponentPicker
    {
        /// <summary>
        /// Gets or sets the type of entity that the badges should apply to.
        /// </summary>
        public string AppliesToEntityType
        {
            get
            {
                return ViewState["AppliesToEntityType"] as string;
            }

            set
            {
                ViewState["AppliesToEntityType"] = value;
                BindItems();
            }
        }

        /// <summary>
        /// Binds the items to the drop down list.
        /// </summary>
        protected override void BindItems()
        {
            Items.Clear();
            Items.Add( new ListItem() );
            var componentDictionary = GetComponentDictionary();

            if ( componentDictionary == null )
            {
                return;
            }

            foreach ( var kvp in componentDictionary )
            {
                var badgeComponent = kvp.Value.Value as BadgeComponent;

                if ( badgeComponent == null || !badgeComponent.IsActive || !badgeComponent.DoesApplyToEntityType( AppliesToEntityType ) )
                {
                    continue;
                }

                var entityType = EntityTypeCache.Get( badgeComponent.GetType() );

                if ( entityType != null )
                {
                    Items.Add( new ListItem( kvp.Value.Key.SplitCase(), entityType.Guid.ToString().ToUpper() ) );
                }
            }
        }
    }
}