// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Reflection;
using System.Web.UI.WebControls;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
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

                this.Items.Clear();
                this.Items.Add( new ListItem() );

                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    Type containerType = Type.GetType( value );
                    if ( containerType != null )
                    {
                        PropertyInfo instanceProperty = containerType.GetProperty( "Instance" );
                        if ( instanceProperty != null )
                        {
                            IContainer container = instanceProperty.GetValue( null, null ) as IContainer;
                            if ( container != null )
                            {
                                foreach ( var component in container.Dictionary )
                                {
                                    if ( component.Value.Value.IsActive )
                                    {
                                        var entityType = EntityTypeCache.Read( component.Value.Value.GetType() );
                                        if ( entityType != null )
                                        {
                                            this.Items.Add( new ListItem( component.Value.Key.SplitCase(), entityType.Guid.ToString().ToUpper() ) );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
                Guid? gatewayGuid = this.SelectedValueAsGuid();
                if ( gatewayGuid.HasValue )
                {
                    var gatewayEntity = EntityTypeCache.Read( gatewayGuid.Value );
                    if ( gatewayEntity != null )
                    {
                        return gatewayEntity.Id;
                    }
                }

                return null;
            }
        }

    }
}