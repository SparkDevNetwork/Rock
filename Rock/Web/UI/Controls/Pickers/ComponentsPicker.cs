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
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections.Generic;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ComponentsPicker : RockCheckBoxList
    {
        /// <summary>
        /// Gets or sets the binary file type id.
        /// </summary>
        /// <value>
        /// The binary file type id.
        /// </value>
        public string ContainerType
        {
            set
            {
                this.Items.Clear();

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
                                            this.Items.Add( new ListItem( entityType.FriendlyName, entityType.Guid.ToString() ) );
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
        /// Gets the selected component ids.
        /// </summary>
        /// <value>
        /// The selected component ids.
        /// </value>
        public List<Guid> SelectedComponents
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => Guid.Parse( a.Value ) ).ToList();
            }
            set
            {
                foreach ( ListItem componentItem in this.Items )
                {
                    componentItem.Selected = value.Exists( a => a.Equals( Guid.Parse( componentItem.Value ) ) );
                }
            }
        }
    }
}