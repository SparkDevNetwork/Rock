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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityTypePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypePicker" /> class.
        /// </summary>
        public EntityTypePicker()
            : base()
        {
            Label = "Entity Type";
            EnhanceForLongLists = true;
        }

        /// <summary>
        /// Gets or sets the EntityTypes.
        /// </summary>
        /// <value>
        /// The EntityTypes.
        /// </value>
        public List<EntityType> EntityTypes
        {
            set
            {
                this.Items.Clear();

                // always add an empty option regardless of IsRequired
                this.Items.Add( new ListItem() );

                if ( IncludeGlobalOption )
                {
                    this.Items.Add( new ListItem( "None (Global Attributes)", "0" ) );
                }

                var entities = value.OrderBy( e => e.FriendlyName ).ToList();

                foreach ( var entity in entities.Where( t => t.IsCommon ) )
                {
                    var li = new System.Web.UI.WebControls.ListItem( entity.FriendlyName, entity.Id.ToString() );
                    li.Attributes.Add( "optiongroup", "Common" );
                    this.Items.Add( li );
                }

                foreach ( var entity in entities )
                {
                    var li = new System.Web.UI.WebControls.ListItem( entity.FriendlyName, entity.Id.ToString() );
                    li.Attributes.Add( "optiongroup", "All Entities" );
                    this.Items.Add( li );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include option for Global entity (0)
        /// </summary>
        /// <value>
        /// <c>true</c> if [include global option]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeGlobalOption
        {
            get
            {
                return ViewState["IncludeGlobalOption"] as bool? ?? false;
            }
            set
            {
                ViewState["IncludeGlobalOption"] = value;
                var item = this.Items.FindByValue( "0" );
                if ( value && item == null )
                {
                    int i = 0;
                    while ( this.Items.Count > i )
                    {
                        if ( this.Items[i].Value.AsInteger() > 0 )
                        {
                            break;
                        }
                        i++;
                    }
                    this.Items.Insert( i, new ListItem( "None (Global Attributes)", "0" ) );
                }
                if ( !value && item != null )
                {
                    this.Items.Remove( item );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected entity type identifier.
        /// </summary>
        /// <value>
        /// The selected entity type identifier.
        /// </value>
        public int? SelectedEntityTypeId
        {
            get
            {
                if ( this.IncludeGlobalOption )
                {
                    return this.SelectedValueAsInt( false );
                }
                else
                {
                    return this.SelectedValueAsInt( true );
                }
            }
            set
            {
                string id = value.HasValue ? value.Value.ToString() : "";

                // Look for the first item in list that matches value and select it.  
                // Unselect all the others.  Because common entities are included in 
                // list multiple times, the first occurrence should be selected
                bool noneSelected = true;
                foreach ( ListItem li in this.Items )
                {
                    if ( li.Value == id && noneSelected )
                    {
                        li.Selected = true;
                        noneSelected = false;
                    }
                    else
                    {
                        li.Selected = false;
                    }
                }
            }
        }

    }
}