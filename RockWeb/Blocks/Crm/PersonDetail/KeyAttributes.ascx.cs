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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.Data;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// User control for viewing key attributes
    /// </summary>
    [DisplayName( "Person Key Attributes" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person key attributes (Person Detail Page)." )]
    public partial class KeyAttributes : Rock.Web.UI.PersonBlock
    {
        #region Properties

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
                if ( attributeList == null )
                {
                    attributeList = new List<int>();
                    ViewState["AttributeList"] = attributeList;
                }
                return attributeList;
            }
            set { ViewState["AttributeList"] = value; }
        }

        /// <summary>
        /// Gets or sets the attributes selected when configuring list
        /// </summary>
        /// <value>
        /// The selected attributes.
        /// </value>
        protected List<int> SelectedAttributes
        {
            get
            {
                List<int> selectedAttributes = ViewState["SelectedAttributes"] as List<int>;
                if (selectedAttributes == null)
                {
                    selectedAttributes = new List<int>();
                    SelectedAttributes = selectedAttributes;
                }
                return selectedAttributes;
            }
            set
            {
                ViewState["SelectedAttributes"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dlgKeyAttribute.SaveClick += dlgKeyAttribute_SaveClick;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Person != null && Person.Id != 0 )
            {
                upKeyAttributes.Visible = true;

                if ( !Page.IsPostBack )
                {
                    BindAttributes();
                }
                else
                {
                    // If dialog is active, save the current selection
                    if ( !string.IsNullOrWhiteSpace( hfActiveDialog.Value ) )
                    {
                        for ( int i = 0; i < cblAttributes.Items.Count; i++ )
                        {
                            ListItem li = cblAttributes.Items[i];
                            int attributeId = int.Parse( li.Value );

                            // Check form value for checkbox list since "selected" seems to be lost on postback for unselected items
                            string value = Request.Form[cblAttributes.UniqueID + "$" + i.ToString()];
                            if ( value != null )
                            {
                                li.Selected = true;
                                if ( !SelectedAttributes.Contains( attributeId ) )
                                {
                                    SelectedAttributes.Add( attributeId );
                                }
                            }
                            else
                            {
                                li.Selected = false;
                                SelectedAttributes.Remove( attributeId );
                            }
                        }
                    }

                    ShowDialog();
                }
            }
            else
            {
                upKeyAttributes.Visible = false;
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CreateControls( false );
        }
        
        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            EditMode = true;
            CreateControls( true );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                foreach ( int attributeId in AttributeList )
                {
                    var attribute = AttributeCache.Read( attributeId );

                    if ( Person != null && EditMode && attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        Control attributeControl = fsAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                        if ( attributeControl != null )
                        {
                            string value = attribute.FieldType.Field.GetEditValue( attributeControl, attribute.QualifierValues );
                            Rock.Attribute.Helper.SaveAttributeValue( Person, attribute, value, rockContext );
                        }
                    }
                }

                EditMode = false;
                CreateControls( false );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            EditMode = false;
            CreateControls( false );
        }

        /// <summary>
        /// Handles the Click event of the lbConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfigure_Click( object sender, EventArgs e )
        {
            SelectedAttributes = new List<int>();
            AttributeList.ForEach( a => SelectedAttributes.Add( a ) );

            ddlCategories.Items.Clear();

            int attributeEntityTypeId = EntityTypeCache.Read("Rock.Model.Attribute").Id;
            string personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id.ToString();

            foreach ( var category in new CategoryService( new RockContext() ).Queryable()
                .Where( c =>
                    c.EntityTypeId == attributeEntityTypeId &&
                    c.EntityTypeQualifierColumn == "EntityTypeId" &&
                    c.EntityTypeQualifierValue == personEntityTypeId ) )
            {
                if ( category.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    ListItem li = new ListItem( category.Name, category.Id.ToString() );
                    ddlCategories.Items.Add( li );
                }
            }

            BindAttributeSelection( ddlCategories.SelectedValueAsId() ?? 0 );

            ShowDialog( "KEYATTRIBUTES" );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCategories_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindAttributeSelection( ddlCategories.SelectedValueAsId() ?? 0 );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgKeyAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgKeyAttribute_SaveClick( object sender, EventArgs e )
        {
            SetUserPreference( "Rock.KeyAttributes", SelectedAttributes.AsDelimited( "," ) );
            BindAttributes();
            HideDialog();
        }

        #endregion

        #region Methods

        private void BindAttributes()
        {
            AttributeList.Clear();

            var attributes = new List<NameValue>();

            foreach ( string keyAttributeId in GetUserPreference( "Rock.KeyAttributes" ).SplitDelimitedValues() )
            {
                int attributeId = 0;
                if ( Int32.TryParse( keyAttributeId, out attributeId ) )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                    if ( attribute != null && attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AttributeList.Add( attribute.Id );
                    }
                }
            }

            CreateControls( true );
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

                    if ( !EditMode || !attribute.IsAuthorized("Edit", CurrentPerson))
                    {
                        formattedValue = attribute.FieldType.Field.FormatValue( fsAttributes, attributeValue, attribute.QualifierValues, false );
                        if ( !string.IsNullOrWhiteSpace( formattedValue ) )
                        {
                            fsAttributes.Controls.Add( new RockLiteral { Label = attribute.Name, Text = formattedValue } );
                        }
                    }
                    else
                    {
                        attribute.AddControl( fsAttributes.Controls, attributeValue, string.Empty, setValues, true );
                    }
                }
            }

            pnlActions.Visible = EditMode;
        }
        
        private void BindAttributeSelection( int categoryId )
        {
            cblAttributes.Items.Clear();

            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;

            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a => 
                    a.EntityTypeId == personEntityTypeId &&
                    a.Categories.Select( c=> c.Id).Contains(categoryId))
                .OrderBy( a => a.Order)
                .ThenBy( a => a.Name))
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    ListItem li = new ListItem( attribute.Name, attribute.Id.ToString() );
                    li.Selected = SelectedAttributes.Contains( attribute.Id );
                    cblAttributes.Items.Add( li );
                }
            }

        }

        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "KEYATTRIBUTES":
                    dlgKeyAttribute.Show();
                    break;
            }
        }

        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "KEYATTRIBUTES":
                    dlgKeyAttribute.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }



        #endregion

        #region Classes

        [Serializable]
        class NameValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public NameValue( string name, string value )
            {
                Name = name;
                Value = value;
            }
        }

        #endregion

}
}