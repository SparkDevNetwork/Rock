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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Opportunity Search Lava" )]
    [Category( "Connection" )]
    [Description( "Allows users to search for an opportunity to join" )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of opportunities.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunitySearch.lava' %}", "", 2 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the connection type name.", false )]
    [BooleanField( "Display Name Filter", "Display the name filter", true )]
    [BooleanField( "Display Campus Filter", "Display the campus filter", true )]
    [BooleanField( "Display Attribute Filters", "Display the attribute filters", true )]
    [LinkedPage( "Detail Page", "The page used to view a connection opportunity." )]
    [IntegerField( "Connection Type Id", "The Id of the connection type whose opportunities are displayed.", true, 1 )]

    public partial class ExternalOpportunitySearch : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            SetFilters();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                SetFilters();
                UpdateList();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
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
            UpdateList();
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            UpdateList();
        }


        #endregion

        #region Internal Methods

        /// <summary>
        /// Updates the list.
        /// </summary>
        private void UpdateList()
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionType = new ConnectionTypeService( rockContext ).Get( GetAttributeValue( "ConnectionTypeId" ).AsInteger() );
                var qrySearch = connectionType.ConnectionOpportunities.ToList();

                if ( GetAttributeValue( "DisplayNameFilter" ).AsBoolean() )
                {
                    if ( !string.IsNullOrWhiteSpace( tbSearchName.Text ) )
                    {
                        var searchTerms = tbSearchName.Text.ToLower().SplitDelimitedValues( true );
                        qrySearch = qrySearch.Where( o => searchTerms.Any( t => t.Contains( o.Name.ToLower() ) || o.Name.ToLower().Contains( t ) ) ).ToList();
                    }
                }

                if ( GetAttributeValue( "DisplayCampusFilter" ).AsBoolean() )
                {
                    var searchCampuses = cblCampus.SelectedValuesAsInt;
                    if ( searchCampuses.Count > 0 )
                    {
                        qrySearch = qrySearch.Where( o => o.ConnectionOpportunityCampuses.Any( c => searchCampuses.Contains( c.CampusId ) ) ).ToList();
                    }
                }

                if ( GetAttributeValue( "DisplayAttributeFilters" ).AsBoolean() )
                {
                    // Filter query by any configured attribute filters
                    if ( AvailableAttributes != null && AvailableAttributes.Any() )
                    {
                        var attributeValueService = new AttributeValueService( rockContext );
                        var parameterExpression = attributeValueService.ParameterExpression;

                        foreach ( var attribute in AvailableAttributes )
                        {
                            var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                            if ( filterControl != null )
                            {
                                var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues );
                                var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                                if ( expression != null )
                                {
                                    var attributeValues = attributeValueService
                                        .Queryable()
                                        .Where( v => v.Attribute.Id == attribute.Id );

                                    attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                    qrySearch = qrySearch.Where( o => attributeValues.Select( v => v.EntityId ).Contains( o.Id ) ).ToList();
                                }
                            }
                        }
                    }
                }

                var opportunitySummaries = new List<OpportunitySummary>();
                foreach ( var opportunity in qrySearch )
                {
                    opportunitySummaries.Add( new OpportunitySummary
                    {
                        IconCssClass = opportunity.IconCssClass,
                        Name = opportunity.PublicName,
                        PhotoUrl = opportunity.PhotoUrl,
                        Description = opportunity.Description.ScrubHtmlAndConvertCrLfToBr(),
                        Id = opportunity.Id
                    } );
                }

                var opportunities = opportunitySummaries
                    .OrderBy( e => e.Name )
                    .ToList();

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Opportunities", opportunities );
                mergeFields.Add( "CurrentPerson", CurrentPerson );

                var pageReference = new PageReference( GetAttributeValue( "DetailPage" ), null );
                mergeFields.Add( "DetailPage", pageReference.BuildUrl() );

                lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

                if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() )
                {
                    string pageTitle = "Connection";
                    RockPage.PageTitle = pageTitle;
                    RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                    RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                }

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        /// <summary>
        /// Sets the filters.
        /// </summary>
        private void SetFilters()
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionType = new ConnectionTypeService( rockContext ).Get( GetAttributeValue( "ConnectionTypeId" ).AsInteger() );

                if ( !GetAttributeValue( "DisplayNameFilter" ).AsBoolean() )
                {
                    tbSearchName.Visible = false;
                }

                if ( GetAttributeValue( "DisplayCampusFilter" ).AsBoolean() )
                {
                    cblCampus.Visible = true;
                    cblCampus.DataSource = CampusCache.All();
                    cblCampus.DataBind();
                }
                else
                {
                    cblCampus.Visible = false;
                }

                if ( GetAttributeValue( "DisplayAttributeFilters" ).AsBoolean() )
                {
                    // Parse the attribute filters 
                    AvailableAttributes = new List<AttributeCache>();
                    if ( connectionType != null )
                    {
                        int entityTypeId = new ConnectionOpportunity().TypeId;
                        foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                            .Where( a =>
                                a.EntityTypeId == entityTypeId &&
                                a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                a.EntityTypeQualifierValue.Equals( connectionType.Id.ToString() ) )
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name ) )
                        {
                            AvailableAttributes.Add( AttributeCache.Read( attributeModel ) );
                        }
                    }

                    // Clear the filter controls
                    phAttributeFilters.Controls.Clear();

                    if ( AvailableAttributes != null )
                    {
                        foreach ( var attribute in AvailableAttributes )
                        {
                            var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false );
                            if ( control != null )
                            {
                                if ( control is IRockControl )
                                {
                                    var rockControl = (IRockControl)control;
                                    rockControl.Label = attribute.Name;
                                    rockControl.Help = attribute.Description;
                                    phAttributeFilters.Controls.Add( control );
                                }
                                else
                                {
                                    var wrapper = new RockControlWrapper();
                                    wrapper.ID = control.ID + "_wrapper";
                                    wrapper.Label = attribute.Name;
                                    wrapper.Controls.Add( control );
                                    phAttributeFilters.Controls.Add( wrapper );
                                }
                            }
                        }
                    }
                }
                else
                {
                    phAttributeFilters.Visible = false;
                }
            }
        }

        /// <summary>
        /// A class for lava to access connection opportunity data
        /// </summary>
        [DotLiquid.LiquidType( "IconCssClass", "Name", "PhotoUrl", "Description", "Id" )]
        public class OpportunitySummary
        {
            public String IconCssClass { get; set; }
            public String Name { get; set; }
            public String PhotoUrl { get; set; }
            public String Description { get; set; }
            public int Id { get; set; }
        }

        #endregion

    }
}