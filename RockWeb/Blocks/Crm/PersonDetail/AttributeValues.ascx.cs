//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// User control for editing the value(s) of a set of attributes for person
    /// </summary>
    [AttributeCategoryField( "Category", "The Attribute Category to display attributes from", false, "Rock.Model.Person" )]
    public partial class AttributeValues : PersonBlock
    {

        /// <summary>
        /// Gets or sets a value indicating whether [edit mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [edit mode]; otherwise, <c>false</c>.
        /// </value>
        protected bool EditMode
        {
            get { return ViewState["EditMode"] as bool? ?? false; }
            set { ViewState["EditMode"] = value; }
        }

        /// <summary>
        /// Gets or sets the attribute list.
        /// </summary>
        /// <value>
        /// The attribute list.
        /// </value>
        protected List<int> AttributeList
        {
            get 
            { 
                List<int> attributeList = ViewState["AttributeList"] as List<int>;
                if (attributeList == null)
                {
                    attributeList = new List<int>();
                    ViewState["AttributeList"] = attributeList;
                }
                return attributeList;
            }
            set { ViewState["AttributeList"] = value; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
 	        base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                string categoryGuid = GetAttributeValue( "Category" );
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Read( guid );
                    if ( category != null )
                    {
                        lCategoryName.Text = category.Name;

                        foreach ( var attribute in new AttributeService().GetByCategoryId( category.Id ) )
                        {
                            if ( attribute.IsAuthorized( "Edit", CurrentPerson ) )
                            {
                                AttributeList.Add( attribute.Id );
                            }
                        }

                    }
                }

                CreateControls( true );
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CreateControls( false );
        }

        private void CreateControls( bool setValues )
        {
            fsAttributes.Controls.Clear();

            if ( Person != null )
            {
                foreach ( int attributeId in AttributeList )
                {
                    var attribute = AttributeCache.Read( attributeId );
                    string attributeValue = Person.GetAttributeValue( attribute.Key );
                    string formattedValue = string.Empty;

                    if ( !EditMode )
                    {
                        formattedValue = attribute.FieldType.Field.FormatValue( fsAttributes, attributeValue, attribute.QualifierValues, false );
                        if ( !string.IsNullOrWhiteSpace( formattedValue ) )
                        {
                            fsAttributes.Controls.Add( new RockLiteral { Label = attribute.Name, Text = formattedValue } );
                        }
                    }
                    else
                    {
                        attribute.AddControl( fsAttributes.Controls, attributeValue, setValues, true );
                    }
                }
            }

            pnlActions.Visible = EditMode;
        }

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            EditMode = true;
            CreateControls( true );
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                foreach ( int attributeId in AttributeList )
                {
                    var attribute = AttributeCache.Read( attributeId );

                    if ( Person != null && EditMode )
                    {
                        Control attributeControl = fsAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                        if ( attributeControl != null )
                        {
                            string value = attribute.FieldType.Field.GetEditValue( attributeControl, attribute.QualifierValues );
                            Rock.Attribute.Helper.SaveAttributeValue( Person, attribute, value, CurrentPersonId );
                        }
                    }
                }

                EditMode = false;
                CreateControls( false );
            }
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            EditMode = false;
            CreateControls( false );
        }
    }
}
