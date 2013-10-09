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

                if ( !Required )
                {
                    this.Items.Add( new ListItem( string.Empty, Rock.Constants.None.IdValue ) );
                }

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
                                            this.Items.Add( new ListItem( component.Value.Key, entityType.Guid.ToString() ) );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}