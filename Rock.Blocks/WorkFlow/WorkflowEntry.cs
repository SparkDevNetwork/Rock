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

using Rock.Attribute;
using Rock.ClientService.Core.Note;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.WorkflowEntry;
using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Enums.Workflow;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Transactions;
using Rock.ViewModels.Blocks.WorkFlow.WorkflowEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Workflow;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Allows for filling out workflows from a mobile application.
    /// </summary>

    [DisplayName( "Workflow Entry" )]
    [Category( "Worfklow" )]
    [Description( "Used to enter information for a workflow that has interactive elements." )]
    [IconCssClass( "fa fa-gears" )]
    [ConfigurationChangedReload( BlockReloadMode.Block )]
    [SupportedSiteTypes( SiteType.Web, SiteType.Mobile )]

    #region Block Attributes

    // Shared Attributes

    [WorkflowTypeField( "Workflow Type",
        Description = "The type of workflow to launch when viewing this.",
        IsRequired = false,
        Key = AttributeKey.WorkflowType,
        Order = 0 )]

    [BooleanField(
        "Show Summary View",
        Description = "If workflow has been completed, should the summary view be displayed?",
        Key = AttributeKey.ShowSummaryView,
        SiteTypes = SiteTypeFlags.Web,
        Order = 1 )]

    [CodeEditorField(
        "Block Title Template",
        Description = "Lava template for determining the title of the block. If not specified, the name of the Workflow Type will be shown.",
        Key = AttributeKey.BlockTitleTemplate,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        SiteTypes = SiteTypeFlags.Web,
        Order = 2 )]

    [TextField(
        "Block Title Icon CSS Class",
        Description = "The CSS class for the icon displayed in the block title. If not specified, the icon for the Workflow Type will be shown.",
        Key = AttributeKey.BlockTitleIconCssClass,
        IsRequired = false,
        SiteTypes = SiteTypeFlags.Web,
        Order = 3 )]

    [BooleanField(
        "Disable Passing WorkflowId",
        Description = "If disabled, prevents the use of a Workflow Id (WorkflowId=) from being passed in and only accepts a WorkflowGuid.",
        Key = AttributeKey.DisablePassingWorkflowId,
        DefaultBooleanValue = false,
        Order = 4 )]

    [BooleanField(
        "Disable Passing WorkflowTypeId",
        Description = "If set, it prevents the use of a Workflow Type Id (WorkflowTypeId=) from being passed in and only accepts a WorkflowTypeGuid. " +
            "To use this block setting on your external site, you will need to create a new page and add the Workflow Entry block to it. " +
            "You may also add a new route so that URLs are in the pattern .../{PageRoute}/{WorkflowTypeGuid}. " +
            "If your workflow uses a form, you will also need to adjust email content to ensure that your URLs are correct.",
        Key = AttributeKey.DisablePassingWorkflowTypeId,
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField(
        "Log Interaction when Form is Viewed",
        Key = AttributeKey.LogInteractionOnView,
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField(
        "Log Interaction when Form is Completed",
        Key = AttributeKey.LogInteractionOnCompletion,
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField(
        "Disable Captcha Support",
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        SiteTypes = SiteTypeFlags.Web,
        Key = AttributeKey.DisableCaptchaSupport,
        Order = 8 )]

    [CustomDropdownListField( "Completion Action",
        description: "What action to perform when there is nothing left for the user to do.",
        listSource: "0^Show Message from Workflow,1^Show Completion Xaml,2^Redirect to Page",
        IsRequired = true,
        DefaultValue = "0",
        SiteTypes = SiteTypeFlags.Mobile,
        Key = AttributeKey.CompletionAction,
        Order = 9 )]

    [CodeEditorField( "Completion Xaml",
        Description = "The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "",
        SiteTypes = SiteTypeFlags.Mobile,
        Key = AttributeKey.CompletionXaml,
        Order = 10 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        SiteTypes = SiteTypeFlags.Mobile,
        Key = AttributeKey.EnabledLavaCommands,
        Order = 11 )]

    [LinkedPage( "Redirect To Page",
        Description = "The page the user will be redirected to if the Completion Action is set to Redirect to Page.",
        IsRequired = false,
        DefaultValue = "",
        SiteTypes = SiteTypeFlags.Mobile,
        Key = AttributeKey.RedirectToPage,
        Order = 12 )]

    [CustomDropdownListField( "Scan Mode",
        description: "",
        listSource: "0^Off,1^Automatic",
        IsRequired = false,
        DefaultValue = "0",
        SiteTypes = SiteTypeFlags.Mobile,
        Key = AttributeKey.ScanMode,
        Order = 13 )]

    [TextField( "Scan Attribute",
        Description = "",
        IsRequired = false,
        DefaultValue = "",
        SiteTypes = SiteTypeFlags.Mobile,
        Key = AttributeKey.ScanAttribute,
        Order = 14 )]

    [BooleanField(
        "Enable for Form Sharing",
        Description = "When enabled and Workflow Type is blank, the Form Builder will be able to generate a shareable link to this page so the chosen form can be filled out using this block instance.",
        DefaultBooleanValue = false,
        Key = AttributeKey.EnableForFormSharing,
        Order = 15 )]

    [BooleanField(
        "Use Form Name for Page Title",
        Description = "When enabled, the page title will be overridden with the name of the form associated with this workflow.",
        DefaultBooleanValue = false,
        Key = AttributeKey.UseFormNameForPageTitle,
        Order = 16 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "02D2DBA8-5300-4367-B15B-E37DFB3F7D1E" )]
    [Rock.SystemGuid.BlockTypeGuid( SystemGuid.BlockType.OBSIDIAN_WORKFLOW_ENTRY )]
    public class WorkflowEntry : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        /// <summary>
        /// Features supported by both server and client. This is required by
        /// the legacy mobile support.
        /// </summary>
        private static class FeatureKey
        {
            /// <summary>
            /// Client values (i.e. values converted from Rock Database to Client Native)
            /// are supported.
            /// </summary>
            public const string ClientValues = "clientValues";
        }

        /// <summary>
        /// The block setting attribute keys for the MobileWorkflowEntry block.
        /// </summary>
        private static class AttributeKey
        {
            // Shared attribute keys.
            public const string WorkflowType = "WorkflowType";
            public const string DisablePassingWorkflowId = "DisablePassingWorkflowId";
            public const string DisablePassingWorkflowTypeId = "DisablePassingWorkflowTypeId";
            public const string LogInteractionOnView = "LogInteractionOnView";
            public const string LogInteractionOnCompletion = "LogInteractionOnCompletion";
            public const string EnableForFormSharing = "EnableForFormSharing";
            public const string UseFormNameForPageTitle = "UseFormNameForPageTitle";

            // Web only attribute keys.
            public const string ShowSummaryView = "ShowSummaryView";
            public const string BlockTitleTemplate = "BlockTitleTemplate";
            public const string BlockTitleIconCssClass = "BlockTitleIconCssClass";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";

            // Mobile only attribute keys.
            public const string CompletionAction = "CompletionAction";
            public const string CompletionXaml = "CompletionXaml";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string RedirectToPage = "RedirectToPage";
            public const string ScanMode = "ScanMode";
            public const string ScanAttribute = "ScanAttribute";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string WorkflowId = "WorkflowId";
            public const string WorkflowGuid = "WorkflowGuid";
            public const string WorkflowName = "WorkflowName";

            public const string ActionId = "ActionId";

            /// <summary>
            /// "WorkflowType" supports integer IDs, unique IDs, ID keys, and slugs.
            /// It is used to load the workflow type associated with the workflow.
            /// </summary>
            public const string WorkflowType = "WorkflowType";
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string WorkflowTypeGuid = "WorkflowTypeGuid";
            public const string WorkflowTypeSlug = "WorkflowTypeSlug";

            public const string Command = "Command";

            public const string GroupId = "GroupId";
            public const string PersonId = "PersonId";
            public const string InteractionStartDateTime = "InteractionStartDateTime";

            // NOTE that the actual parameter for CampusId and CampusGuid is just 'Campus', but making them different internally to make it clearer
            public const string CampusId = "Campus";
            public const string CampusGuid = "Campus";
        }

        #endregion

        #region Block Attributes

        /// <summary>
        /// Gets the scan mode.
        /// </summary>
        /// <value>
        /// The scan mode.
        /// </value>
        protected int ScanMode => GetAttributeValue( AttributeKey.ScanMode ).AsInteger();

        /// <summary>
        /// Gets the scan attribute.
        /// </summary>
        /// <value>
        /// The scan attribute.
        /// </value>
        protected string ScanAttribute => GetAttributeValue( AttributeKey.ScanAttribute );

        /// <summary>
        /// Gets the workflow type unique identifier block setting.
        /// </summary>
        /// <value>
        /// The workflow type unique identifier block setting.
        /// </value>
        protected Guid? WorkflowTypeGuid => GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull();

        private bool UseFormNameForPageTitle => GetAttributeValue( AttributeKey.UseFormNameForPageTitle ).AsBoolean();

        #endregion Block Attributes

        #region Page Parameters

        private string WorkflowTypePageParameter => PageParameter( PageParameterKey.WorkflowType );

        private int? WorkflowTypeIdPageParameter =>
            PageParameter( PageParameterKey.WorkflowType ).AsIntegerOrNull()
            ?? PageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();

        private Guid? WorkflowTypeGuidPageParameter =>
            PageParameter( PageParameterKey.WorkflowType ).AsGuidOrNull()
            ?? PageParameter( PageParameterKey.WorkflowTypeGuid ).AsGuidOrNull();

        private string WorkflowTypeSlugPageParameter => PageParameter( PageParameterKey.WorkflowTypeSlug );

        #endregion Page Parameters

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                ScanAttribute = ScanMode == 1 && ScanAttribute.IsNotNullOrWhiteSpace() ? ScanAttribute : null
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var workflowId = PageParameter( PageParameterKey.WorkflowId ).AsIntegerOrNull();
            var workflowGuid = PageParameter( PageParameterKey.WorkflowGuid ).AsGuidOrNull();
            var workflow = LoadWorkflow( workflowId, workflowGuid, out var errorMessage );

            if ( workflow == null )
            {
                return new WorkflowEntryOptionsBag
                {
                    InitialAction = CreateErrorMessage( null, null, errorMessage )
                };
            }

            if ( CheckAndProcessLoginRequired( workflow ) )
            {
                return null;
            }

            // If the workflow type was not configured by block setting
            // or if the block is configured to always use the form name for the page title and the workflow type uses a form,
            // then update the page title to match the workflow type name.
            if ( !GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull().HasValue
                 || ( this.UseFormNameForPageTitle && workflow.WorkflowTypeCache.IsFormBuilder ) )
            {
                this.RequestContext.Response.SetPageTitle( workflow.WorkflowTypeCache.Name );
            }

            var actionId = RequestContext.GetPageParameter( PageParameterKey.ActionId ).AsIntegerOrNull();
            var initialAction = ProcessWorkflow( workflow, actionId, null, null, null );

            return new WorkflowEntryOptionsBag
            {
                IsCaptchaEnabled = !GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean(),
                InitialAction = initialAction
            };
        }

        private WorkflowTypeCache LoadWorkflowTypeCache()
        {
            var workflowTypeGuidBlockSetting = this.WorkflowTypeGuid;
            var workflowTypeIdPageParam = this.WorkflowTypeIdPageParameter;
            var workflowTypeGuidPageParam = this.WorkflowTypeGuidPageParameter;
            var workflowTypeKeyOrSlugPageParam = this.WorkflowTypePageParameter;
            var workflowTypeSlugPageParam = this.WorkflowTypeSlugPageParameter;
            var allowPassingWorkflowTypeId = !GetAttributeValue( AttributeKey.DisablePassingWorkflowTypeId ).AsBoolean();

            if ( workflowTypeGuidBlockSetting.HasValue )
            {
                return WorkflowTypeCache.Get( workflowTypeGuidBlockSetting.Value, this.RockContext );
            }
            else if ( workflowTypeGuidPageParam.HasValue )
            {
                return WorkflowTypeCache.Get( workflowTypeGuidPageParam.Value, this.RockContext );
            }
            else if ( workflowTypeIdPageParam.HasValue && allowPassingWorkflowTypeId )
            {
                return WorkflowTypeCache.Get( workflowTypeIdPageParam.Value, this.RockContext );
            }
            else if ( workflowTypeSlugPageParam.IsNotNullOrWhiteSpace() )
            {
                return WorkflowTypeCache.GetBySlug( workflowTypeSlugPageParam );
            }
            else if ( workflowTypeKeyOrSlugPageParam.IsNotNullOrWhiteSpace() )
            {
                return WorkflowTypeCache.Get( workflowTypeKeyOrSlugPageParam, allowPassingWorkflowTypeId )
                    // Try loading it from a slug.
                    ?? WorkflowTypeCache.GetBySlug( workflowTypeKeyOrSlugPageParam );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the workflow. This will either load an existing workflow or
        /// attempt to activate a new one.
        /// </summary>
        /// <param name="workflowId">The existing workflow identifier to load.</param>
        /// <param name="workflowGuid">The existing workflow unique identifier to load.</param>
        /// <returns>An instance of <see cref="Model.Workflow"/> or <c>null</c>.</returns>
        private Model.Workflow LoadWorkflow( int? workflowId, Guid? workflowGuid, out InteractiveMessageBag errorMessage )
        {
            var workflowType = LoadWorkflowTypeCache();

            if ( workflowType == null )
            {
                return ValidateWorkflow( null, null, out errorMessage );
            }
            else if ( workflowGuid.HasValue )
            {
                var workflow = new WorkflowService( RockContext )
                    .Queryable()
                    .FirstOrDefault( w => w.WorkflowTypeId == workflowType.Id && w.Guid == workflowGuid.Value );

                // Load all attributes for this workflow and activities.
                workflow?.LoadAttributes( RockContext );
                workflow?.Activities.LoadAttributes( RockContext );

                return ValidateWorkflow( workflow, workflowType, out errorMessage );
            }
            else if ( workflowId.HasValue && workflowId.Value != 0 && !GetAttributeValue( AttributeKey.DisablePassingWorkflowId ).AsBoolean() )
            {
                var workflow = new WorkflowService( RockContext )
                    .Queryable()
                    .FirstOrDefault( w => w.WorkflowTypeId == workflowType.Id && w.Id == workflowId.Value );

                // Load all attributes for this workflow and activities.
                workflow?.LoadAttributes( RockContext );
                workflow?.Activities.LoadAttributes( RockContext );

                return ValidateWorkflow( workflow, workflowType, out errorMessage );
            }
            else
            {
                var workflowName = RequestContext.GetPageParameter( PageParameterKey.WorkflowName );

                if ( workflowName.IsNullOrWhiteSpace() )
                {
                    workflowName = $"New {workflowType.Name}";
                }

                // We don't load attributes because the Activate method will
                // do that anyway.
                var workflow = Model.Workflow.Activate( workflowType, workflowName );

                if ( workflow == null )
                {
                    errorMessage = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Error,
                        Title = "Workflow Activation Error",
                        Content = "Workflow could not be activated."
                    };

                    return null;
                }

                return ValidateWorkflow( workflow, workflowType, out errorMessage );
            }
        }

        /// <summary>
        /// Validate the workflow and type and populate an error message if
        /// something is not correct.
        /// </summary>
        /// <param name="workflow">The workflow to be processed.</param>
        /// <param name="workflowType">The workflow type to be processed.</param>
        /// <param name="errorMessage">On return may contain an error message to display.</param>
        /// <returns>The workflow to be processed or <c>null</c> if <paramref name="errorMessage"/> should be displayed.</returns>
        private Model.Workflow ValidateWorkflow( Model.Workflow workflow, WorkflowTypeCache workflowType, out InteractiveMessageBag errorMessage )
        {
            if ( workflowType == null )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Error,
                    Title = "Configuration Error",
                    Content = "Workflow type was not configured or specified correctly."
                };

                return null;
            }
            else if ( !workflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Title = "Sorry",
                    Content = "You are not authorized to view this typ eof workflow."
                };

                return null;
            }
            else if ( workflowType.IsActive != true )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Title = "Sorry",
                    Content = "This type of workflow is not active."
                };

                return null;
            }
            else if ( workflowType.FormStartDateTime.HasValue && workflowType.FormStartDateTime.Value > RockDateTime.Now )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Title = "Sorry",
                    Content = "This type of workflow is not active."
                };

                return null;
            }
            else if ( workflowType.FormEndDateTime.HasValue && workflowType.FormEndDateTime.Value < RockDateTime.Now )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Title = "Sorry",
                    Content = "This type of workflow is not active."
                };

                return null;
            }
            else if ( workflowType.FormBuilderTemplate != null && !workflowType.FormBuilderTemplate.IsActive )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Title = "Sorry",
                    Content = "This type of workflow is not active."
                };

                return null;
            }
            else if ( workflow == null )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Title = "Sorry",
                    Content = "This was not a valid workflow."
                };

                return null;
            }
            else
            {
                errorMessage = null;

                return workflow;
            }
        }

        /// <summary>
        /// Sets the initial workflow attributes.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="fields">The fields.</param>
        private void SetInitialWorkflowAttributes( Model.Workflow workflow, Dictionary<string, string> fields )
        {
            // Set initial values from the page parameters.
            foreach ( var pageParameter in RequestContext.PageParameters )
            {
                workflow.SetAttributeValue( pageParameter.Key, pageParameter.Value );
            }

            // Set/Update initial values from what the shell sent us.
            if ( fields != null )
            {
                foreach ( var field in fields )
                {
                    workflow.SetAttributeValue( field.Key, field.Value );
                }
            }
        }

        /// <summary>
        /// Gets the initial entity object to use when a new workflow is
        /// launched. This pulls data from the page parameters to find the
        /// entity to use.
        /// </summary>
        /// <returns>An instance of <see cref="IEntity"/> if one is available; otherwise <c>null</c>.</returns>
        private IEntity GetInitialWorkflowEntity()
        {
            var personId = RequestContext.GetPageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            var groupId = RequestContext.GetPageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();

            if ( personId.HasValue )
            {
                return new PersonService( RockContext ).Get( personId.Value );
            }
            else if ( groupId.HasValue )
            {
                return new GroupService( RockContext ).Get( groupId.Value );
            }

            return null;
        }

        /// <summary>
        /// Process the workflow repeatedly until ready to return the details
        /// to the client for display.
        /// </summary>
        /// <param name="workflow">The workflow to be processed.</param>
        /// <param name="actionId">If not <c>null</c> then this will be used to ensure that it is the only action that can be returned.</param>
        /// <returns>An instance of <see cref="InteractiveActionBag"/> that represents what data to display.</returns>
        private InteractiveActionBag ProcessWorkflow( Model.Workflow workflow, int? actionId, DateTimeOffset? actionStartDateTime, Guid? actionTypeGuid, Dictionary<string, string> componentData )
        {
            InteractiveActionResult actionResult = null;
            Guid? lastActionTypeGuid = null;
            WorkflowAction lastAction = null;
            IEntity entity = null;

            if ( workflow.Id == 0 )
            {
                entity = GetInitialWorkflowEntity();
            }

            while ( actionResult == null || actionResult.ProcessingType != InteractiveActionContinueMode.Stop )
            {
                var action = ProcessAndGetNextAction( workflow, entity, actionId, out var errorMessage );

                // Either an error happened or the workflow completed.
                if ( action == null )
                {
                    if ( errorMessage == null )
                    {
                        WriteInteraction( false, workflow, null, RockDateTime.Now );
                    }

                    return GetEndOfWorkflowBag( workflow, actionTypeGuid, actionResult, errorMessage );
                }

                // If this action is the same as the last, that likely means the
                // component is broken. For example if said "Continue" with a
                // unsuccessful result.
                if ( action.Guid == lastAction?.Guid )
                {
                    return CreateErrorMessage( workflow, workflow.WorkflowTypeCache, "Invalid action", "We detected an invalid action state that prevents further processing." );
                }

                // Check if we have a previous interactive action result that
                // allowed us to continue but only up to another interactive
                // action is hit.
                if ( actionResult != null && actionResult.ProcessingType == InteractiveActionContinueMode.ContinueWhileUnattended )
                {
                    WriteInteraction( false, workflow, action.ActionTypeId, RockDateTime.Now );

                    return CreateInteractiveActionBag( workflow, lastActionTypeGuid, actionResult );
                }

                var interactiveAction = ( IInteractiveAction ) action.ActionTypeCache.WorkflowAction;

                if ( actionTypeGuid.HasValue && actionTypeGuid.Value == action.ActionTypeCache.Guid && componentData != null )
                {
                    // This is ever so slightly unsafe since they could be giving
                    // us data for a different action of the same type, but we
                    // just have to trust a bit since the workflow might not be
                    // persisted. Thus we can't do anything more specific.
                    actionResult = ProcessCurrentAction( workflow, action, actionStartDateTime, actionTypeGuid.Value, componentData );
                }
                else
                {
                    actionResult = interactiveAction.StartAction( action, RockContext, RequestContext );
                }

                actionId = null;
                actionTypeGuid = null;
                componentData = null;

                // If the action was successful then mark the action and activity
                // as completed if we need to.
                if ( actionResult.IsSuccess )
                {
                    // Interactive actions must always be completed after
                    // returning successfully.
                    if ( !action.CompletedDateTime.HasValue )
                    {
                        action.MarkComplete();
                    }

                    if ( action.ActionTypeCache.IsActivityCompletedOnSuccess && !action.Activity.CompletedDateTime.HasValue )
                    {
                        action.Activity.MarkComplete();
                    }
                }

                lastAction = action;
                lastActionTypeGuid = action.ActionTypeCache.Guid;
            }

            if ( workflow.IsPersisted )
            {
                new WorkflowService( RockContext ).PersistImmediately( lastAction );
            }
            else if ( lastActionTypeGuid.HasValue && actionTypeGuid.HasValue && lastActionTypeGuid != actionTypeGuid )
            {
                // If we have multiple interactive actions then we must persist.
                new WorkflowService( RockContext ).PersistImmediately( lastAction );
            }

            WriteInteraction( false, workflow, lastAction?.ActionTypeId, RockDateTime.Now );

            return CreateInteractiveActionBag( workflow, lastActionTypeGuid, actionResult );
        }

        /// <summary>
        /// Processes the current workflow action from the provided component
        /// data. This will be called when submitting component data back to
        /// the server.
        /// </summary>
        /// <param name="workflow">The workflow instance being processed.</param>
        /// <param name="action">The action to be processed.</param>
        /// <param name="actionStartDateTime">The date and time that the action was started (displayed).</param>
        /// <param name="actionTypeGuid">The unique identifier of the action type the component data came from.</param>
        /// <param name="componentData">The data provided by the UI component.</param>
        /// <returns>The result from the component, or an error object.</returns>
        private InteractiveActionResult ProcessCurrentAction( Model.Workflow workflow, WorkflowAction action, DateTimeOffset? actionStartDateTime, Guid actionTypeGuid, Dictionary<string, string> componentData )
        {
            if ( action != null && action.ActionTypeCache.Guid == actionTypeGuid )
            {
                WriteInteraction( true, workflow, action.ActionTypeId, actionStartDateTime?.ToOrganizationDateTime() );

                var interactiveAction = ( IInteractiveAction ) action.ActionTypeCache.WorkflowAction;
                var result = interactiveAction.UpdateAction( action, componentData, RockContext, RequestContext );

                if ( workflow.IsPersisted )
                {
                    new WorkflowService( RockContext ).PersistImmediately( action );
                }

                return result;
            }
            else
            {
                return new InteractiveActionResult
                {
                    IsSuccess = false,
                    ProcessingType = InteractiveActionContinueMode.Stop,
                    ActionData = new InteractiveActionDataBag
                    {
                        Message = new InteractiveMessageBag
                        {
                            Type = InteractiveMessageType.Error,
                            Title = "Invalid state",
                            Content = "The workflow was not in a valid state to process the data."
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Gets the action details that represent what to display at the end
        /// of the workflow processing.
        /// </summary>
        /// <param name="workflow">That workflow that is being processed.</param>
        /// <param name="lastActionTypeGuid">The unique identifier of the last interactive action type that was executed.</param>
        /// <param name="lastActionResult">The result object of the last interactive action that was executed.</param>
        /// <param name="errorMessage">The error message that might have been generated during processing.</param>
        /// <returns>The data that describes the workflow UI to display.</returns>
        private InteractiveActionBag GetEndOfWorkflowBag( Model.Workflow workflow, Guid? lastActionTypeGuid, InteractiveActionResult lastActionResult, InteractiveMessageBag errorMessage )
        {
            // If the block is specifically configured to show the summary
            // view after the workflow has finished processing then ignore
            // what the workflow might have provided.
            if ( !workflow.IsActive && GetAttributeValue( AttributeKey.ShowSummaryView ).AsBoolean() && workflow.WorkflowTypeCache.SummaryViewText.IsNotNullOrWhiteSpace() )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add( "Action", null );
                mergeFields.Add( "Activity", null );
                mergeFields.Add( "Workflow", workflow );

                return CreateInteractiveActionBag( workflow, null, new InteractiveActionResult
                {
                    ActionData = new InteractiveActionDataBag
                    {
                        Message = new InteractiveMessageBag
                        {
                            Type = InteractiveMessageType.Html,
                            Content = workflow.WorkflowTypeCache.SummaryViewText.ResolveMergeFields( mergeFields )
                        }
                    }
                } );
            }

            // Check if we have a previous interactive action result
            // that allowed us to continue processing but still wants
            // to display its data on the page.
            if ( lastActionResult != null )
            {
                return CreateInteractiveActionBag( workflow, lastActionTypeGuid, lastActionResult );
            }

            // We either had an error, in which case message will be non-null
            // or there is no interactive action to display.
            return CreateInteractiveActionBag( workflow, null, new InteractiveActionResult
            {
                ActionData = new InteractiveActionDataBag
                {
                    Message = errorMessage ?? GetCompletionMessage( workflow, string.Empty )
                }
            } );
        }

        /// <summary>
        /// Processes the workflow and then gets the next action.
        /// </summary>
        /// <param name="workflow">The workflow to process.</param>
        /// <param name="entity">The entity to be passed to the workflow.</param>
        /// <param name="actionId">If not <c>null</c> then this will be used to ensure that it is the only action that can be returned.</param>
        /// <param name="errorMessage">The error message if workflow processing failed.</param>
        /// <returns>If not <c>null</c> contains the workflow action representing an interactive action to display.</returns>
        private WorkflowAction ProcessAndGetNextAction( Model.Workflow workflow, IEntity entity, int? actionId, out InteractiveMessageBag errorMessage )
        {
            var workflowService = new WorkflowService( RockContext );

            errorMessage = null;

            var processStatus = workflowService.Process( workflow, entity, out var errorMessages );

            if ( !processStatus )
            {
                errorMessage = new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Error,
                    Title = "Workflow Error",
                    Content = string.Join( "\n", errorMessages )
                };

                return null;
            }

            // Special case check for the workflow having been deleted already.
            if ( workflow.Status == "DeleteWorkflowNow" )
            {
                return null;
            }

            return workflow.GetNextInteractiveAction( RequestContext.CurrentPerson, actionId, BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the completion message to use based on the block settings.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="responseText">The response text from the last action.</param>
        /// <returns></returns>
        private InteractiveMessageBag GetCompletionMessage( Model.Workflow workflow, string responseText )
        {
            int completionAction = GetAttributeValue( AttributeKey.CompletionAction ).AsInteger();
            var xaml = GetAttributeValue( AttributeKey.CompletionXaml );
            var redirectToPage = GetAttributeValue( AttributeKey.RedirectToPage ).AsGuidOrNull();
            var enabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

            // Note: Completion action will always be 0 on web.

            if ( completionAction == 2 && redirectToPage.HasValue )
            {
                return new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Redirect,
                    Content = redirectToPage.ToString()
                };
            }
            else if ( completionAction == 1 && !string.IsNullOrWhiteSpace( xaml ) )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Workflow", workflow );

                return new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Xaml,
                    Content = xaml.ResolveMergeFields( mergeFields, null, enabledLavaCommands )
                };
            }
            else if ( responseText.IsNullOrWhiteSpace() )
            {
                var message = workflow.WorkflowTypeCache.NoActionMessage
                    ?? "The selected workflow is not in a state that requires you to enter information.";
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Workflow", workflow );

                return new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Warning,
                    Content = message.ResolveMergeFields( mergeFields, null, enabledLavaCommands )
                };
            }
            else
            {
                return new InteractiveMessageBag
                {
                    Type = InteractiveMessageType.Information,
                    Content = responseText
                };
            }
        }

        /// <summary>
        /// Gets the action details that instruct the client on how to display
        /// the next user interface screen.
        /// </summary>
        /// <param name="workflow">The workflow being processed.</param>
        /// <param name="lastActionTypeGuid">The last interactive action type that was processed.</param>
        /// <param name="lastActionResult">The result from the last interactive action that was processed.</param>
        /// <returns>An instance of <see cref="InteractiveActionBag"/> that describes the screen.</returns>
        private InteractiveActionBag CreateInteractiveActionBag( Model.Workflow workflow, Guid? lastActionTypeGuid, InteractiveActionResult lastActionResult )
        {
            var workflowType = workflow.WorkflowTypeCache;
            Guid? actionComponentGuid = null;
            string url = null;

            // Build up a URL that can be used on the web block. After the first
            // submission it will update the browser URL to show this value so
            // there is an easy copy and paste pattern.
            if ( workflow != null && workflow.Id != 0 )
            {
                var pageParams = RequestContext.GetPageParameters();

                if ( !GetAttributeValue( AttributeKey.DisablePassingWorkflowId ).AsBoolean() )
                {
                    pageParams.TryAdd( PageParameterKey.WorkflowId, workflow.Id.ToString() );
                }

                pageParams.TryAdd( PageParameterKey.WorkflowGuid, workflow.Guid.ToString() );

                // Remove the PageId parameter that will show up if we are
                // not using a named route.
                pageParams.Remove( "PageId" );

                url = this.GetCurrentPageUrl( pageParams );
            }

            if ( lastActionTypeGuid.HasValue )
            {
                actionComponentGuid = WorkflowActionTypeCache.Get( lastActionTypeGuid.Value, RockContext )?.EntityType?.Guid;
            }

            var actionBag = new InteractiveActionBag
            {
                Title = GetBlockTitle( workflowType ),
                IconCssClass = GetBlockTitleIconCssClass( workflowType ),
                CreatedDateTime = workflow.CreatedDateTime?.ToRockDateTimeOffset(),
                PrefixedId = workflow.WorkflowId,
                WorkflowGuid = workflow.IsPersisted ? ( Guid? ) workflow.Guid : null,
                Url = url,
                ActionStartDateTime = RockDateTime.Now.ToRockDateTimeOffset(),
                ActionTypeGuid = lastActionTypeGuid,
                ActionComponentGuid = actionComponentGuid,
                ActionData = lastActionResult.ActionData
            };

            if ( workflowType.NoActionMessage.IsNotNullOrWhiteSpace() )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add( "Action", null );
                mergeFields.Add( "Activity", null );
                mergeFields.Add( "Workflow", workflow );

                actionBag.NoActionMessage = workflowType.NoActionMessage.ResolveMergeFields( mergeFields );
            }
            else
            {
                actionBag.NoActionMessage = "The selected workflow is not in a state that requires you to enter information.";
            }

            if ( lastActionResult.IsNotesVisible && workflow != null && workflow.Id != 0 )
            {
                var entityType = EntityTypeCache.Get<Model.Workflow>( true, RockContext );
                var noteTypes = NoteTypeCache.GetByEntity( entityType.Id, string.Empty, string.Empty )
                    .OrderBy( nt => nt.Order )
                    .ThenBy( nt => nt.Name )
                    .ToList();
                var noteTypeIds = noteTypes.Select( nt => nt.Id ).ToList();

                var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson )
                {
                    AllowedNoteTypes = NoteTypeCache.GetByEntity( entityType.Id, string.Empty, string.Empty, true )
                };

                var noteCollection = noteClientService.GetViewableNotes( workflow );
                var notes = noteClientService.OrderNotes( noteCollection, true ).ToList();
                var watchedNoteIds = noteClientService.GetWatchedNoteIds( notes );

                notes.LoadAttributes( RockContext );

                actionBag.IsNotesVisible = true;
                actionBag.NoteTypes = noteTypes
                    .Select( nt => noteClientService.GetNoteTypeBag( nt ) )
                    .ToList();
                actionBag.Notes = notes
                    .Select( n => noteClientService.GetNoteBag( n, watchedNoteIds ) )
                    .ToList();
            }

            return actionBag;
        }

        /// <summary>
        /// Creates an error message response to be sent back to the client.
        /// </summary>
        /// <param name="workflow">The workflow that is being processed, may be <c>null</c>.</param>
        /// <param name="workflowType">The worklow type that is being processed, may be <c>null</c>.</param>
        /// <param name="message">The message to display.</param>
        /// <returns>An instance of <see cref="InteractiveActionBag"/> that contains the error message.</returns>
        private InteractiveActionBag CreateErrorMessage( Model.Workflow workflow, WorkflowTypeCache workflowType, InteractiveMessageBag message )
        {
            return new InteractiveActionBag
            {
                Title = GetBlockTitle( workflowType ),
                IconCssClass = GetBlockTitleIconCssClass( workflowType ),
                CreatedDateTime = workflow?.CreatedDateTime?.ToRockDateTimeOffset(),
                ActionStartDateTime = RockDateTime.Now.ToRockDateTimeOffset(),
                PrefixedId = workflow?.WorkflowId,
                ActionData = new InteractiveActionDataBag
                {
                    Message = message
                }
            };
        }

        /// <summary>
        /// Creates an error message response to be sent back to the client.
        /// </summary>
        /// <param name="workflowType">The worklow type that is being processed, may be <c>null</c>.</param>
        /// <param name="title">The title of the error.</param>
        /// <param name="content">The details of the error.</param>
        /// <returns>An instance of <see cref="InteractiveActionBag"/> that contains the error message.</returns>
        private InteractiveActionBag CreateErrorMessage( Model.Workflow workflow, WorkflowTypeCache workflowType, string title, string content )
        {
            return CreateErrorMessage( workflow, workflowType, new InteractiveMessageBag
            {
                Type = InteractiveMessageType.Error,
                Title = title,
                Content = content
            } );
        }

        /// <summary>
        /// Gets the title of the block that will be displayed in the panel header.
        /// </summary>
        /// <param name="workflowType">The type of workflow that is being processed.</param>
        /// <returns>A string of test representing the block title.</returns>
        private string GetBlockTitle( WorkflowTypeCache workflowType )
        {
            // If the block title is specified by a configuration setting, use it.
            var blockTitle = GetAttributeValue( AttributeKey.BlockTitleTemplate );

            if ( blockTitle.IsNotNullOrWhiteSpace() )
            {
                // Resolve the block title using the specified Lava template.
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "WorkflowType", workflowType );
                mergeFields.Add( "Item", workflowType );

                blockTitle = blockTitle.ResolveMergeFields( mergeFields );
            }

            // If the block title is not configured, use the Workflow Type if it is available.
            if ( string.IsNullOrWhiteSpace( blockTitle ) )
            {
                blockTitle = workflowType != null
                    ? $"{workflowType.WorkTerm} Entry"
                    : "Workflow Entry";
            }

            return blockTitle;
        }

        /// <summary>
        /// Gets the icon CSS class of the block that will be displayed in the
        /// panel header.
        /// </summary>
        /// <param name="workflowType">The type of workflow that is being processed.</param>
        /// <returns>A string of test representing the block icon CSS class.</returns>
        private string GetBlockTitleIconCssClass( WorkflowTypeCache workflowTypeCache )
        {
            var iconCssClass = GetAttributeValue( AttributeKey.BlockTitleIconCssClass );

            if ( string.IsNullOrWhiteSpace( iconCssClass ) )
            {
                iconCssClass = workflowTypeCache?.IconCssClass;
            }

            return iconCssClass ?? "fa fa-gear";
        }

        /// <summary>
        /// Writes either the form shown or form completed interaction for the
        /// workflow.
        /// </summary>
        /// <param name="formCompleted">If <c>true</c> then the form completed interaction will be written.</param>
        /// <param name="workflow">The workflow that is being processed.</param>
        /// <param name="workflowActionTypeId">The identifier of the action type that is being processed.</param>
        /// <param name="interactionStartDateTime">The date and time when the form was originally shown.</param>
        private void WriteInteraction( bool formCompleted, Model.Workflow workflow, int? workflowActionTypeId, DateTime? interactionStartDateTime )
        {
            if ( !formCompleted && !GetAttributeValue( AttributeKey.LogInteractionOnView ).AsBoolean() )
            {
                return;
            }

            if ( formCompleted && !GetAttributeValue( AttributeKey.LogInteractionOnCompletion ).AsBoolean() )
            {
                return;
            }

            var workflowLaunchInteractionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.WORKFLOW_LAUNCHES.AsGuid(), RockContext )?.Id;

            var interactionTransactionInfo = new InteractionTransactionInfo
            {
                // NOTE: InteractionTransactionInfo.PersonAliasId will do this
                // same logic if PersonAliasId isn't specified. Doing it here to
                // make it more obvious.
                PersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId ?? RequestContext.CurrentVisitorId,

                InteractionEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.WORKFLOW.AsGuid(), RockContext ).Id,
                InteractionDateTime = RockDateTime.Now,
                InteractionChannelId = workflowLaunchInteractionChannelId ?? 0,
                InteractionRelatedEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.WORKFLOW_ACTION_TYPE.AsGuid(), RockContext ).Id,
                InteractionRelatedEntityId = workflowActionTypeId,
                LogCrawlers = false
            };

            /* 7-30-2021 MDP

             If the workflow isn't persisted, the WorkflowId would be 0. If so, just leave the InteractionEntityId
             null. The InteractionData will still have WorkflowType and ActionType, which are the main things that will
             be needed when looking at WorkflowEntry Interactions. So, leaving InteractionEntityId null (workflow.Id)
             is OK.
             see https://app.asana.com/0/0/1200679813013532/f
            */
            if ( workflow.Id > 0 )
            {
                interactionTransactionInfo.InteractionEntityId = workflow.Id;
                interactionTransactionInfo.InteractionEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.WORKFLOW.AsGuid(), RockContext ).Id;
            }

            var workflowType = workflow.WorkflowTypeCache;

            if ( formCompleted )
            {
                interactionTransactionInfo.InteractionSummary = $"Completed a workflow of type: {workflowType?.Name}";
                interactionTransactionInfo.InteractionOperation = "Form Completed";

                if ( interactionStartDateTime.HasValue )
                {
                    interactionTransactionInfo.InteractionLength = ( RockDateTime.Now - interactionStartDateTime.Value ).TotalSeconds;
                }
            }
            else
            {
                interactionTransactionInfo.InteractionSummary = $"Launched a workflow of type: {workflowType?.Name}";
                interactionTransactionInfo.InteractionOperation = "Form Viewed";
            }

            // there is only one Channel for Workflow Entry (Rock.SystemGuid.InteractionChannel.WORKFLOW_LAUNCHES)
            // so there isn't a channel entity
            IEntity channelEntity = null;

            var componentEntity = workflowType;

            var interactionTransaction = new InteractionTransaction(
                DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS.AsGuid() ),
                channelEntity,
                componentEntity,
                interactionTransactionInfo );

            interactionTransaction.Enqueue();
        }

        /// <summary>
        /// Determines if the captcha is valid. This takes into considering
        /// what site type the block is on to know if captcha is even required.
        /// </summary>
        /// <returns><c>true</c> if the captcha is valid or not required; otherwise <c>false</c>.</returns>
        private bool IsCaptchaValid()
        {
            // Captcha is only valid on web.
            if ( SiteCache.Get( PageCache.SiteId, RockContext )?.SiteType != SiteType.Web )
            {
                return true;
            }

            // Admin doesn't want to use captcha on the site.
            if ( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() )
            {
                return true;
            }

            return RequestContext.IsCaptchaValid;
        }

        /// <summary>
        /// Checks if this workflow type requires the person to be logged in. If
        /// so and the person is not logged in then they are redirected to the
        /// login page.
        /// </summary>
        /// <param name="workflow">The workflow being processed.</param>
        /// <returns><c>true</c> if a redirect was issued; otherwise <c>false</c>.</returns>
        private bool CheckAndProcessLoginRequired( Model.Workflow workflow )
        {
            var workflowType = workflow.WorkflowTypeCache;
            var isLoginRequired = workflowType.FormBuilderTemplate?.IsLoginRequired
                ?? workflowType.IsLoginRequired;

            if ( isLoginRequired && RequestContext.CurrentPerson == null )
            {
                PageCache.Layout.Site.RedirectToLoginPage( true );
                return true;
            }

            return false;
        }

        #endregion

        #region Legacy Mobile Methods

        /// <summary>
        /// Gets the person entry details to be sent to the shell.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="currentPersonId">The current person identifier.</param>
        /// <param name="mergeFields">The merge fields to use for Lava parsing.</param>
        /// <returns>The object that will be included in the response that details the person entry part of the form.</returns>
        private static WorkflowFormPersonEntry GetLegacyMobilePersonEntryDetails( RockContext rockContext, WorkflowAction action, int? currentPersonId, IDictionary<string, object> mergeFields )
        {
            var form = action.ActionTypeCache.WorkflowForm;

            if ( form == null || !form.AllowPersonEntry )
            {
                return null;
            }

            if ( form.PersonEntryHideIfCurrentPersonKnown && currentPersonId.HasValue )
            {
                return null;
            }

            var mobileSite = Rock.Mobile.MobileHelper.GetCurrentApplicationSite( true, rockContext );

            action.GetPersonEntryPeople( rockContext, currentPersonId, out var personEntryPerson, out var personEntrySpouse );

            var mobilePerson = personEntryPerson != null ? Rock.Mobile.MobileHelper.GetMobilePerson( personEntryPerson, mobileSite ) : null;
            var mobileSpouse = personEntrySpouse != null ? Rock.Mobile.MobileHelper.GetMobilePerson( personEntrySpouse, mobileSite ) : null;

            // Get the default address if it is supposed to show.
            MobileAddress mobileAddress = null;
            var promptForAddress = ( form.PersonEntryAddressEntryOption != WorkflowActionFormPersonEntryOption.Hidden ) && form.PersonEntryGroupLocationTypeValueId.HasValue;
            if ( promptForAddress && ( personEntryPerson?.PrimaryFamilyId ).HasValue )
            {
                var personEntryGroupLocationTypeValueId = form.PersonEntryGroupLocationTypeValueId.Value;

                var familyLocation = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.GroupId == personEntryPerson.PrimaryFamilyId.Value && a.GroupLocationTypeValueId == form.PersonEntryGroupLocationTypeValueId )
                    .Select( a => a.Location )
                    .FirstOrDefault();

                mobileAddress = familyLocation != null ? Rock.Mobile.MobileHelper.GetMobileAddress( familyLocation ) : null;
            }

            Guid? maritalStatusGuid;

            if ( personEntryPerson != null )
            {
                maritalStatusGuid = personEntryPerson.MaritalStatusValue?.Guid;
            }
            else
            {
                // default to Married if this is a new person
                maritalStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
            }

            return new WorkflowFormPersonEntry
            {
                PreHtml = form.PersonEntryPreHtml.ResolveMergeFields( mergeFields ),
                PostHtml = form.PersonEntryPostHtml.ResolveMergeFields( mergeFields ),
                CampusIsVisible = form.PersonEntryCampusIsVisible,
                SpouseEntryOption = GetLegacyMobileVisibility( form.PersonEntrySpouseEntryOption ),
                GenderEntryOption = GetLegacyMobileVisibility( form.PersonEntryGenderEntryOption ),
                EmailEntryOption = GetLegacyMobileVisibility( form.PersonEntryEmailEntryOption ),
                MobilePhoneEntryOption = GetLegacyMobileVisibility( form.PersonEntryMobilePhoneEntryOption ),
                BirthdateEntryOption = GetLegacyMobileVisibility( form.PersonEntryBirthdateEntryOption ),
                AddressEntryOption = form.PersonEntryGroupLocationTypeValueId.HasValue ? GetLegacyMobileVisibility( form.PersonEntryAddressEntryOption ) : VisibilityTriState.Hidden,
                MaritalStatusEntryOption = GetLegacyMobileVisibility( form.PersonEntryMaritalStatusEntryOption ),
                SpouseLabel = form.PersonEntrySpouseLabel,
                Values = new WorkflowFormPersonEntryValues
                {
                    Person = mobilePerson,
                    Spouse = mobileSpouse,
                    Address = mobileAddress,
                    MaritalStatusGuid = maritalStatusGuid
                }
            };
        }

        /// <summary>
        /// Converts the <see cref="WorkflowActionFormPersonEntryOption"/> value
        /// into one understood by the mobile shell.
        /// </summary>
        /// <param name="option">The visibility option.</param>
        /// <returns>The <see cref="VisibilityTriState"/> value that shows if the value should be hidden, optional or required.</returns>
        private static VisibilityTriState GetLegacyMobileVisibility( WorkflowActionFormPersonEntryOption option )
        {
            switch ( option )
            {
                case WorkflowActionFormPersonEntryOption.Optional:
                    return VisibilityTriState.Optional;

                case WorkflowActionFormPersonEntryOption.Required:
                    return VisibilityTriState.Required;

                case WorkflowActionFormPersonEntryOption.Hidden:
                default:
                    return VisibilityTriState.Hidden;
            }
        }

        /// <summary>
        /// Gets the data to be sent to a legacy mobile shell in order to display
        /// the entry form.
        /// </summary>
        /// <param name="action">The action that represents the entry form to display.</param>
        /// <param name="workflow">The workflow the action belongs to.</param>
        /// <param name="currentPerson">The current person that the form will be displayed to.</param>
        /// <param name="useClientValues"><c>true</c> if the newer client values format should be used.</param>
        /// <returns>An instance of <see cref="WorkflowForm"/> that represents the mobile form to display.</returns>
        private WorkflowForm GetLegacyMobileWorkflowForm( WorkflowAction action, Model.Workflow workflow, Person currentPerson, bool useClientValues )
        {
            var activity = action.Activity;
            var form = action.ActionTypeCache.WorkflowForm;

            // Prepare the merge fields for the HTML content.
            var mergeFields = RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            var mobileForm = new WorkflowForm
            {
                WorkflowGuid = workflow.Id != 0 ? ( Guid? ) workflow.Guid : null,
                HeaderHtml = form.Header.ResolveMergeFields( mergeFields ),
                FooterHtml = form.Footer.ResolveMergeFields( mergeFields ),
                PersonEntry = GetLegacyMobilePersonEntryDetails( RockContext, action, RequestContext.CurrentPerson?.Id, mergeFields )
            };

            // Populate all the form fields that should be visible on the workflow.
            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );
                    string value = attribute.DefaultValue;

                    // Get the current value from either the workflow or the activity.
                    if ( workflow.AttributeValues.ContainsKey( attribute.Key ) && workflow.AttributeValues[attribute.Key] != null )
                    {
                        value = workflow.AttributeValues[attribute.Key].Value;
                    }
                    else if ( activity.AttributeValues.ContainsKey( attribute.Key ) && activity.AttributeValues[attribute.Key] != null )
                    {
                        value = activity.AttributeValues[attribute.Key].Value;
                    }

                    var mobileField = new MobileField
                    {
                        AttributeGuid = attribute.Guid,
                        Key = attribute.Key,
                        Title = attribute.Name,
                        IsRequired = formAttribute.IsRequired,
                        ConfigurationValues = useClientValues
                            ? attribute.FieldType.Field?.GetPublicConfigurationValues( attribute.ConfigurationValues, Field.ConfigurationValueUsage.Edit, null )
                            : attribute.QualifierValues.ToDictionary( v => v.Key, v => v.Value.Value ),
                        FieldTypeGuid = attribute.FieldType.Guid,
#pragma warning disable CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        RockFieldType = attribute.FieldType.Class,
#pragma warning restore CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        Value = useClientValues
                            ? attribute.FieldType.Field?.GetPublicEditValue( value, attribute.ConfigurationValues )
                            : value
                    };

                    MobileHelper.UpdateLegacyFieldValuesForClient( mobileField, attribute, RequestContext );

                    if ( formAttribute.IsReadOnly )
                    {
                        var field = attribute.FieldType.Field;

                        string formattedValue = null;

                        // get formatted value 
                        if ( attribute.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName )
                        {
                            formattedValue = field.FormatValueAsHtml( null, attribute.EntityTypeId, activity.Id, value, attribute.QualifierValues, true );
                        }
                        else
                        {
                            formattedValue = field.FormatValueAsHtml( null, attribute.EntityTypeId, activity.Id, value, attribute.QualifierValues );
                        }

                        mobileField.Value = formattedValue;
                        mobileField.FieldTypeGuid = null;
#pragma warning disable CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        mobileField.RockFieldType = string.Empty;
#pragma warning restore CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support

                        if ( formAttribute.HideLabel )
                        {
                            mobileField.Title = string.Empty;
                        }
                    }

                    mobileForm.Fields.Add( mobileField );
                }
            }

            mobileForm.Buttons = form.GetFormActionButtons( RockContext )
                .Select( b => new WorkflowFormButton
                {
                    Text = b.Text,
                    Type = b.Value
                } )
                .ToList();

            return mobileForm;
        }

        /// <summary>
        /// Converts a <see cref="MobilePerson"/> object from the legacy block
        /// action to the new <see cref="PersonBasicEditorBag"/> that is used
        /// by <see cref="WorkflowPersonEntryProcessor"/> helper class.
        /// </summary>
        /// <param name="person">The <see cref="MobilePerson"/> object to be converted.</param>
        /// <returns>A new instance of <see cref="PersonBasicEditorBag"/> that represents <paramref name="person"/>.</returns>
        private PersonBasicEditorBag ConvertLegacyMobileToPersonBasicEditorBag( MobilePerson person )
        {
            if ( person == null )
            {
                return null;
            }

            return new PersonBasicEditorBag
            {
                FirstName = person.FirstName,
                NickName = person.NickName,
                LastName = person.LastName,
                Email = person.Email,
                MobilePhoneCountryCode = string.Empty,
                MobilePhoneNumber = person.MobilePhone,
                PersonGender = person.Gender.ToNative(),
                PersonBirthDate = person.BirthDate.HasValue
                    ? new DatePartsPickerValueBag
                    {
                        Day = person.BirthDate.Value.DateTime.Day,
                        Month = person.BirthDate.Value.DateTime.Month,
                        Year = person.BirthDate.Value.DateTime.Year
                    }
                    : null
            };
        }

        /// <summary>
        /// Converts a <see cref="WorkflowFormPersonEntryValues"/> object from the legacy block
        /// action to the new <see cref="PersonEntryValuesBag"/> that is used
        /// by <see cref="WorkflowPersonEntryProcessor"/> helper class.
        /// </summary>
        /// <param name="values">The <see cref="MobilePerson"/> object to be converted.</param>
        /// <returns>A new instance of <see cref="PersonEntryValuesBag"/> that represents <paramref name="values"/>.</returns>
        private PersonEntryValuesBag ConvertLegacyMobilePersonEntryValuesBag( WorkflowFormPersonEntryValues values )
        {
            return new PersonEntryValuesBag
            {
                Address = new AddressControlBag
                {
                    Street1 = values.Address.Street1,
                    City = values.Address.City,
                    PostalCode = values.Address.PostalCode,
                    Country = values.Address.Country,
                    State = values.Address.State
                },
                CampusGuid = values.Person.CampusGuid,
                MaritalStatusGuid = values.MaritalStatusGuid,
                Person = ConvertLegacyMobileToPersonBasicEditorBag( values.Person ),
                Spouse = ConvertLegacyMobileToPersonBasicEditorBag( values.Spouse )
            };
        }

        /// <summary>
        /// Converts a <see cref="InteractiveMessageBag"/> to the legacy mobile
        /// object <see cref="WorkflowFormMessage"/>.
        /// </summary>
        /// <param name="message">The message to be converted.</param>
        /// <returns>An instance of <see cref="WorkflowFormMessage"/> that represents <paramref name="message"/>.</returns>
        private WorkflowFormMessage ConvertToLegacyMobileWorkflowFormMessage( InteractiveMessageBag message )
        {
            return new WorkflowFormMessage
            {
                Type = ( WorkflowFormMessageType ) ( int ) message.Type,
                Title = message.Title,
                Content = message.Content
            };
        }

        /// <summary>
        /// Converts the list of <see cref="MobileField"/> objects to a standard
        /// dictionary of attribute identifiers and values.
        /// </summary>
        /// <param name="mobileFields">The mobile fields to convert.</param>
        /// <returns>A dictionary that represents <paramref name="mobileFields"/>.</returns>
        private Dictionary<Guid, string> ConvertFromLegacyMobileFieldList( List<MobileField> mobileFields )
        {
            var values = new Dictionary<Guid, string>();

            foreach ( var mobileField in mobileFields )
            {
                if ( !mobileField.AttributeGuid.HasValue )
                {
                    continue;
                }

                var attribute = AttributeCache.Get( mobileField.AttributeGuid.Value, RockContext );

                if ( attribute != null )
                {
                    MobileHelper.UpdateLegacyFieldValuesFromClient( mobileField, attribute, RequestContext );
                }

                values.AddOrReplace( attribute.Guid, mobileField.Value );

            }

            return values;
        }

        #endregion

        #region IBreadCrumbBlock

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var result = new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
            };

            if ( GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull().HasValue )
            {
                return result;
            }

            var workflowType = LoadWorkflowTypeCache();

            if ( workflowType != null )
            {
                result.BreadCrumbs.Add( new BreadCrumbLink( workflowType.Name, pageReference ) );
            }

            return result;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Executes a workflow until it either ends or hits an interactive
        /// action that needs to be displayed.
        /// </summary>
        /// <param name="workflowGuid">The unique identifier of the workflow to be processed.</param>
        /// <param name="actionTypeGuid">The unique identifier of the component that <paramref name="componentData"/> belongs to.</param>
        /// <param name="componentData">The custom data from the UI component that will be processed.</param>
        /// <returns>A response that indicates what to display next.</returns>
        [BlockAction]
        public BlockActionResult GetNextInteractiveAction( Guid? workflowGuid = null, DateTimeOffset? actionStartDateTime = null, Guid? actionTypeGuid = null, Dictionary<string, string> componentData = null )
        {
            if ( !IsCaptchaValid() )
            {
                return ActionBadRequest( "Captcha was not valid." );
            }

            var workflow = LoadWorkflow( null, workflowGuid, out var errorMessage );

            if ( workflow == null )
            {
                return ActionOk( errorMessage );
            }

            // Set initial workflow attribute values if this is a new workflow.
            if ( !workflowGuid.HasValue )
            {
                // Mobile shell will send additional initial values in the component data.
                if ( PageCache.Layout.Site.SiteType == SiteType.Mobile )
                {
                    SetInitialWorkflowAttributes( workflow, componentData );
                }
                else
                {
                    SetInitialWorkflowAttributes( workflow, null );
                }
            }

            return ActionOk( ProcessWorkflow( workflow, null, actionStartDateTime, actionTypeGuid, componentData ) );
        }

        /// <summary>
        /// Begins the edit process of a note by returning the <see cref="NoteEditBag"/>
        /// that contains any information required to edit the requested note.
        /// </summary>
        /// <param name="idKey">The encrypted identifier of the note to edit.</param>
        /// <param name="workflowGuid">The unique identifier of the workflow.</param>
        /// <returns>An instance of <see cref="NoteEditBag"/> that provides the editable details of the note.</returns>
        [BlockAction]
        public BlockActionResult EditNote( string idKey, Guid workflowGuid )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var workflowId = new WorkflowService( RockContext ).GetId( workflowGuid );
            var note = new NoteService( RockContext ).Get( idKey, false );

            if ( !workflowId.HasValue )
            {
                return ActionBadRequest( "Invalid workflow." );
            }

            // Extra security check to ensure we are not giving access to other notes.
            if ( note == null || note.EntityId != workflowId )
            {
                return ActionBadRequest( "Note not found." );
            }

            var noteBag = noteClientService.EditNote( note, out var errorMessage );

            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Saves the changes made to a note.
        /// </summary>
        /// <param name="note">The request that describes the note and the changes that were made.</param>
        /// <param name="workflowGuid">The unique identifier of the workflow.</param>
        /// <returns>A new <see cref="NoteBag"/> instance that describes the note for display purposes.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( SaveNoteRequestBag note, Guid workflowGuid )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var contextEntity = new WorkflowService( RockContext ).Get( workflowGuid );

            if ( contextEntity == null )
            {
                return ActionBadRequest( "Invalid workflow." );
            }

            var noteBag = noteClientService.SaveNote( note, contextEntity, PageCache.Id, this.GetCurrentPageUrl(), RequestContext, out var errorMessage );

            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Sets the watched state of a specific note.
        /// </summary>
        /// <param name="idKey">The encrypted identifier of the note to watch or unwatch.</param>
        /// <param name="workflowGuid">The unique identifier of the workflow being processed.</param>
        /// <param name="isWatching"><c>true</c> if the note should be watched; otherwise <c>false</c>.</param>
        /// <returns>An empty 200-OK response if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult WatchNote( string idKey, Guid workflowGuid, bool isWatching )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var workflowId = new WorkflowService( RockContext ).GetId( workflowGuid );
            var note = new NoteService( RockContext ).Get( idKey, false );

            if ( !workflowId.HasValue )
            {
                return ActionBadRequest( "Invalid workflow." );
            }

            // Extra security check to ensure we are not giving access to other notes.
            if ( note == null || note.EntityId != workflowId )
            {
                return ActionBadRequest( "Note not found." );
            }

            if ( !noteClientService.WatchNote( note, isWatching, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        /// <summary>
        /// Deletes the requested note from the database.
        /// </summary>
        /// <param name="idKey">The encrypted identifier of the note to delete.</param>
        /// <param name="workflowGuid">The unique identifier of the workflow.</param>
        /// <returns>An empty 200-OK response if the note was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( string idKey, Guid workflowGuid )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var workflowId = new WorkflowService( RockContext ).GetId( workflowGuid );
            var note = new NoteService( RockContext ).Get( idKey, false );

            if ( !workflowId.HasValue )
            {
                return ActionBadRequest( "Invalid workflow." );
            }

            // Extra security check to ensure we are not giving access to other notes.
            if ( note == null || note.EntityId != workflowId )
            {
                return ActionBadRequest( "Note not found." );
            }

            if ( !noteClientService.DeleteNote( note, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        /// <summary>
        /// Gets the next form to display for the workflow. The form may be either
        /// an entry form or just the text or redirect that should be performed.
        /// </summary>
        /// <remarks>
        /// This is the implementation for legacy Mobile shell clients prior to
        /// version shell 1.7.0.3.
        /// </remarks>
        /// <param name="workflowGuid">The workflow unique identifier of the workflow being processed.</param>
        /// <param name="formAction">The form action button that was pressed.</param>
        /// <param name="formFields">The form field values.</param>
        /// <param name="personEntryValues">The person entry values.</param>
        /// <param name="supportedFeatures">The list of features that the client supports.</param>
        /// <returns>The data for the next form to be displayed.</returns>
        [BlockAction]
        public WorkflowForm GetNextForm( Guid? workflowGuid = null, string formAction = null, List<MobileField> formFields = null, WorkflowFormPersonEntryValues personEntryValues = null, List<string> supportedFeatures = null )
        {
            var workflowService = new WorkflowService( RockContext );

            var workflow = LoadWorkflow( null, workflowGuid, out var errorMessage );
            var currentPerson = RequestContext.CurrentPerson;

            if ( workflow == null )
            {
                return new WorkflowForm
                {
                    Message = new WorkflowFormMessage
                    {
                        Type = WorkflowFormMessageType.Error,
                        Content = "No Workflow Type has been set."
                    }
                };
            }

            // Set initial workflow attribute values.
            if ( !workflowGuid.HasValue )
            {
                var initialAttributeValues = new Dictionary<string, string>();

                if ( formFields != null )
                {
                    foreach ( var formField in formFields )
                    {
                        initialAttributeValues.TryAdd( formField.Key, formField.Value );
                    }
                }

                SetInitialWorkflowAttributes( workflow, initialAttributeValues );
            }

            var action = ProcessAndGetNextAction( workflow, null, null, out errorMessage );

            if ( action == null )
            {
                if ( errorMessage == null )
                {
                    WriteInteraction( false, workflow, null, RockDateTime.Now );
                }

                // We either had an error, in which case message will be non-null
                // or there is no interactive action to display.
                return new WorkflowForm
                {
                    Message = ConvertToLegacyMobileWorkflowFormMessage( errorMessage ?? GetCompletionMessage( workflow, string.Empty ) )
                };
            }

            // If this is a form submittal, then complete the form and re-process.
            if ( !string.IsNullOrEmpty( formAction ) && formFields != null )
            {
                if ( personEntryValues != null )
                {
                    using ( var personEntryRockContext = new RockContext() )
                    {
                        var processor = new WorkflowPersonEntryProcessor( action, personEntryRockContext );
                        processor.SetFormPersonEntryValues( RequestContext.CurrentPerson?.Id, ConvertLegacyMobilePersonEntryValuesBag( personEntryValues ) );
                    }
                }

                Rock.Workflow.Action.UserEntryForm.SetFormFieldValues( action, ConvertFromLegacyMobileFieldList( formFields ), RockContext );
                var actionData = Rock.Workflow.Action.UserEntryForm.CompleteEntryFormAction( action, formAction, RockContext, RequestContext );

                WriteInteraction( true, workflow, action.ActionTypeId, null );

                action = ProcessAndGetNextAction( workflow, null, null, out errorMessage );

                if ( action == null )
                {
                    if ( errorMessage == null )
                    {
                        WriteInteraction( false, workflow, null, RockDateTime.Now );
                    }

                    // We either had an error, in which case message will be non-null
                    // or there is no interactive action to display.
                    return new WorkflowForm
                    {
                        Message = ConvertToLegacyMobileWorkflowFormMessage( errorMessage ?? GetCompletionMessage( workflow, actionData?.Message?.Content ) )
                    };
                }

                // If there is a second interactive action, we need to persist.
                workflowService.PersistImmediately( action );
            }

            WriteInteraction( false, workflow, action.ActionTypeId, RockDateTime.Now );

            // Begin building up the response with the form data.
            var useClientValues = supportedFeatures?.Contains( FeatureKey.ClientValues ) ?? false;

            return GetLegacyMobileWorkflowForm( action, workflow, currentPerson, useClientValues );
        }

        #endregion
    }
}
