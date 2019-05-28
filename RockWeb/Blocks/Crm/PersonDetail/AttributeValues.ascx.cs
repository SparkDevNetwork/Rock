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

    [AttributeCategoryField( "Category",
        allowMultiple: true,
        Key = AttributeKeys.Category,
        AllowMultiple = true,
        Description = "The Attribute Categories to display attributes from",
        EntityTypeName = "Rock.Model.Person",
        IsRequired = true,
        Order = 0 )]

    [TextField("Attribute Order",
        Key = AttributeKeys.AttributeOrder,
        Description = "The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.",
        IsRequired = false,
        Order = 1)]

    [BooleanField("Use Abbreviated Name",
        Key = AttributeKeys.UseAbbreviatedName,
        Description = "Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 2
        )]

    [TextField( "Set Page Title",
        Key = AttributeKeys.SetPageTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "",
        Order = 3 )]

    [TextField( "Set Page Icon",
        Key = AttributeKeys.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "",
        Order = 4 )]

    [BooleanField( "Show Category Names as Separators",
        Key = AttributeKeys.ShowCategoryNamesasSeparators,
        Description = "Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 5 )]
    public partial class AttributeValues : PersonBlock
    {
        #region Attribute Keys
        protected static class AttributeKeys
        {
            public const string Category = "Category";
            public const string AttributeOrder = "AttributeOrder";
            public const string UseAbbreviatedName = "UseAbbreviatedName";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string ShowCategoryNamesasSeparators = "ShowCategoryNamesasSeparators";
        }

        #endregion Attribute Keys

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
        /// Gets or sets the attribute categories list.
        /// </summary>
        /// <value>
        /// The attribute categories list.
        /// </value>
        protected Dictionary<int, List<int>> AttributeCategoriesList
        {
            get
            {
                Dictionary<int, List<int>> attributeList = ViewState["AttributeList"] as Dictionary<int, List<int>>;
                if ( attributeList == null )
                {
                    attributeList = new Dictionary<int, List<int>>();
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

            SetPanelTitleAndIcon();
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
            AttributeCategoriesList = null;
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
                    int personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;

                    var rockContext = new RockContext();

                    foreach ( int attributeId in AttributeCategoriesList.Keys )
                    {
                        var attribute = AttributeCache.Get( attributeId );

                        if ( Person != null &&
                            attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            Control attributeControl = fsAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                            if ( attributeControl != null )
                            {
                                string originalValue = Person.GetAttributeValue( attribute.Key );
                                string newValue = attribute.FieldType.Field.GetEditValue( attributeControl, attribute.QualifierValues );
                                Rock.Attribute.Helper.SaveAttributeValue( Person, attribute, newValue, rockContext );
                            }
                        }
                    }
                }
                else if ( ViewMode == VIEW_MODE_ORDER && _canAdministrate )
                {
                    // Split and delineate again to remove trailing delimiter
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
            AttributeCategoriesList = new Dictionary<int, List<int>>();

            var categories = GetAttributeValue( AttributeKeys.Category ).SplitDelimitedValues( false ).AsGuidList();
            if ( categories.Any() )
            {
                var orderOverride = new List<int>();
                GetAttributeValue( "AttributeOrder" ).SplitDelimitedValues().ToList().ForEach( a => orderOverride.Add( a.AsInteger() ) );

                var orderedAttributeList = new AttributeService( new RockContext() ).Queryable().Where( a => a.IsActive && a.Categories.Any( c => categories.Contains( c.Guid ) ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToAttributeCacheList();

                foreach ( int attributeId in orderOverride )
                {
                    var attribute = orderedAttributeList.FirstOrDefault( a => a.Id == attributeId );
                    if ( attribute != null && attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AttributeCategoriesList.Add( attribute.Id, attribute.CategoryIds );
                    }
                }

                foreach ( var attribute in orderedAttributeList.Where( a => !orderOverride.Contains( a.Id ) ) )
                {
                    if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AttributeCategoriesList.Add( attribute.Id, attribute.CategoryIds );
                    }
                }
            }

            CreateControls( true );
        }

        /// <summary>
        /// Sets the page title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            CategoryCache category = null;
            var categories = GetAttributeValue( AttributeKeys.Category ).SplitDelimitedValues( false ).AsGuidList();
            if ( categories.Count == 1 )
            {
                category = CategoryCache.Get( categories.First() );
            }

            string panelTitle = this.GetAttributeValue( AttributeKeys.SetPageTitle );
            if ( !string.IsNullOrEmpty( panelTitle ) )
            {
                lTitle.Text = panelTitle;
            }
            else if ( category != null )
            {
                lTitle.Text = category.Name;
            }
            else
            {
                lTitle.Text = "Attribute Values";
            }

            string panelIcon = this.GetAttributeValue( AttributeKeys.SetPageIcon );
            if ( !string.IsNullOrEmpty( panelIcon ) )
            {
                iIcon.Attributes["class"] = panelIcon;
            }
            else if ( category != null )
            {
                iIcon.Attributes["class"] = category.IconCssClass;
            }
        }

        private void CreateControls( bool setValues )
        {
            var showCategoryNamesasSeparators = GetAttributeValue( AttributeKeys.ShowCategoryNamesasSeparators ).AsBoolean();
            fsAttributes.Controls.Clear();

            string validationGroup = string.Format("vgAttributeValues_{0}", this.BlockId );
            valSummaryTop.ValidationGroup = validationGroup;
            btnSave.ValidationGroup = validationGroup;

            hfAttributeOrder.Value = AttributeCategoriesList.Keys.ToList().AsDelimited( "|" );

            if ( Person != null )
            {
                if ( showCategoryNamesasSeparators )
                {
                    var categoryGuids = GetAttributeValue( AttributeKeys.Category ).SplitDelimitedValues( false ).AsGuidList();
                    var categories = new CategoryService( new RockContext() ).GetByGuids( categoryGuids ).OrderBy( a => a.Order );
                    foreach ( var category in categories )
                    {

                        var attributeList = AttributeCategoriesList.Where( a => a.Value.Contains( category.Id ) ).Select( a => a.Key );
                        if ( attributeList.Any() )
                        {
                            var h4 = new HtmlGenericControl( "h4" );
                            h4.InnerText = category.Name;
                            fsAttributes.Controls.Add( h4 );
                            var hr = new HtmlGenericControl( "hr/" );
                            fsAttributes.Controls.Add( hr );

                            CreateAttributeControl( setValues, validationGroup, attributeList );
                        }
                    }
                    
                }
                else
                {
                    CreateAttributeControl( setValues, validationGroup, AttributeCategoriesList.Keys );
                }
            }

            lbOrder.Visible = !showCategoryNamesasSeparators;
            pnlActions.Visible = ( ViewMode != VIEW_MODE_VIEW );
        }

        private void CreateAttributeControl( bool setValues, string validationGroup, IEnumerable<int> attributeList )
        {
            foreach ( int attributeId in attributeList )
            {
                var attribute = AttributeCache.Get( attributeId );
                string attributeValue = Person.GetAttributeValue( attribute.Key );
                string formattedValue = string.Empty;
                string attributeLabel = GetAttributeValue( AttributeKeys.UseAbbreviatedName ).AsBoolean() == false ? attribute.Name : attribute.AbbreviatedName;

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

                        div.Controls.Add( new LiteralControl( " " + attributeLabel ) );
                    }
                    else
                    {
                        if ( attribute.FieldType.Class == typeof(Rock.Field.Types.ImageFieldType).FullName )
                        {
                            formattedValue = attribute.FieldType.Field.FormatValueAsHtml( fsAttributes, attribute.EntityTypeId, Person.Id, attributeValue, attribute.QualifierValues, true );
                        }
                        else
                        {
                            formattedValue = attribute.FieldType.Field.FormatValueAsHtml( fsAttributes, attribute.EntityTypeId, Person.Id, attributeValue, attribute.QualifierValues, false );
                        }

                        if ( !string.IsNullOrWhiteSpace( formattedValue ) )
                        {
                            if ( attribute.FieldType.Class == typeof( Rock.Field.Types.MatrixFieldType ).FullName )
                            {
                                fsAttributes.Controls.Add( new RockLiteral { Label = attributeLabel, Text = formattedValue, CssClass= "matrix-attribute" } );
                            }
                            else
                            {
                                fsAttributes.Controls.Add( new RockLiteral { Label = attributeLabel, Text = formattedValue } );
                            }
                        }
                    }
                }
                else
                {
                    attribute.AddControl( fsAttributes.Controls, attributeValue, validationGroup, setValues, true );
                }
            }
        }

        #endregion

    }
}
