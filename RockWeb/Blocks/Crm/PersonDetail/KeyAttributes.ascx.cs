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
using System.Web.UI.HtmlControls;

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
        #region Fields

        // View modes
        private readonly string VIEW_MODE_VIEW = "VIEW";
        private readonly string VIEW_MODE_EDIT = "EDIT";
        private readonly string VIEW_MODE_ORDER = "ORDER";
        private string _preferenceKey = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the view mode.
        /// </summary>
        /// <value>
        /// The view mode.
        /// </value>
        protected string ViewMode
        {
            get
            {
                var viewMode = ViewState["ViewMode"];
                if ( viewMode == null )
                {
                    return VIEW_MODE_VIEW;
                }
                else
                {
                    return viewMode.ToString();
                }
            }
            set
            {
                ViewState["ViewMode"] = value;
            }
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
            
            _preferenceKey = "Rock.KeyAttributes." + this.BlockId.ToString();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upKeyAttributes );

            dlgKeyAttribute.SaveClick += dlgKeyAttribute_SaveClick;

            string script = @"
        $('fieldset.attribute-values').sortable({
            handle: '.fa-bars',
            update: function(event, ui) {
                var newOrder = '';
                $(this).children('div').each(function( index ) {
                    newOrder += $(this).attr('data-attribute-id') + '|';
                });
                $(this).siblings('input.js-attribute-values-order').val(newOrder);
            }
        });
";
            ScriptManager.RegisterStartupScript( lbOrder, lbOrder.GetType(), "attribute-value-order", script, true );
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
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            AttributeList = null;
            BindAttributes();
            CreateControls( true );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ViewMode = VIEW_MODE_EDIT;
            CreateControls( true );
        }

        /// <summary>
        /// Handles the Click event of the lbOrder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOrder_Click( object sender, EventArgs e )
        {
            ViewMode = VIEW_MODE_ORDER;
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
                if ( ViewMode == VIEW_MODE_EDIT )
                {
                    int personEntityTypeId = EntityTypeCache.Read( typeof( Person ) ).Id;

                    var rockContext = new RockContext();
                    rockContext.WrapTransaction( () =>
                    {
                        var changes = new List<string>();

                        foreach ( int attributeId in AttributeList )
                        {
                            var attribute = AttributeCache.Read( attributeId );

                            if ( Person != null && 
                                ViewMode == VIEW_MODE_EDIT && 
                                attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                Control attributeControl = fsAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                                if ( attributeControl != null )
                                {
                                    string originalValue = Person.GetAttributeValue( attribute.Key );
                                    string newValue = attribute.FieldType.Field.GetEditValue( attributeControl, attribute.QualifierValues );
                                    Rock.Attribute.Helper.SaveAttributeValue( Person, attribute, newValue, rockContext );

                                    // Check for changes to write to history
                                    if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                                    {
                                        string formattedOriginalValue = string.Empty;
                                        if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                        {
                                            formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                        }

                                        string formattedNewValue = string.Empty;
                                        if ( !string.IsNullOrWhiteSpace( newValue ) )
                                        {
                                            formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                        }

                                        History.EvaluateChange( changes, attribute.Name, formattedOriginalValue, formattedNewValue );
                                    }
                                }
                            }
                        }
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                Person.Id, changes );
                        }
                    } );
                }
                else if ( ViewMode == VIEW_MODE_ORDER )
                {
                    // Split and deliminate again to remove trailing delimiter
                    var attributeOrder = hfAttributeOrder.Value.SplitDelimitedValues().ToList().AsDelimited( "," );
                    SetUserPreference( _preferenceKey, attributeOrder );

                    BindAttributes();
                }

                ViewMode = VIEW_MODE_VIEW;
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
            ViewMode = VIEW_MODE_VIEW;
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
            // Re: bug #721
            // I can't see why this next line was in there because by the time we get
            // here the SelectedAttributes list has all the right items. So I changed
            // the SetUserPreference line to just use the SelectedAttributes instead of AttributeList.
            //SelectedAttributes.Where( a => !AttributeList.Contains( a ) ).ToList().ForEach( a => AttributeList.Add( a ) );
            SetUserPreference( _preferenceKey, SelectedAttributes.AsDelimited( "," ) );
            BindAttributes();
            CreateControls( true );

            HideDialog();
        }

        #endregion

        #region Methods

        private void BindAttributes()
        {
            AttributeList = new List<int>();

            var attributes = new List<NameValue>();

            foreach ( string keyAttributeId in GetUserPreference( _preferenceKey ).SplitDelimitedValues() )
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

            hfAttributeOrder.Value = AttributeList.AsDelimited( "|" );

            if ( Person != null )
            {
                foreach ( int attributeId in AttributeList )
                {
                    var attribute = AttributeCache.Read( attributeId );
                    string attributeValue = Person.GetAttributeValue( attribute.Key );
                    string formattedValue = string.Empty;

                    if ( ViewMode != VIEW_MODE_EDIT || !attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        if ( ViewMode == VIEW_MODE_ORDER  )
                        {
                            var div = new HtmlGenericControl( "div" );
                            fsAttributes.Controls.Add( div );
                            div.Attributes.Add( "data-attribute-id", attribute.Id.ToString() );
                            div.Attributes.Add( "class", "form-group" );

                            var a = new HtmlGenericControl( "a" );
                            div.Controls.Add( a );

                            var i = new HtmlGenericControl( "i" );
                            a.Controls.Add( i );
                            i.Attributes.Add( "class", "fa fa-bars" );

                            div.Controls.Add( new LiteralControl( " " + attribute.Name ) );
                        }
                        else
                        {                            
                            if ( attribute.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName )
                            {
                                formattedValue = attribute.FieldType.Field.FormatValueAsHtml( fsAttributes, attributeValue, attribute.QualifierValues, true );
                            }
                            else
                            {
                                formattedValue = attribute.FieldType.Field.FormatValueAsHtml( fsAttributes, attributeValue, attribute.QualifierValues, false );
                            }
                            
                            if ( !string.IsNullOrWhiteSpace( formattedValue ) )
                            {
                                fsAttributes.Controls.Add( new RockLiteral { Label = attribute.Name, Text = formattedValue } );
                            }
                        }
                    }
                    else
                    {
                        attribute.AddControl( fsAttributes.Controls, attributeValue, string.Empty, setValues, true );
                    }
                }
            }

            pnlActions.Visible = ( ViewMode != VIEW_MODE_VIEW );
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