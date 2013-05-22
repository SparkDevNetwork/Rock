//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    public class ComponentsPicker : LabeledCheckBoxList
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