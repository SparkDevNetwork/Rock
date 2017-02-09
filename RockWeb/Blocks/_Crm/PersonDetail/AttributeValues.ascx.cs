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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// User control for editing the value(s) of a set of attributes for person
    /// </summary>
    [DisplayName( "Attribute Values" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows for editing the value(s) of a set of attributes for person." )]

    [AttributeCategoryField( "Category", "The Attribute Category to display attributes from", false, "Rock.Model.Person", true, "", "", 0 )]
    [TextField("Attribute Order", "The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.", false, "", "", 1)]
    public partial class AttributeValues : PersonBlock
    {

        #region Fields

        // View modes
        private readonly string VIEW_MODE_VIEW = "VIEW";
        private readonly string VIEW_MODE_EDIT = "EDIT";
        private readonly string VIEW_MODE_ORDER = "ORDER";

        private bool _canAdministrate = false;

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

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlAttributeValues );

            _canAdministrate = IsUserAuthorized(Rock.Security.Authorization.ADMINISTRATE );

            lbOrder.Visible = _canAdministrate;

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
                upnlAttributeValues.Visible = true;

                if ( !Page.IsPostBack )
                {
                    BindData();
                }
            }
            else
            {
                upnlAttributeValues.Visible = false;
            }
        }

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
            BindData();
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

                    var changes = new List<string>();
                    foreach ( int attributeId in AttributeList )
                    {
                        var attribute = AttributeCache.Read( attributeId );

                        if ( Person != null &&
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

                                    
                                    History.EvaluateChange( changes, attribute.Name, formattedOriginalValue, formattedNewValue, attribute.FieldType.Field.IsSensitive());
                                }
                            }
                        }
                    }

                    if ( changes.Any() )
                    {
                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                            Person.Id, changes );
                    }
                }
                else if ( ViewMode == VIEW_MODE_ORDER && _canAdministrate )
                {
                    // Split and deliminate again to remove trailing delimiter
                    var attributeOrder = hfAttributeOrder.Value.SplitDelimitedValues().ToList().AsDelimited( "|" );

                    SetAttributeValue( "AttributeOrder", attributeOrder );
                    SaveAttributeValues();

                    BindData();
                }

                ViewMode = VIEW_MODE_VIEW;
                CreateControls( false );
            }
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ViewMode = VIEW_MODE_VIEW;
            CreateControls( false );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bind the data based on the configured category setting.
        /// </summary>
        private void BindData()
        {
            AttributeList = new List<int>();

            string categoryGuid = GetAttributeValue( "Category" );
            Guid guid = Guid.Empty;
            if ( Guid.TryParse( categoryGuid, out guid ) )
            {
                var category = CategoryCache.Read( guid );
                if ( category != null )
                {
                    if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
                    {
                        lCategoryName.Text = string.Format( "<i class='{0}'></i> {1}", category.IconCssClass, category.Name );
                    }
                    else
                    {
                        lCategoryName.Text = category.Name;
                    }

                    var orderOverride = new List<int>();
                    GetAttributeValue( "AttributeOrder" ).SplitDelimitedValues().ToList().ForEach( a => orderOverride.Add( a.AsInteger() ) );

                    var orderedAttributeList = new AttributeService( new RockContext() ).GetByCategoryId( category.Id )
                        .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

                    foreach ( int attributeId in orderOverride )
                    {
                        var attribute = orderedAttributeList.FirstOrDefault( a => a.Id == attributeId );
                        if ( attribute != null && attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            AttributeList.Add( attribute.Id );
                        }
                    }

                    foreach ( var attribute in orderedAttributeList.Where( a => !orderOverride.Contains( a.Id ) ) )
                    {
                        if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            AttributeList.Add( attribute.Id );
                        }
                    }
                }
            }

            CreateControls( true );
        }

        private void CreateControls( bool setValues )
        {
            fsAttributes.Controls.Clear();

            string validationGroup = string.Format("vgAttributeValues_{0}", this.BlockId );
            valSummaryTop.ValidationGroup = validationGroup;
            btnSave.ValidationGroup = validationGroup;

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
                        if ( ViewMode == VIEW_MODE_ORDER && _canAdministrate )
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
                            if ( attribute.FieldType.Class == typeof(Rock.Field.Types.ImageFieldType).FullName )
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
                        attribute.AddControl( fsAttributes.Controls, attributeValue, validationGroup, setValues, true );
                    }
                }
            }

            pnlActions.Visible = ( ViewMode != VIEW_MODE_VIEW );
        }

        #endregion

    }
}
