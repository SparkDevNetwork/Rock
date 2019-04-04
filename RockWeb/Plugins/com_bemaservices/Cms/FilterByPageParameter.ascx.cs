using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using System.Web;
using Rock.Security;

namespace RockWeb.Plugins.com_bemaservices.Cms
{

    /// <summary>
    /// Filter block that passes the filter values as query string parameters.
    /// </summary>
    [DisplayName( "Filter By Page Parameters" )]
    [Category( "BEMA Services > Cms" )]
    [Description( "Filter block that passes the filter values as query string parameters." )]

    [TextField( "Heading", "The text to display as the heading.", true, "Filters", "", 1 )]
    [TextField( "Heading Icon CSS Class", "The css class name to use for the heading icon. ", true, "fa fa-filter", "", 2 )]
    [IntegerField( "Filters Per Row", "The number of filters to have per row.  Maximum is 12.", true, 2, "", 3 )]
    [BooleanField( "Show Reset Filters", "Determines if the Reset Filters button should be displayed", true, "", 4 )]
    [TextField( "Filter Button Text", "Sets the button text for the filter button.", true, "Filter", "", 5)]
    [LinkedPage( "Page Redirect", "If set, the filter button will redirect to the selected page.", false, "", "", 6 )]
    public partial class FilterByPageParameter : RockBlock, IDynamicAttributesBlock
    {
        #region Fields

        int _blockTypeEntityId;
        Block _block;
        int _filtersPerRow;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _blockTypeEntityId = EntityTypeCache.GetId<Block>().Value;
            _block = new BlockService( new RockContext() ).Get( this.BlockId );

            lHeading.Text = GetAttributeValue( "Heading" );
            lHeadingIcon.Text = "<i class='" + GetAttributeValue( "HeadingIconCSSClass" ) + "'></i>";

            int perRow = GetAttributeValue( "FiltersPerRow" ).AsInteger();
            if ( perRow > 12 )
            {
                perRow = 12;
            }

            _filtersPerRow = perRow;

            gFilters.DataKeyNames = new string[] { "Id" };
            gFilters.Actions.ShowAdd = true;
            gFilters.Actions.AddClick += gFilters_Add;
            gFilters.Actions.ShowExcelExport = false;
            gFilters.Actions.ShowMergeTemplate = false;

            edtFilter.IsActive = true;
            edtFilter.IsAnalyticsVisible = false;
            edtFilter.IsAnalyticHistory = false;
            edtFilter.IsCategoriesVisible = false;
            edtFilter.IsIndexingEnabled = false;
            edtFilter.IsIconCssClassVisible = false;
            edtFilter.AllowSearchVisible = false;
            edtFilter.IsShowInGridVisible = false;

            // hide these attribute settings manually since there isn't a property to do so.
            edtFilter.FindControl( "_cbEnableHistory" ).Visible = false;
            edtFilter.FindControl( "cbRequired" ).Visible = false;

            lbEdit.Visible = IsUserAuthorized( "Edit" );

            var filterButtonText = GetAttributeValue( "FilterButtonText" );
            btnFilter.Text = filterButtonText.IsNotNullOrWhiteSpace() ? filterButtonText : "Filter";

            btnResetFilters.Visible = GetAttributeValue( "ShowResetFilters" ).AsBoolean();

            var securityField = gFilters.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            }

            mdFilter.SaveClick += mdFilter_SaveClick;

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if (!Page.IsPostBack)
            {
                var query = new AttributeService( new RockContext() ).Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
                var attribsWithDefaultValue = query.AsQueryable().Where( a => a.DefaultValue != null && a.DefaultValue != "" ).ToList();

                // if we have any filters with default values, we want to load this block with the page parameters already set.
                if ( attribsWithDefaultValue.Any() && !this.RockPage.PageParameters().Any() )
                {
                    ResetFilters();
                }
                else
                {
                    LoadFilters();
                }                
            }
            else
            {
                LoadFilters();
            }          

            base.OnLoad( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the filters.
        /// </summary>
        private void LoadFilters()
        {
            BuildControls();

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    var attributeCache = AttributeCache.Get( attribute.Value.Guid );

                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        var value = Request.QueryString[attribute.Key];
                        if ( value != null )
                        {
                            attributeCache.FieldType.Field.SetEditValue( control, null, value );
                        }
                        else if ( attribute.Value.DefaultValue != null )
                        {
                            attributeCache.FieldType.Field.SetEditValue( control, null, attribute.Value.DefaultValue );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the filter controls.
        /// </summary>
        private void BuildControls()
        {
            var query = new AttributeService( new RockContext() ).Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            var attributes = query.OrderBy( a => a.Order ).ToList();

            var exclusions = new List<string>();
            exclusions.Add( "PageRedirect" );

            _block.LoadAttributes();
            phAttributes.Controls.Clear();

            var attributeKeys = new List<string>();
            foreach ( var attribute in attributes )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    attributeKeys.Add( attribute.Key );
                }
            }

            Helper.AddEditControls( "", attributeKeys, _block, phAttributes, "", false, exclusions, _filtersPerRow );
        }

        /// <summary>
        /// Resets the filters to their original state.  Any filters with default values will be set as well.
        /// </summary>
        private void ResetFilters()
        {
            var queryString = HttpUtility.ParseQueryString( String.Empty );

            BuildControls();

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        string value = attribute.Value.DefaultValue;

                        if ( value.IsNotNullOrWhiteSpace() )
                        {
                            queryString.Set( attribute.Key, value );
                        }
                    }
                }
            }

            if ( queryString.AllKeys.Any() )
            {
                Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
            }
            else
            {
                Response.Redirect( Request.Url.AbsolutePath, false );
            }
        }

        /// <summary>
        /// Binds the filter grid.
        /// </summary>
        private void BindGrid()
        {
            IQueryable<Rock.Model.Attribute> query = null;
            var attributeService = new AttributeService( new RockContext() );

            query = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            gFilters.DataSource = query.OrderBy( a => a.Order ).ToList();
            gFilters.DataBind();
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
            LoadFilters();
        }

        /// <summary>
        /// Handles the btnFilter event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            var queryString = HttpUtility.ParseQueryString( String.Empty );
            _block.LoadAttributes( new RockContext() );

            if ( _block.Attributes != null )
            {
                foreach ( var attribute in _block.Attributes )
                {
                    Control control = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        string value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );

                        if ( value.IsNotNullOrWhiteSpace() )
                        {
                            queryString.Set( attribute.Key, value );
                        }
                    }
                }
            }

            string url = Request.Url.AbsolutePath;

            Guid? pageGuid = GetAttributeValue( "PageRedirect" ).AsGuidOrNull();
            if ( pageGuid.HasValue )
            {
                var page = PageCache.Get( pageGuid.Value );

                url = System.Web.VirtualPathUtility.ToAbsolute( string.Format( "~/page/{0}", page.Id ) );
            }

            if ( queryString.AllKeys.Any() )
            {
                Response.Redirect( string.Format( "{0}?{1}", url, queryString ), false );
            }
            else
            {
                Response.Redirect( url, false );
            }
        }

        /// <summary>
        /// Handles the btnResetFilters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnResetFilters_Click( object sender, EventArgs e )
        {
            ResetFilters();
        }

        /// <summary>
        /// Handles the EditFilters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditFilters_Click( object sender, EventArgs e )
        {
            BindGrid();

            lHeading.Text = "Filter Settings";
            lHeadingIcon.Visible = false;
            lbEdit.Visible = false;

            pnlEdit.Visible = true;
            pnlFilters.Visible = false;
        }

        /// <summary>
        /// Handles the lbClose event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbClose_Click( object sender, EventArgs e )
        {
            ResetFilters();
            Response.Redirect( Request.Url.AbsolutePath );
        }

        /// <summary>
        /// Handles the GridReorder events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gFilters_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var qry = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() );
            qry = qry.OrderBy( a => a.Order );
            var updatedAttributeIds = attributeService.Reorder( qry.ToList(), e.OldIndex, e.NewIndex );

            rockContext.SaveChanges();

            BindGrid();
            LoadFilters();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gFilters_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int attributeId = ( int ) gFilters.DataKeys[e.Row.RowIndex].Value;

                var attribute = Rock.Web.Cache.AttributeCache.Get( attributeId );
                var fieldType = FieldTypeCache.Get( attribute.FieldTypeId );

                Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                if ( lDefaultValue != null )
                {
                    lDefaultValue.Text = fieldType.Field.FormatValueAsHtml( lDefaultValue, attribute.EntityTypeId, null, attribute.DefaultValue, attribute.QualifierValues, true );
                }
            }
        }

        /// <summary>
        /// Handles the Edit filters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFilters_Edit( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var attribute = new AttributeService( rockContext ).Get( e.RowKeyId );

            edtFilter.ReservedKeyNames = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() )
                 .Where( a => a.Id != e.RowKeyId )
                 .Select( a => a.Key )
                 .Distinct()
                 .ToList();

            edtFilter.SetAttributeProperties( attribute );

            mdFilter.Title = "Edit Filter";
            mdFilter.Show();
        }

        /// <summary>
        /// Handes the Delete filters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFilters_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            var attribute = attributeService.Get( e.RowKeyId );
            if ( attribute != null )
            {
                attributeService.Delete( attribute );

                rockContext.SaveChanges();
            }

            BindGrid();
            LoadFilters();
        }

        /// <summary>
        /// Handles the Add actions event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFilters_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            // reset editor
            edtFilter.Name = "";
            edtFilter.Key = "";
            edtFilter.AttributeId = null;
            edtFilter.IsFieldTypeEditable = true;
            edtFilter.SetAttributeFieldType( FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id, null );

            edtFilter.ReservedKeyNames = attributeService.Get( _blockTypeEntityId, "Id", _block.Id.ToString() )
                 .Select( a => a.Key )
                 .Distinct()
                 .ToList();

            mdFilter.Title = "Add Filter";
            mdFilter.Show();
        }

        /// <summary>
        /// Handles the Save filters event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdFilter_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = null;

            // sets the attribute to use the "CustomSetting" attribute
            var entityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE ).Id;
            var entityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.BLOCK ).Id;

            edtFilter.CategoryIds = new CategoryService( new RockContext() ).Queryable().Where( c => c.Name == "CustomSetting" &&
                                                                                                c.EntityTypeId == entityTypeId &&
                                                                                                c.EntityTypeQualifierColumn == "EntityTypeId" &&
                                                                                                c.EntityTypeQualifierValue == entityId.ToString() )
                                                                                        .Select( c => c.Id );

            attribute = Helper.SaveAttributeEdits( edtFilter, _blockTypeEntityId, "Id", _block.Id.ToString() );

            // Attribute will be null if it was not valid
            if ( attribute == null )
            {
                return;
            }

            mdFilter.Hide();
            BindGrid();
        }

        #endregion
    }
}