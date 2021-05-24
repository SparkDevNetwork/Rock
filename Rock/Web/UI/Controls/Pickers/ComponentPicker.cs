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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;

using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a component
    /// </summary>
    public class ComponentPicker : RockDropDownList
    {
        /// <summary>
        /// Gets or sets the type of the container.
        /// </summary>
        /// <value>
        /// The type of the container.
        /// </value>
        public string ContainerType
        {
            get
            {
                return ViewState["ContainerType"] as string;
            }

            set
            {
                ViewState["ContainerType"] = value;
                BindItems();
            }
        }

        /// <summary>
        /// Gets the selected entity type identifier.
        /// </summary>
        /// <value>
        /// The selected entity type identifier.
        /// </value>
        public int? SelectedEntityTypeId
        {
            get
            {
                Guid? componentEntityTypeGuid = this.SelectedValueAsGuid();
                if ( componentEntityTypeGuid.HasValue )
                {
                    var componentEntityType = EntityTypeCache.Get( componentEntityTypeGuid.Value );
                    if ( componentEntityType != null )
                    {
                        return componentEntityType.Id;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the component dictionary from the container.
        /// </summary>
        /// <returns></returns>
        protected Dictionary<int, KeyValuePair<string, Component>> GetComponentDictionary()
        {
            if ( ContainerType.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var resolvedContainerType = Type.GetType( ContainerType );

            if ( resolvedContainerType == null )
            {
                return null;
            }

            var instanceProperty = resolvedContainerType.GetProperty( "Instance" );

            if ( instanceProperty == null )
            {
                return null;
            }

            var container = instanceProperty.GetValue( null, null ) as IContainer;
            return container?.Dictionary;
        }

        /// <summary>
        /// Binds the items to the drop down list.
        /// </summary>
        protected virtual void BindItems()
        {
            Items.Clear();
            Items.Add( new ListItem() );
            var componentDictionary = GetComponentDictionary();

            if ( componentDictionary == null )
            {
                return;
            }

            foreach ( var component in componentDictionary )
            {
                if ( component.Value.Value.IsActive )
                {
                    var entityType = EntityTypeCache.Get( component.Value.Value.GetType() );
                    if ( entityType != null )
                    {
                        var componentName = component.Value.Key;

                        // If the component name already has a space then trust
                        // that they are using the exact name formatting they want.
                        if ( !componentName.Contains( ' ' ) )
                        {
                            componentName = componentName.SplitCase();
                        }

                        Items.Add( new ListItem( componentName, entityType.Guid.ToString().ToUpper() ) );
                    }
                }
            }
        }
    }
}