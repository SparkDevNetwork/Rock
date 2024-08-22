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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Block that enables previewing an entity set and then launching a workflow for each item within the set. This block was designed to
    /// work in tandem with <see cref="Rock.Web.UI.Controls.GridActions.ShowLaunchWorkflow"/>.
    /// </summary>
    [DisplayName( "Workflow Launch" )]
    [Category( "Workflow" )]
    [Description( "Block that enables previewing an entity set and then launching a workflow for each item within the set." )]

    #region Block Attributes

    [WorkflowTypeField(
        "Workflow Types",
        Key = AttributeKey.WorkflowTypes,
        Description = "Only the selected workflow types will be shown. If left blank, any workflow type can be launched.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Allow Multiple Workflow Launches",
        Key = AttributeKey.AllowMultipleWorkflowLaunches,
        Description = "If set to yes, allows launching multiple different types of workflows. After one is launched, the block will allow the individual to select another type to be launched. This will only show if more than one type is configured.",
        DefaultBooleanValue = AttributeDefault.AllowMultipleWorkflowLaunches,
        Order = 2 )]

    [TextField(
        "Panel Title",
        Description = "The title to display in the block panel.",
        DefaultValue = AttributeDefault.PanelTitle,
        Order = 3,
        Key = AttributeKey.PanelTitle )]

    [TextField(
        "Panel Title Icon CSS Class",
        Description = "The icon to use before the panel title.",
        DefaultValue = AttributeDefault.PanelIcon,
        Order = 4,
        Key = AttributeKey.PanelIcon )]

    [IntegerField(
        "Default Number of Items to Show",
        Description = "The number of entities to list on screen before summarizing ('...and xx more').",
        DefaultIntegerValue = AttributeDefault.DefaultNumberOfItemsToShow,
        Order = 5,
        Key = AttributeKey.DefaultNumberOfItemsToShow )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "D7C15C1B-7487-42C3-A485-AD154F46558A" )]
    public partial class WorkflowLaunch : Rock.Web.UI.RockBlock
    {

        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The workflow types
            /// </summary>
            public const string WorkflowTypes = "WorkflowTypes";

            /// <summary>
            /// The allow multiple workflow launches
            /// </summary>
            public const string AllowMultipleWorkflowLaunches = "AllowMultipleWorkflowLaunches";

            /// <summary>
            /// The panel title
            /// </summary>
            public const string PanelTitle = "PanelTitle";

            /// <summary>
            /// The panel icon
            /// </summary>
            public const string PanelIcon = "PanelIcon";

            /// <summary>
            /// The default number of items to show
            /// </summary>
            public const string DefaultNumberOfItemsToShow = "DefaultNumberOfItemsToShow";
        }

        /// <summary>
        /// Attribute value defaults
        /// </summary>
        private static class AttributeDefault
        {
            /// <summary>
            /// The default number of items to show
            /// </summary>
            public const int DefaultNumberOfItemsToShow = 50;

            /// <summary>
            /// The panel title
            /// </summary>
            public const string PanelTitle = "Workflow Launch";

            /// <summary>
            /// The panel icon
            /// </summary>
            public const string PanelIcon = "fa fa-cog";

            /// <summary>
            /// The allow multiple workflow launches
            /// </summary>
            public const bool AllowMultipleWorkflowLaunches = true;
        }

        /// <summary>
        /// Page Parameter Keys
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The entity set identifier
            /// </summary>
            public const string EntitySetId = "EntitySetId";

            /// <summary>
            /// The workflow type identifier
            /// </summary>
            public const string WorkflowTypeId = "WorkflowTypeId";

            /// <summary>
            /// The by pass confirm
            /// </summary>
            public const string BypassConfirm = "BypassConfirm";
        }

        /// <summary>
        /// View State Keys
        /// </summary>
        private static class ViewStateKey
        {
            /// <summary>
            /// The entity set item count
            /// </summary>
            public const string EntitySetItemCount = "EntitySetItemCount";

            /// <summary>
            /// The do show all
            /// </summary>
            public const string DoShowAll = "DoShowAll";

            /// <summary>
            /// The workflow type identifier
            /// </summary>
            public const string WorkflowTypeId = "WorkflowTypeId";

            /// <summary>
            /// The has launched
            /// </summary>
            public const string HasLaunched = "HasLaunched";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether all entities are shown or only the default number to show.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [do show all]; otherwise, <c>false</c>.
        /// </value>
        private bool _doShowAll
        {
            get
            {
                return ViewState[ViewStateKey.DoShowAll].ToStringSafe().AsBoolean();
            }
            set
            {
                ViewState[ViewStateKey.DoShowAll] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has launched.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has launched; otherwise, <c>false</c>.
        /// </value>
        private bool _hasLaunched
        {
            get
            {
                return ViewState[ViewStateKey.HasLaunched].ToStringSafe().AsBoolean();
            }
            set
            {
                ViewState[ViewStateKey.HasLaunched] = value;
            }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( PageParameter( PageParameterKey.BypassConfirm ).AsBoolean() && !PageParameter( PageParameterKey.WorkflowTypeId ).IsNullOrWhiteSpace() )
                {
                    LaunchWorkflows();
                    _hasLaunched = true;
                }

                RenderState();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the lbShowAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowAll_Click( object sender, EventArgs e )
        {
            _doShowAll = true;
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnLaunch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLaunch_Click( object sender, EventArgs e )
        {
            LaunchWorkflows();
            _hasLaunched = true;
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnReset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReset_Click( object sender, EventArgs e )
        {
            _hasLaunched = false;
            RenderState();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Gets the selected workflow type identifier.
        /// </summary>
        /// <returns></returns>
        private WorkflowTypeCache GetSelectedWorkflowType()
        {
            var workflowTypeId = PageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();

            if ( workflowTypeId.HasValue )
            {
                return WorkflowTypeCache.Get( workflowTypeId.Value );
            }

            var workflowTypes = GetAttributeValues( AttributeKey.WorkflowTypes )
                .Select( WorkflowTypeCache.Get )
                .ToList();

            if ( workflowTypes.Count == 1 )
            {
                var workflowType = workflowTypes.Single();
                return workflowType;
            }

            workflowTypeId = workflowTypes.Any() ?
                ddlWorkflowType.SelectedValueAsInt() :
                wtpWorkflowType.SelectedValueAsInt();

            return WorkflowTypeCache.Get( workflowTypeId ?? 0 );
        }

        /// <summary>
        /// Launches the workflows.
        /// </summary>
        private void LaunchWorkflows()
        {
            var workflowType = GetSelectedWorkflowType();

            if ( workflowType == null )
            {
                // Validator control will alert the user
                return;
            }

            var workflowAttributes = PageParameters().ToDictionary(k => k.Key, v => v.Value.ToString());
            var entitySetId = GetEntitySetId();
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            entitySetService.LaunchWorkflows( entitySetId, workflowType.Id, CurrentPersonAliasId, workflowAttributes );
        }

        /// <summary>
        /// Renders the state.
        /// </summary>
        private void RenderState()
        {
            BindTitleBar();

            nbNotificationBox.Visible = false;
            var isValid = ValidateBlockRequirements();

            if ( !isValid )
            {
                HideAllControls();
                return;
            }

            BindRepeater();
            BindWorkflowTypeControls();
            BindShowAllControls();
            BindButtons();
            BindSuccessMessage();
            BindEntityTypeName();
        }

        /// <summary>
        /// Binds the success message.
        /// </summary>
        private void BindSuccessMessage()
        {
            if ( !_hasLaunched )
            {
                return;
            }

            var workflowType = GetSelectedWorkflowType();
            var entityTypeCache = GetEntityTypeCache();

            ShowSuccess( string.Format( "A new {0} workflow is being launched for each of the {1} above.",
                workflowType == null ? string.Empty : workflowType.Name,
                entityTypeCache == null ? "entities" : entityTypeCache.FriendlyName.Pluralize() ) );
        }

        /// <summary>
        /// Binds the buttons.
        /// </summary>
        private void BindButtons()
        {
            btnLaunch.Visible = !_hasLaunched;

            if ( _hasLaunched )
            {
                var allowMultiple = GetAttributeValue( AttributeKey.AllowMultipleWorkflowLaunches ).AsBooleanOrNull() ??
                    AttributeDefault.AllowMultipleWorkflowLaunches;

                btnReset.Visible = allowMultiple;
            }
            else
            {
                btnReset.Visible = false;
            }
        }

        /// <summary>
        /// Binds the title bar.
        /// </summary>
        private void BindTitleBar()
        {
            var title = GetAttributeValue( AttributeKey.PanelTitle );

            if ( title.IsNullOrWhiteSpace() )
            {
                title = AttributeDefault.PanelTitle;
            }

            lTitle.Text = title;

            var icon = GetAttributeValue( AttributeKey.PanelIcon );

            if ( icon.IsNullOrWhiteSpace() )
            {
                icon = AttributeDefault.PanelIcon;
            }

            lIcon.Text = string.Format( "<i class='{0}'></i>", icon );
        }

        /// <summary>
        /// Ensures the block requirements.
        /// </summary>
        /// <returns></returns>
        private bool ValidateBlockRequirements()
        {
            var entitySetId = GetEntitySetId();

            if ( entitySetId == default( int ) )
            {
                ShowError( "An entity set id is required" );
                return false;
            }

            if ( GetEntityQuery() == null )
            {
                ShowError( "A valid entity set is required" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hides all controls.
        /// </summary>
        private void HideAllControls()
        {
            lbShowAll.Visible = false;
            wtpWorkflowType.Visible = false;
            ddlWorkflowType.Visible = false;
            btnLaunch.Visible = false;
            btnReset.Visible = false;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowError( string message )
        {
            if ( nbNotificationBox.Visible )
            {
                return;
            }

            nbNotificationBox.Text = message;
            nbNotificationBox.NotificationBoxType = NotificationBoxType.Danger;
            nbNotificationBox.Visible = true;
        }

        /// <summary>
        /// Shows the success.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowSuccess( string message )
        {
            if ( nbNotificationBox.Visible )
            {
                return;
            }

            nbNotificationBox.Text = message;
            nbNotificationBox.NotificationBoxType = NotificationBoxType.Success;
            nbNotificationBox.Visible = true;
        }

        /// <summary>
        /// Binds the show all controls.
        /// </summary>
        private void BindShowAllControls()
        {
            if ( _doShowAll )
            {
                lSummary.Visible = false;
                lbShowAll.Visible = false;
                return;
            }

            var totalItemCount = GetEntitySetItemCount();
            var displayedItemCount = rEntitySetItems.Items.Count;
            var remainingItems = totalItemCount - displayedItemCount;

            if ( remainingItems < 1 )
            {
                lSummary.Visible = false;
                lbShowAll.Visible = false;
                return;
            }

            lbShowAll.Visible = true;
            lSummary.Text = string.Format( "... and {0} more.", remainingItems );
        }

        /// <summary>
        /// Binds the workflow type picker.
        /// </summary>
        private void BindWorkflowTypeControls()
        {
            if ( _hasLaunched )
            {
                wtpWorkflowType.Visible = false;
                ddlWorkflowType.Visible = false;
                lWorkflowType.Visible = false;
                return;
            }

            // If a page parameter is set, then it overrides everything else
            var workflowTypeId = PageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();

            if ( workflowTypeId.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeId.Value );

                if ( workflowType != null && workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    lWorkflowType.Text = GetLockedWorkflowTypeHtml( workflowType );
                    lWorkflowType.Visible = true;
                    wtpWorkflowType.Visible = false;
                    ddlWorkflowType.Visible = false;
                    return;
                }
            }

            // If no valid page parameter, then use the block settings
            var workflowTypes = GetAttributeValues( AttributeKey.WorkflowTypes )
                .Select( WorkflowTypeCache.Get )
                .ToList();

            wtpWorkflowType.Visible = workflowTypes.Count == 0;
            lWorkflowType.Visible = workflowTypes.Count == 1;
            ddlWorkflowType.Visible = workflowTypes.Count >= 2;

            if ( workflowTypes.Count == 1 )
            {
                var workflowType = workflowTypes.Single();
                lWorkflowType.Text = GetLockedWorkflowTypeHtml( workflowType );
            }
            else if ( workflowTypes.Any() )
            {
                ddlWorkflowType.DataSource = workflowTypes;
                ddlWorkflowType.DataBind();
            }
        }

        /// <summary>
        /// Gets the locked workflow type HTML.
        /// </summary>
        /// <param name="workflowTypeCache">The workflow type cache.</param>
        /// <returns></returns>
        private string GetLockedWorkflowTypeHtml( WorkflowTypeCache workflowTypeCache )
        {
            return string.Format( "<strong>Workflow Type</strong><br />{0}", workflowTypeCache.Name );
        }

        /// <summary>
        /// Binds the repeater.
        /// </summary>
        private void BindRepeater()
        {
            var entitySetId = GetEntitySetId();
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            var entityQuery = GetEntityQuery();
            var entityTypeCache = GetEntityTypeCache();

            if ( entityQuery == null || entityTypeCache == null )
            {
                rEntitySetItems.DataSource = null;
                rEntitySetItems.DataBind();

                ShowError( "The entity set is not valid" );
                return;
            }

            // Generate a view model for each entity that contains HTML to show the user what the entities are in the most
            // human friendly way possible
            var theType = entityTypeCache.GetEntityType();
            IEnumerable<RepeaterViewModel> viewModels;

            const string twoLineTemplate = "{0}<br /><sup>({1})</sup>";
            const string nameAndIdTemplate = "{0} Id: {1}";

            /*
             * 2020-02-13 BJW
             *
             * This giant if statement is written this way so as to make the conditional choice for how to render each entity outside
             * of a loop. It would probably be easier to read if we put the conditionals inside the loop, but then those conditionals
             * would be evaluated for every entity. Since this block has potential to display thousands of entities, I thought it best
             * to try to optimize this way.
             */
            var hasName = theType.GetProperty( "Name" ) != null;
            var hasTitle = theType.GetProperty( "Title" ) != null;

            if ( entityTypeCache.Id == EntityTypeCache.Get<Person>().Id || entityTypeCache.Id == EntityTypeCache.Get<Group>().Id )
            {
                // Person or Group have a good ToString override that works well here
                viewModels = entityQuery.ToList().Select( e => new RepeaterViewModel
                {
                    Html = e.ToString()
                } );
            }
            else if ( entityTypeCache.Id == EntityTypeCache.Get<GroupMember>().Id )
            {
                // Group members should show both the group and the person ToString results
                viewModels = entityQuery.Include( "Person" ).Include( "Group" ).ToList().Select( e => new RepeaterViewModel
                {
                    Html = string.Format( twoLineTemplate, ( ( GroupMember ) e ).Person, ( ( GroupMember ) e ).Group )
                } );
            }
            else if ( entityTypeCache.Id == EntityTypeCache.Get<ConnectionRequest>().Id )
            {
                // Connection requests should show both the opportunity and the person ToString results. ConnectionOpportunity has a good ToString method
                viewModels = entityQuery.Include( "PersonAlias.Person" ).Include( "ConnectionOpportunity" ).ToList().Select( e => new RepeaterViewModel
                {
                    Html = string.Format( twoLineTemplate, ( ( ConnectionRequest ) e ).PersonAlias.Person, ( ( ConnectionRequest ) e ).ConnectionOpportunity )
                } );
            }
            else if ( hasName || hasTitle )
            {
                // In order of preference, use the Name, Title or Entity Type/Id properties.
                viewModels = entityQuery.ToList().Select( e => new RepeaterViewModel
                {
                    Html = ( ( hasName ? e.GetPropertyValue( "Name" ) : null )
                           ?? ( hasTitle ? e.GetPropertyValue( "Title" ) : null )
                           ?? string.Format( nameAndIdTemplate, entityTypeCache.FriendlyName, e.Id ) ).ToStringSafe()
                } );
            }
            else if ( theType.GetProperty( "Person" ) != null && theType.GetProperty( "Person" ).GetCustomAttribute( typeof( NotMappedAttribute ) ) == null )
            {
                // If there is a Person property then use the person's name with the entity id underneath
                viewModels = entityQuery.Include( "Person" ).ToList().Select( e => new RepeaterViewModel
                {
                    Html = string.Format( twoLineTemplate,
                        e.GetPropertyValue( "Person" ),
                        string.Format( nameAndIdTemplate, entityTypeCache.FriendlyName, e.Id ) )
                } );
            }
            else if ( theType.GetProperty( "PersonAlias" ) != null && theType.GetProperty( "PersonAlias" ).GetCustomAttribute( typeof( NotMappedAttribute ) ) == null )
            {
                // If there is a PersonAlias property then use the person's name with the entity id underneath
                viewModels = entityQuery.Include( "PersonAlias.Person" ).ToList().Select( e => new RepeaterViewModel
                {
                    Html = string.Format( twoLineTemplate,
                        ( ( PersonAlias ) e.GetPropertyValue( "PersonAlias" ) ).Person,
                        string.Format( nameAndIdTemplate, entityTypeCache.FriendlyName, e.Id ) )
                } );
            }
            else
            {
                // There are no configured properties on this entity type, so just so the entity type name and entity id
                viewModels = entityQuery.ToList().Select( e => new RepeaterViewModel
                {
                    Html = string.Format( nameAndIdTemplate, entityTypeCache.FriendlyName, e.Id )
                } );
            }

            rEntitySetItems.DataSource = viewModels;
            rEntitySetItems.DataBind();
        }

        /// <summary>
        /// Binds the name of the entity type.
        /// </summary>
        private void BindEntityTypeName()
        {
            var entityTypeCache = GetEntityTypeCache();
            lEntityTypeName.Text = entityTypeCache == null ?
                string.Empty :
                string.Format( "<strong>{0}</strong>", entityTypeCache.FriendlyName.Pluralize() );
        }

        #endregion Methods

        #region Data Helpers

        /// <summary>
        /// Gets the entity set identifier.
        /// </summary>
        /// <returns></returns>
        private int GetEntitySetId()
        {
            return PageParameter( PageParameterKey.EntitySetId ).AsInteger();
        }

        /// <summary>
        /// Gets the entity set.
        /// </summary>
        /// <returns></returns>
        private EntitySet GetEntitySet()
        {
            if ( _entitySet == null )
            {
                var entitySetId = GetEntitySetId();

                var rockContext = new RockContext();
                var entitySetService = new EntitySetService( rockContext );
                _entitySet = entitySetService.Queryable().AsNoTracking().FirstOrDefault( es => es.Id == entitySetId );
            }

            return _entitySet;
        }
        private EntitySet _entitySet = null;

        /// <summary>
        /// Gets the entity type cache.
        /// </summary>
        /// <returns></returns>
        private EntityTypeCache GetEntityTypeCache()
        {
            var entitySet = GetEntitySet();
            return entitySet == null ? null : EntityTypeCache.Get( entitySet.EntityTypeId ?? 0 );
        }

        /// <summary>
        /// Gets the entity ids query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<IEntity> GetEntityQuery()
        {
            var entitySetId = GetEntitySetId();

            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            var query = entitySetService.GetEntityQuery( entitySetId );

            if ( query == null )
            {
                return null;
            }

            query = query.AsNoTracking();

            if ( !_doShowAll )
            {
                var limit = GetAttributeValue( AttributeKey.DefaultNumberOfItemsToShow ).AsIntegerOrNull() ??
                    AttributeDefault.DefaultNumberOfItemsToShow;

                query = query.Take( limit );
            }

            return query;
        }

        /// <summary>
        /// Gets the entity set item count.
        /// </summary>
        /// <returns></returns>
        private int GetEntitySetItemCount()
        {
            var countFromViewState = ViewState[ViewStateKey.EntitySetItemCount].ToStringSafe().AsIntegerOrNull();

            if ( countFromViewState.HasValue )
            {
                return countFromViewState.Value;
            }

            var entitySetId = GetEntitySetId();
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            var count = entitySetService.GetEntityQuery( entitySetId ).Count();

            ViewState[ViewStateKey.EntitySetItemCount] = count;
            return count;
        }

        #endregion Data Helpers

        #region ViewModels

        /// <summary>
        /// View Model for the Repeater Items
        /// </summary>
        internal class RepeaterViewModel
        {
            /// <summary>
            /// Gets or sets the HTML.
            /// </summary>
            /// <value>
            /// The HTML.
            /// </value>
            public string Html { get; set; }
        }

        #endregion ViewModels
    }
}