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
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.WorkflowEntry;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Allows for filling out workflows from a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Workflow Entry" )]
    [Category( "Mobile > Cms" )]
    [Description( "Allows for filling out workflows from a mobile application." )]
    [IconCssClass( "fa fa-gears" )]

    #region Block Attributes

    [WorkflowTypeField( "Workflow Type",
        Description = "The type of workflow to launch when viewing this.",
        IsRequired = true,
        Key = AttributeKeys.WorkflowType,
        Order = 0 )]

    [CustomDropdownListField( "Completion Action",
        description: "What action to perform when there is nothing left for the user to do.",
        listSource: "0^Show Message from Workflow,1^Show Completion Xaml,2^Redirect to Page",
        IsRequired = true,
        DefaultValue = "0",
        Key = AttributeKeys.CompletionAction,
        Order = 1 )]

    [CodeEditorField( "Completion Xaml",
        Description = "The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.CompletionXaml,
        Order = 2 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 3 )]

    [LinkedPage( "Redirect To Page",
        Description = "The page the user will be redirected to if the Completion Action is set to Redirect to Page.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.RedirectToPage,
        Order = 4 )]

    [CustomDropdownListField( "Scan Mode",
        description: "",
        listSource: "0^Off,1^Automatic",
        IsRequired = false,
        DefaultValue = "0",
        Key = AttributeKeys.ScanMode,
        Order = 5 )]

    [TextField( "Scan Attribute",
        Description = "",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.ScanAttribute,
        Order = 6 )]

    #endregion

    public class WorkflowEntry : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the MobileWorkflowEntry block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The completion action key
            /// </summary>
            public const string CompletionAction = "CompletionAction";

            /// <summary>
            /// The completion xaml key
            /// </summary>
            public const string CompletionXaml = "CompletionXaml";

            /// <summary>
            /// The enabled lava commands key
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";

            /// <summary>
            /// The redirect to page key
            /// </summary>
            public const string RedirectToPage = "RedirectToPage";

            /// <summary>
            /// The scan mode key
            /// </summary>
            public const string ScanMode = "ScanMode";

            /// <summary>
            /// The scan attribute key
            /// </summary>
            public const string ScanAttribute = "ScanAttribute";

            /// <summary>
            /// The workflow type key
            /// </summary>
            public const string WorkflowType = "WorkflowType";
        }

        /// <summary>
        /// Gets the scan mode.
        /// </summary>
        /// <value>
        /// The scan mode.
        /// </value>
        protected int ScanMode => GetAttributeValue( AttributeKeys.ScanMode ).AsInteger();

        /// <summary>
        /// Gets the scan attribute.
        /// </summary>
        /// <value>
        /// The scan attribute.
        /// </value>
        protected string ScanAttribute => GetAttributeValue( AttributeKeys.ScanAttribute );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.WorkflowEntry";

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

        /// <summary>
        /// Loads the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Model.Workflow LoadWorkflow( int? workflowId, RockContext rockContext )
        {
            if ( workflowId.HasValue )
            {
                return new WorkflowService( rockContext ).Get( workflowId.Value );
            }
            else
            {
                var workflowType = WorkflowTypeCache.Get( GetAttributeValue( AttributeKeys.WorkflowType ).AsGuid() );

                return Model.Workflow.Activate( workflowType, $"New {workflowType.Name}" );
            }
        }

        /// <summary>
        /// Sets the initial workflow attributes.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="fields">The fields.</param>
        private void SetInitialWorkflowAttributes( Model.Workflow workflow, List<MobileField> fields )
        {
            //
            // Set initial values from the page parameters.
            //
            foreach ( var pageParameter in RequestContext.PageParameters )
            {
                workflow.SetAttributeValue( pageParameter.Key, pageParameter.Value );
            }

            //
            // Set/Update initial values from what the shell sent us.
            //
            if ( fields != null )
            {
                foreach ( var field in fields )
                {
                    workflow.SetAttributeValue( field.Key, field.Value );
                }
            }
        }

        /// <summary>
        /// Gets the next action with a Form attached.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        private WorkflowAction GetNextAction( Model.Workflow workflow, Person currentPerson )
        {
            int personId = currentPerson?.Id ?? 0;
            bool canEdit = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson );

            //
            // Find all the activities that this person can see.
            //
            var activities = workflow.Activities
                .Where( a =>
                    a.IsActive &&
                    (
                        canEdit ||
                        ( !a.AssignedGroupId.HasValue && !a.AssignedPersonAliasId.HasValue ) ||
                        ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == personId ) ||
                        ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                    )
                )
                .OrderBy( a => a.ActivityTypeCache.Order )
                .ToList();

            //
            // Find the first action that the user is authorized to work with that has a Form
            // attached to it.
            //
            foreach ( var activity in activities )
            {
                if ( canEdit || activity.ActivityTypeCache.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    foreach ( var action in activity.ActiveActions )
                    {
                        if ( action.ActionTypeCache.WorkflowForm != null && action.IsCriteriaValid )
                        {
                            return action;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the form values.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="formFields">The form fields.</param>
        private void SetFormValues( WorkflowAction action, List<MobileField> formFields )
        {
            var activity = action.Activity;
            var workflow = activity.Workflow;
            var form = action.ActionTypeCache.WorkflowForm;

            var values = new Dictionary<int, string>();
            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible && !formAttribute.IsReadOnly )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );
                    var formField = formFields.FirstOrDefault( f => f.AttributeId == formAttribute.AttributeId );

                    if ( attribute != null && formField != null )
                    {
                        IHasAttributes item = null;

                        if ( attribute.EntityTypeId == workflow.TypeId )
                        {
                            item = workflow;
                        }
                        else if ( attribute.EntityTypeId == activity.TypeId )
                        {
                            item = activity;
                        }

                        if ( item != null )
                        {
                            item.SetAttributeValue( attribute.Key, formField.Value );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Completes the form action based on the action selected by the user.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="formAction">The form action.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private string CompleteFormAction( WorkflowAction action, string formAction, Person currentPerson, RockContext rockContext )
        {
            var workflowService = new WorkflowService( rockContext );
            var activity = action.Activity;
            var workflow = activity.Workflow;

            var mergeFields = RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            Guid activityTypeGuid = Guid.Empty;
            string responseText = "Your information has been submitted successfully.";

            //
            // Get the target activity type guid and response text from the
            // submitted form action.
            //
            foreach ( var act in action.ActionTypeCache.WorkflowForm.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var actionDetails = act.Split( new char[] { '^' } );
                if ( actionDetails.Length > 0 && actionDetails[0] == formAction )
                {
                    if ( actionDetails.Length > 2 )
                    {
                        activityTypeGuid = actionDetails[2].AsGuid();
                    }

                    if ( actionDetails.Length > 3 && !string.IsNullOrWhiteSpace( actionDetails[3] ) )
                    {
                        responseText = actionDetails[3].ResolveMergeFields( mergeFields );
                    }
                    break;
                }
            }

            action.MarkComplete();
            action.FormAction = formAction;
            action.AddLogEntry( "Form Action Selected: " + action.FormAction );

            if ( action.ActionTypeCache.IsActivityCompletedOnSuccess )
            {
                action.Activity.MarkComplete();
            }

            //
            // Set the attribute that should contain the submitted form action.
            //
            if ( action.ActionTypeCache.WorkflowForm.ActionAttributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( action.ActionTypeCache.WorkflowForm.ActionAttributeGuid.Value );
                if ( attribute != null )
                {
                    IHasAttributes item = null;

                    if ( attribute.EntityTypeId == workflow.TypeId )
                    {
                        item = workflow;
                    }
                    else if ( attribute.EntityTypeId == activity.TypeId )
                    {
                        item = activity;
                    }

                    if ( item != null )
                    {
                        item.SetAttributeValue( attribute.Key, formAction );
                    }
                }
            }

            //
            // Activate the requested activity if there was one.
            //
            if ( !activityTypeGuid.IsEmpty() )
            {
                var activityType = workflow.WorkflowTypeCache.ActivityTypes.Where( a => a.Guid.Equals( activityTypeGuid ) ).FirstOrDefault();
                if ( activityType != null )
                {
                    WorkflowActivity.Activate( activityType, workflow );
                }
            }

            return responseText;
        }

        /// <summary>
        /// Processes the workflow and then get next action.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private WorkflowAction ProcessAndGetNextAction( Model.Workflow workflow, Person currentPerson, RockContext rockContext, out WorkflowFormMessage message )
        {
            message = null;

            var processStatus = new WorkflowService( rockContext ).Process( workflow, null, out var errorMessages );
            if ( !processStatus )
            {
                message = new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Error,
                    Title = "Workflow Error",
                    Content = string.Join( "\n", errorMessages )
                };

                return null;
            }

            return GetNextAction( workflow, currentPerson );
        }

        /// <summary>
        /// Gets the completion message to use based on the block settings.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="responseText">The response text from the last action.</param>
        /// <returns></returns>
        private WorkflowFormMessage GetCompletionMessage( Model.Workflow workflow, string responseText )
        {
            int completionAction = GetAttributeValue( AttributeKeys.CompletionAction ).AsInteger();
            var xaml = GetAttributeValue( AttributeKeys.CompletionXaml );
            var redirectToPage = GetAttributeValue( AttributeKeys.RedirectToPage ).AsGuidOrNull();

            if ( completionAction == 2 && redirectToPage.HasValue )
            {
                return new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Redirect,
                    Content = redirectToPage.ToString()
                };
            }
            else if ( completionAction == 1 && !string.IsNullOrWhiteSpace( xaml ) )
            {
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Workflow", workflow );

                return new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Xaml,
                    Content = xaml.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) )
                };
            }
            else
            {
                if ( string.IsNullOrWhiteSpace( responseText ) )
                {
                    var message = workflow.WorkflowTypeCache.NoActionMessage;
                    var mergeFields = RequestContext.GetCommonMergeFields();

                    mergeFields.Add( "Workflow", workflow );

                    responseText = message.ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKeys.EnabledLavaCommands ) );
                }

                return new WorkflowFormMessage
                {
                    Type = WorkflowFormMessageType.Information,
                    Content = responseText
                };
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>A collection of string/string pairs.</returns>
        [BlockAction]
        public WorkflowForm GetNextForm( int? workflowId = null, string formAction = null, List<MobileField> formFields = null )
        {
            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            var workflow = LoadWorkflow( workflowId, rockContext );
            var currentPerson = GetCurrentPerson();

            //
            // Set initial workflow attribute values.
            //
            if ( !workflowId.HasValue )
            {
                SetInitialWorkflowAttributes( workflow, formFields );
            }

            var action = ProcessAndGetNextAction( workflow, currentPerson, rockContext, out var message );
            if ( action == null )
            {
                return new WorkflowForm
                {
                    Message = message ?? GetCompletionMessage( workflow, string.Empty )
                };
            }

            //
            // If this is a form submittal, then complete the form and re-process.
            //
            if ( !string.IsNullOrEmpty( formAction ) && formFields != null )
            {
                SetFormValues( action, formFields );
                var responseText = CompleteFormAction( action, formAction, currentPerson, rockContext );

                action = ProcessAndGetNextAction( workflow, currentPerson, rockContext, out message );
                if ( action == null )
                {
                    return new WorkflowForm
                    {
                        Message = message ?? GetCompletionMessage( workflow, responseText )
                    };
                }
            }

            //
            // Begin building up the response with the form data.
            //
            var activity = action.Activity;
            var form = action.ActionTypeCache.WorkflowForm;

            var mobileForm = new WorkflowForm
            {
                WorkflowId = workflow.Id
            };

            //
            // Populate all the form fields that should be visible on the workflow.
            //
            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );
                    string value = attribute.DefaultValue;

                    //
                    // Get the current value from either the workflow or the activity.
                    //
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
                        AttributeId = attribute.Id,
                        Key = attribute.Key,
                        Title = attribute.Name,
                        IsRequired = formAttribute.IsRequired,
                        ConfigurationValues = attribute.QualifierValues.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value ),
                        RockFieldType = attribute.FieldType.Class,
                        Value = value
                    };

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
                        mobileField.RockFieldType = string.Empty;

                        if ( formAttribute.HideLabel )
                        {
                            mobileField.Title = string.Empty;
                        }
                    }

                    mobileForm.Fields.Add( mobileField );
                }
            }

            //
            // Build the list of form actions (buttons) that should be presented
            // to the user.
            //
            foreach ( var btn in form.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var actionDetails = btn.Split( new char[] { '^' } );
                if ( actionDetails.Length > 0 )
                {
                    var btnType = DefinedValueCache.Get( actionDetails[1].AsGuid() );

                    if ( btnType != null )
                    {
                        mobileForm.Buttons.Add( new WorkflowFormButton
                        {
                            Text = actionDetails[0],
                            Type = btnType.Value
                        } );
                    }
                }
            }

            return mobileForm;
        }

        #endregion
    }
}
