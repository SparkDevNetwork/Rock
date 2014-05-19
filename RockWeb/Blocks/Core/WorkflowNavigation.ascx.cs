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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for navigating workflow types and launching and/or managing workflows.
    /// </summary>
    [DisplayName( "Workflow Navigation" )]
    [Category( "Core" )]
    [Description( "Block for navigating workflow types and launching and/or managing workflows." )]

    [LinkedPage("Entry Page", "Page used to launch a new workflow of the selected type.")]
    [LinkedPage( "Manage Page", "Page used to manage workflows of the selected type." )]
    public partial class WorkflowNavigation : Rock.Web.UI.RockBlock
    {
        #region Fields
        #endregion

        #region Properties

        private List<WorkflowNavigationCategory> RootCategories { get; set; }

        #endregion

        #region Base Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["RootCategories"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RootCategories = new List<WorkflowNavigationCategory>();
            }
            else
            {
                RootCategories = JsonConvert.DeserializeObject<List<WorkflowNavigationCategory>>( json );
            }

            BuildControls();

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
                RootCategories = GetData();
                BuildControls();
            }
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            ViewState["RootCategories"] = JsonConvert.SerializeObject( RootCategories, Formatting.None, jsonSetting );

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

        }

        #endregion

        #region Methods

        private List<WorkflowNavigationCategory> GetData()
        {
            int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.WorkflowType ) ).Id;

            var rockContext = new RockContext();
            var categories = new CategoryService( rockContext ).GetByEntityTypeId( entityTypeId ).ToList();
            var workflowTypes = new WorkflowTypeService( rockContext ).Queryable().ToList();

            return GetCategories( null, categories, workflowTypes );
        }

        private List<WorkflowNavigationCategory> GetCategories( int? parentCategoryId, IEnumerable<Category> categories, IEnumerable<WorkflowType> workflowTypes )
        {
            var items = new List<WorkflowNavigationCategory>();

            foreach ( var category in categories
                .Where( c =>
                    c.ParentCategoryId == parentCategoryId ||
                    ( !c.ParentCategoryId.HasValue && !parentCategoryId.HasValue ) )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name ) )
            {
                if ( category.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                {
                    var categoryItem = new WorkflowNavigationCategory( category );
                    items.Add( categoryItem );

                    // Recurse child categories
                    categoryItem.ChildCategories = GetCategories( category.Id, categories, workflowTypes );

                    // get workflow types
                    categoryItem.WorkflowTypes = new List<WorkflowNavigationWorkflowType>();
                    foreach ( var workflowType in workflowTypes
                        .Where( w =>
                            w.CategoryId == category.Id )
                        .OrderBy( w => w.Order )
                        .ThenBy( w => w.Name ) )
                    {
                        if ( workflowType.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                        {
                            categoryItem.WorkflowTypes.Add( new WorkflowNavigationWorkflowType( workflowType ) );
                        }
                    }
                }
            }

            return items;
        }

        private void BuildControls()
        {
            pnlContent.Controls.Clear();
            foreach ( var childCategory in RootCategories )
            {
                BuildCategoryControl( pnlContent, childCategory );
            }
        }

        private void BuildCategoryControl(Control parentControl, WorkflowNavigationCategory category)
        {
            var divPanel = new HtmlGenericContainer( "div" );
            //divPanel.AddCssClass( "panel panel-default" );
            parentControl.Controls.Add( divPanel );

            var divHeading = new HtmlGenericControl( "div" );
            //divHeading.AddCssClass( "panel-heading" );
            divPanel.Controls.Add( divHeading );

            var headingTitle = new HtmlGenericControl( "h3" );
            //headingTitle.AddCssClass( "panel-title" );
            divHeading.Controls.Add( headingTitle );

            var headingA = new HtmlGenericControl( "a" );
            headingA.Attributes.Add( "data-toggle", "collapse" );
            headingA.Attributes.Add( "data-parent", "#" + parentControl.ClientID );
            headingTitle.Controls.Add( headingA );

            if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
            {
                headingA.Controls.Add( new LiteralControl( string.Format( "<i class='{0}'></i> ", category.IconCssClass ) ) );
            }
            headingA.Controls.Add( new LiteralControl( category.Name ) );

            var divCollapse = new HtmlGenericControl( "div" );
            divCollapse.ID = string.Format( "collapse-category-", category.Id );
            divCollapse.AddCssClass( "panel-collapse collapse" );
            divPanel.Controls.Add( divCollapse );

            headingA.Attributes.Add( "href", "#" + divCollapse.ClientID );

            if (category.WorkflowTypes.Any())
            {
                var ulGroup = new HtmlGenericControl( "ul" );
                ulGroup.AddCssClass( "list-group" );
                divCollapse.Controls.Add( ulGroup );

                foreach ( var workflowType in category.WorkflowTypes )
                {
                    var li = new HtmlGenericControl( "li" );
                    li.AddCssClass( "list-group-item" );
                    ulGroup.Controls.Add( li );

                    var qryParms = new Dictionary<string, string>();
                    qryParms.Add( "WorkflowTypeId", workflowType.Id.ToString() );

                    var aNew = new HtmlGenericControl( "a" );
                    aNew.Attributes.Add( "href", LinkedPageUrl( "EntryPage", qryParms ) );
                    li.Controls.Add( aNew );

                    if ( !string.IsNullOrWhiteSpace( workflowType.IconCssClass ) )
                    {
                        aNew.Controls.Add( new LiteralControl( string.Format( "<i class='{0}'></i> ", workflowType.IconCssClass ) ) );
                    }

                    aNew.Controls.Add( new LiteralControl( workflowType.Name ) );

                    if ( IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
                    {
                        li.Controls.Add( new LiteralControl( " " ) );

                        var aManage = new HtmlGenericControl( "a" );
                        aManage.AddCssClass( "pull-right" );
                        aManage.Attributes.Add( "href", LinkedPageUrl( "ManagePage", qryParms ) );
                        aManage.InnerText = "Manage";
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
        /// Initializes a new instance of the <see cref="WorkflowNavigationWorkflowType"/> class.
        /// </summary>
        public WorkflowNavigationWorkflowType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNavigationWorkflowType"/> class.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        public WorkflowNavigationWorkflowType( WorkflowType workflowType )
        {
            Id = workflowType.Id;
            Name = workflowType.Name;
            Description = workflowType.Description;
            IconCssClass = workflowType.IconCssClass;
            WorkTerm = workflowType.WorkTerm;
        }
    }
}