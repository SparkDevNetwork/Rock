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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Block for navigating workflow types and launching and/or managing workflows.
    /// </summary>
    [DisplayName( "Workflow Navigation" )]
    [Category( "WorkFlow" )]
    [Description( "Block for navigating workflow types and launching and/or managing workflows." )]

    [CategoryField( "Categories", "The categories to display", true, "Rock.Model.WorkflowType", "", "", false, "", "", 0 )]
    [BooleanField( "Include Child Categories", "Should descendent categories of the selected Categories be included?", true, "", 1 )]
    [LinkedPage( "Entry Page", "Page used to launch a new workflow of the selected type.", true, "", "", 2 )]
    [LinkedPage( "Manage Page", "Page used to manage workflows of the selected type.", true, "", "", 3 )]
    public partial class WorkflowNavigation : Rock.Web.UI.RockBlock
    {
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
                BuildControls();
            }
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
            BuildControls();
        }

        #endregion

        #region Methods

        private List<WorkflowNavigationCategory> GetData()
        {
            int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.WorkflowType ) ).Id;

            var selectedCategories = new List<Guid>();
            GetAttributeValue( "Categories" ).SplitDelimitedValues().ToList().ForEach( c => selectedCategories.Add( c.AsGuid() ) );

            bool includeChildCategories = GetAttributeValue( "IncludeChildCategories" ).AsBoolean();

            var rockContext = new RockContext();
            var categories = new CategoryService( rockContext ).GetNavigationItems( entityTypeId, selectedCategories, includeChildCategories, CurrentPerson );
            var workflowTypes = new WorkflowTypeService( rockContext ).Queryable( "ActivityTypes.ActionTypes" ).ToList();
            return GetWorkflowNavigationCategories( categories, workflowTypes );
        }

        private List<WorkflowNavigationCategory> GetWorkflowNavigationCategories( List<CategoryNavigationItem> categoryItems, IEnumerable<WorkflowType> workflowTypes )
        {
            var items = new List<WorkflowNavigationCategory>();

            foreach ( var category in categoryItems )
            {
                var workflowNavigationCategory = new WorkflowNavigationCategory( category.Category );
                workflowNavigationCategory.ChildCategories = GetWorkflowNavigationCategories( category.ChildCategories, workflowTypes );
                workflowNavigationCategory.WorkflowTypes = new List<WorkflowNavigationWorkflowType>();
                foreach ( var workflowType in workflowTypes
                    .Where( w => w.CategoryId == category.Category.Id )
                    .OrderBy( w => w.Order )
                    .ThenBy( w => w.Name ) )
                {
                    if ( workflowType.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    {
                        workflowNavigationCategory.WorkflowTypes.Add( new WorkflowNavigationWorkflowType( workflowType, workflowType.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson ) ) );
                    }
                }

                items.Add( workflowNavigationCategory );
            }

            return items;
        }

        private void BuildControls()
        {
            pnlContent.Controls.Clear();
            foreach ( var childCategory in GetData() )
            {
                BuildCategoryControl( pnlContent, childCategory );
            }
        }

        private void BuildCategoryControl( Control parentControl, WorkflowNavigationCategory category )
        {
            var divPanel = new HtmlGenericContainer( "div" );
            divPanel.AddCssClass( "panel panel-workflowitem" );
            parentControl.Controls.Add( divPanel );

            var divPanelHeading = new HtmlGenericControl( "div" );
            divPanelHeading.AddCssClass( "panel-heading" );
            divPanel.Controls.Add( divPanelHeading );

            var headingA = new HtmlGenericControl( "a" );
            headingA.Attributes.Add( "data-toggle", "collapse" );
            headingA.Attributes.Add( "data-parent", "#" + parentControl.ClientID );
            headingA.AddCssClass( "collapsed clearfix" );
            divPanelHeading.Controls.Add( headingA );

            var divHeadingTitle = new HtmlGenericControl( "div" );
            divHeadingTitle.AddCssClass( "panel-title clearfix" );
            headingA.Controls.Add( divHeadingTitle );

            var headingTitle = new HtmlGenericControl( "h3" );
            headingTitle.AddCssClass( "pull-left" );
            divHeadingTitle.Controls.Add( headingTitle );

            var panelNavigation = new HtmlGenericControl( "i" );
            panelNavigation.AddCssClass( "fa panel-navigation pull-right" );
            divHeadingTitle.Controls.Add( panelNavigation );

            if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
            {
                headingTitle.Controls.Add( new LiteralControl( string.Format( "<i class='{0} icon-fw'></i> ", category.IconCssClass ) ) );
            }
            headingTitle.Controls.Add( new LiteralControl( category.Name ) );

            var divCollapse = new HtmlGenericControl( "div" );
            divCollapse.ID = string.Format( "collapse-category-", category.Id );
            divCollapse.AddCssClass( "panel-collapse collapse" );
            divPanel.Controls.Add( divCollapse );

            headingA.Attributes.Add( "href", "#" + divCollapse.ClientID );

            if ( category.WorkflowTypes.Any() )
            {
                var ulGroup = new HtmlGenericControl( "ul" );
                ulGroup.AddCssClass( "list-group" );
                divCollapse.Controls.Add( ulGroup );

                foreach ( var workflowType in category.WorkflowTypes )
                {
                    var li = new HtmlGenericControl( "li" );
                    li.AddCssClass( "list-group-item clickable" );
                    ulGroup.Controls.Add( li );

                    var qryParms = new Dictionary<string, string>();
                    qryParms.Add( "WorkflowTypeId", workflowType.Id.ToString() );

                    bool showLinkToEntry = workflowType.HasForms && workflowType.IsActive;

                    var aNew = new HtmlGenericControl( showLinkToEntry ? "a" : "span" );
                    if (workflowType.HasForms)
                    {
                        aNew.Attributes.Add( "href", LinkedPageUrl( "EntryPage", qryParms ) );
                    }
                    li.Controls.Add( aNew );

                    if ( !string.IsNullOrWhiteSpace( workflowType.IconCssClass ) )
                    {
                        aNew.Controls.Add( new LiteralControl( string.Format( "<i class='{0} icon-fw'></i> ", workflowType.IconCssClass ) ) );
                    }

                    aNew.Controls.Add( new LiteralControl( workflowType.Name ) );

                    if ( workflowType.CanManage || IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
                    {
                        li.Controls.Add( new LiteralControl( " " ) );

                        var aManage = new HtmlGenericControl( "a" );
                        aManage.AddCssClass( "pull-right" );
                        aManage.Attributes.Add( "href", LinkedPageUrl( "ManagePage", qryParms ) );
                        var iManageIcon = new HtmlGenericControl( "i" );
                        aManage.Controls.Add( iManageIcon );
                        iManageIcon.AddCssClass( "fa fa-wrench" );
                        li.Controls.Add( aManage );
                    }
                }
            }

            if ( category.ChildCategories.Any() )
            {
                var divBody = new HtmlGenericControl( "div" );
                //divBody.AddCssClass( "panel-body" );
                divCollapse.Controls.Add( divBody );

                foreach ( var childCategory in category.ChildCategories )
                {
                    BuildCategoryControl( divBody, childCategory );
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class WorkflowNavigationItem
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        /// <value>
        /// The color of the highlight.
        /// </value>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNavigationItem"/> class.
        /// </summary>
        public WorkflowNavigationItem()
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class WorkflowNavigationCategory : WorkflowNavigationItem
    {
        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<WorkflowNavigationCategory> ChildCategories { get; set; }

        /// <summary>
        /// Gets or sets the workflow types.
        /// </summary>
        /// <value>
        /// The workflow types.
        /// </value>
        public List<WorkflowNavigationWorkflowType> WorkflowTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNavigationCategory"/> class.
        /// </summary>
        public WorkflowNavigationCategory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNavigationItem"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        public WorkflowNavigationCategory( Category category )
        {
            Id = category.Id;
            Name = category.Name;
            Description = category.Description;
            IconCssClass = category.IconCssClass;
            HighlightColor = category.HighlightColor;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class WorkflowNavigationWorkflowType : WorkflowNavigationItem
    {
        /// <summary>
        /// Gets or sets the work term.
        /// </summary>
        /// <value>
        /// The work term.
        /// </value>
        public string WorkTerm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has forms.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has forms; otherwise, <c>false</c>.
        /// </value>
        public bool HasForms { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can manage.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can manage; otherwise, <c>false</c>.
        /// </value>
        public bool CanManage { get; set; }

        /// <summary>
        /// Gets or sets whether or not the workflow type is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the workflow type is active, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNavigationWorkflowType"/> class.
        /// </summary>
        public WorkflowNavigationWorkflowType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNavigationWorkflowType"/> class.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        public WorkflowNavigationWorkflowType( WorkflowType workflowType, bool canManage )
        {
            Id = workflowType.Id;
            Name = workflowType.Name;
            Description = workflowType.Description;
            IconCssClass = workflowType.IconCssClass;
            WorkTerm = workflowType.WorkTerm;
            HasForms = workflowType.HasActiveForms;
            HighlightColor = string.Empty;
            CanManage = canManage;

            if ( workflowType.IsActive.HasValue )
            {
                IsActive = workflowType.IsActive.Value;
            }
            else
            {
                IsActive = true;
            }
            
        }
    }
}