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
using Rock.Utility;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a component
    /// </summary>
    public class ComponentPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPicker"/> class.
        /// </summary>
        public ComponentPicker()
        {
            // Default constructor keeps IncludeInactive as false.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPicker"/> class.
        /// </summary>
        /// <param name="includeInactive">if set to <c>true</c> includes inactive items.</param>
        public ComponentPicker( bool includeInactive )
        {
            IncludeInactive = includeInactive;
        }

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
        /// Gets or sets a value indicating whether inactive items are included. (defaults to False).
        /// </summary>
        /// <value>
        ///     <c>true</c> if inactive items should be included; otherwise, <c>false</c>.
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

            var resolvedContainerType = Container.ResolveContainer( ContainerType );

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
        /// Binds the active or active and inactive items to the picker based on the IncludeInactive property.
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
                var isActive = component.Value.Value.IsActive;
                if ( isActive || ( !isActive && IncludeInactive ) )
                {
                    AddComponentListItem( component, isActive );
                }
            }
        }

        /// <summary>
        /// Adds a component to the <see cref="Items"/> collection, displaying its name
        /// and marking it as " (inactive)" if applicable.
        /// </summary>
        /// <param name="component">
        /// A <see cref="KeyValuePair{TKey, TValue}"/> where the key is the component's ID, 
        /// and the value is another key-value pair with a string key (typically the component's name) 
        /// and a <see cref="Component"/> instance.
        /// </param>
        /// <param name="isActive">
        /// A <see cref="bool"/> indicating whether the component is active. If false, 
        /// " (inactive)" is appended to the display name.
        /// </param>
        /// <remarks>
        /// The component's display name is derived from its <see cref="EntityType"/> metadata if available,
        /// otherwise it falls back to the internal name key, formatted via <c>SplitCase()</c> if needed.
        /// </remarks>
        private void AddComponentListItem( KeyValuePair<int, KeyValuePair<string, Component>> component, bool isActive )
        {
            var entityType = EntityTypeCache.Get( component.Value.Value.GetType() );
            if ( entityType == null )
            {
                return;
            }

            var componentEntityType = EntityTypeCache.Get( entityType.Guid );
            var componentName = Rock.Reflection.GetDisplayName( componentEntityType.GetEntityType() );

            if ( string.IsNullOrWhiteSpace( componentName ) )
            {
                componentName = component.Value.Key;
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }
            }

            var itemText = isActive ? componentName : $"{componentName} (inactive)";
            Items.Add( new ListItem( itemText, entityType.Guid.ToString().ToUpper() ) );
        }
    }
}