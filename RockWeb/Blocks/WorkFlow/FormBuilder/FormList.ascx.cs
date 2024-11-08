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

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// List the existing forms for the selected category.
    /// </summary>
    [DisplayName( "Form List" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows the List of existing forms for the selected category." )]

    #region Rock Attributes

    [LinkedPage(
        "Submissions Page",
        Description = "The page that shows the submissions for this form.",
        Order = 0,
        Key = AttributeKeys.SubmissionsPage )]

    [LinkedPage(
        "Form Builder Page",
        Description = "The page that has the form builder editor.",
        Order = 1,
        Key = AttributeKeys.FormBuilderPage )]

    [LinkedPage(
        "Analytics Page",
        Description = " The page that shows the analytics for this form.",
        Order = 2,
        Key = AttributeKeys.AnalyticsPage )]

    #endregion Rock Attributes
    [Rock.SystemGuid.BlockTypeGuid( "B7C76420-9B34-422A-B161-87BDB45DD50C" )]
    public partial class FormList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Page Param Keys
        /// </summary>
        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string Tab = "tab";
        }

        public static class AttributeKeys
        {
            public const string AnalyticsPage = "AnalyticsPage";
            public const string FormBuilderPage = "FormBuilderPage";
            public const string SubmissionsPage = "SubmissionsPage";
        }

        #endregion

        #region User Preference Keys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKeys
        {
            public const string CategoryId = "CategoryId";
        }

        #endregion User Preference Keys

        #region Fields

        private bool _isAuthorizedToEdit = false;
        public const string CategoryNodePrefix = "C";

        #endregion

        /// <summary>
        /// The RestParams (used by the Markup)
        /// </summary>
        protected string RestParms;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                bool canEditBlock = IsUserAuthorized( Authorization.EDIT );

                // hide all the actions if user doesn't have EDIT to the block
                divTreeviewActions.Visible = canEditBlock;
                var cachedEntityType = EntityTypeCache.Get<WorkflowType>();
                if ( cachedEntityType != null )
                {
                    string parms = "?getCategorizedItems=false&lazyLoad=false";
                    parms += string.Format( "&entityTypeId={0}", cachedEntityType.Id );
                    RestParms = parms;

                    Category category = null;
                    var categoryId = GetCategoryId();
                    if ( categoryId.HasValue )
                    {
                        category = new CategoryService( new RockContext() ).Get( categoryId.Value );
                    }

                    if ( category == null )
                    {
                        category = FindFirstCategory( cachedEntityType.Id );
                    }

                    if ( category != null )
                    {
                        hfSelectedCategory.Value = category.Id.ToString();
                    }

                    List<string> parentIdList = new List<string>();
                    while ( category != null )
                    {
                        category = category.ParentCategory;
                        if ( category != null )
                        {
                            string categoryExpandedID = category.Id.ToString();
                            if ( !parentIdList.Contains( categoryExpandedID ) )
                            {
                                parentIdList.Insert( 0, categoryExpandedID );
                            }
                            else
                            {
                                // infinite recursion
                                break;
                            }
                        }
                    }

                    hfInitialCategoryParentIds.Value = parentIdList.AsDelimited( "," );
                    btnSecurity.EntityTypeId = EntityTypeCache.Get<Category>().Id;

                    ListCategoryForms();
                }
            }

            // handle custom postback events
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "category-selected" ) )
                    {
                        hfSelectedCategory.Value = nameValue[1];

                        var preferences = GetBlockPersonPreferences();
                        preferences.SetValue( UserPreferenceKeys.CategoryId, nameValue[1] );
                        preferences.Save();

                        ListCategoryForms();
                    }
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        #region Category

        /// <summary>
        /// Handles the Click event of the btnCategorySave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCategorySave_Click( object sender, EventArgs e )
        {
            Category category;

            var rockContext = new RockContext();
            CategoryService categoryService = new CategoryService( rockContext );

            int categoryId = hfCategoryId.ValueAsInt();

            if ( categoryId == 0 )
            {
                var entityTypeId = EntityTypeCache.GetId<WorkflowType>();
                category = new Category();
                category.IsSystem = false;
                category.EntityTypeId = entityTypeId.Value;
                category.Order = 0;
                categoryService.Add( category );
            }
            else
            {
                category = categoryService.Get( categoryId );
            }

            category.Name = tbCategoryName.Text;
            category.Description = tbCategoryDescription.Text;
            category.ParentCategoryId = hfParentCategory.Value.AsIntegerOrNull();
            category.IconCssClass = tbIconCssClass.Text;
            category.HighlightColor = cpHighlightColor.Text;

            if ( !Page.IsValid )
            {
                return;
            }

            // if the category IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of category didn't pass.
            // So, make sure a message is displayed in the validation summary
            cvCategory.IsValid = category.IsValid;

            if ( !cvCategory.IsValid )
            {
                cvCategory.ErrorMessage = category.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.CategoryId] = category.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEditCategory_Click( object sender, EventArgs e )
        {
            var service = new CategoryService( new RockContext() );
            var category = service.Get( hfSelectedCategory.ValueAsInt() );
            if ( category != null )
            {
                hfParentCategory.Value = category.ParentCategoryId.ToStringSafe();
                ShowCategoryDetails( category );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDeleteCategory_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CategoryService( rockContext );
            var category = service.Get( hfSelectedCategory.ValueAsInt() );

            if ( category != null )
            {
                if ( !category.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this category.", ModalAlertType.Information );
                    return;
                }

                service.Delete( category );

                rockContext.SaveChanges();

                NavigateToPage( RockPage.Guid, new Dictionary<string, string>() );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryChild_Click( object sender, EventArgs e )
        {
            hfParentCategory.Value = hfSelectedCategory.Value;
            var category = new Category();
            ShowCategoryDetails( category );
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryRoot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryRoot_Click( object sender, EventArgs e )
        {
            hfParentCategory.Value = string.Empty;
            var category = new Category();
            ShowCategoryDetails( category );
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the lbAddForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddForm_Click( object sender, EventArgs e )
        {
            lTitle.Text = "Create New Form";
            lDescription.Text = "Complete the fields below to setup your new form. Upon completion you'll be taken to the form designer.";
            divFormListTopPanel.Visible = false;
            pnlAddForm.Visible = true;
            pnlFormList.Visible = false;
            tbFormName.Text = string.Empty;
            tbDescription.Text = string.Empty;
            cpCategory.SetValue( hfSelectedCategory.ValueAsInt() );
            BindTemplateDropdown();
        }

        /// <summary>
        /// Handles the Click event of the btnStartBuilding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnStartBuilding_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var service = new WorkflowTypeService( rockContext );

            WorkflowType workflowType = new WorkflowType();

            var validationErrors = new List<string>();
            var formBuilderSettings = new Rock.Workflow.FormBuilder.FormSettings();
            formBuilderSettings.CompletionAction = new Rock.Workflow.FormBuilder.FormCompletionActionSettings
            {
                Message = "Your information has been submitted successfully.",
                Type = Rock.Workflow.FormBuilder.FormCompletionActionType.DisplayMessage
            };

            workflowType.IsActive = true;
            workflowType.Name = tbFormName.Text;
            workflowType.Description = tbDescription.Text;
            workflowType.CategoryId = cpCategory.SelectedValueAsInt();
            workflowType.FormBuilderTemplateId = ddlTemplate.SelectedValueAsInt();
            workflowType.ProcessingIntervalSeconds = 365 * 24 * 60 * 60;
            workflowType.IsPersisted = false;
            workflowType.LoggingLevel = WorkflowLoggingLevel.None;
            workflowType.IsFormBuilder = true;
            workflowType.WorkTerm = "Form";
            workflowType.IsSystem = false;
            workflowType.FormBuilderSettingsJson = formBuilderSettings.ToJson();
            if ( validationErrors.Any() )
            {
                nbValidationError.Text = string.Format(
                    "Please correct the following:<ul><li>{0}</li></ul>",
                    validationErrors.AsDelimited( "</li><li>" ) );
                nbValidationError.Visible = true;

                return;
            }

            if ( !workflowType.IsValid )
            {
                return;
            }

            service.Add( workflowType );
            rockContext.SaveChanges();

            // Create temporary state objects for the new workflow type
            var newAttributesState = new List<Rock.Model.Attribute>();

            // Dictionary to keep the attributes and activity types linked between the source and the target based on their guids
            var guidXref = new Dictionary<Guid, Guid>();

            var personAttribute = new Rock.Model.Attribute();
            personAttribute.Id = 0;
            personAttribute.Guid = Guid.NewGuid();
            personAttribute.IsSystem = false;
            personAttribute.Name = "Person";
            personAttribute.Key = "Person";
            personAttribute.IsGridColumn = true;
            personAttribute.Order = 0;
            personAttribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.PERSON ).Id;
            newAttributesState.Add( personAttribute );
            var personQualifier = new AttributeQualifier();
            personQualifier.Id = 0;
            personQualifier.Guid = Guid.NewGuid();
            personQualifier.IsSystem = false;
            personQualifier.Key = "EnableSelfSelection";
            personQualifier.Value = "False";
            personAttribute.AttributeQualifiers.Add( personQualifier );

            var spouseAttribute = new Rock.Model.Attribute();
            spouseAttribute.Id = 0;
            spouseAttribute.Guid = Guid.NewGuid();
            spouseAttribute.IsSystem = false;
            spouseAttribute.Name = "Spouse";
            spouseAttribute.Key = "Spouse";
            spouseAttribute.Order = 1;
            spouseAttribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.PERSON ).Id;
            newAttributesState.Add( spouseAttribute );
            var spouseQualifier = new AttributeQualifier();
            spouseQualifier.Id = 0;
            spouseQualifier.Guid = Guid.NewGuid();
            spouseQualifier.IsSystem = false;
            spouseQualifier.Key = "EnableSelfSelection";
            spouseQualifier.Value = "False";
            spouseAttribute.AttributeQualifiers.Add( spouseQualifier );

            var familyAttribute = new Rock.Model.Attribute();
            familyAttribute.Id = 0;
            familyAttribute.Guid = Guid.NewGuid();
            familyAttribute.IsSystem = false;
            familyAttribute.Name = "Family";
            familyAttribute.Key = "Family";
            familyAttribute.Order = 2;
            familyAttribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.GROUP ).Id;
            newAttributesState.Add( familyAttribute );

            // Save the workflow type attributes
            SaveAttributes( new Workflow().TypeId, "WorkflowTypeId", workflowType.Id.ToString(), newAttributesState, rockContext );

            var formBuilderEntityTypeId = EntityTypeCache.GetId<Rock.Workflow.Action.FormBuilder>();
            var workflowActivityType = new WorkflowActivityType();
            workflowType.ActivityTypes.Add( workflowActivityType );
            workflowActivityType.IsActive = true;
            workflowActivityType.Name = "Form Builder";
            workflowActivityType.Order = 0;
            workflowActivityType.IsActivatedWithWorkflow = true;
            var workflowActionType = new WorkflowActionType();
            workflowActivityType.ActionTypes.Add( workflowActionType );
            workflowActionType.WorkflowForm = new WorkflowActionForm();
            workflowActionType.WorkflowForm.PersonEntryPersonAttributeGuid = personAttribute.Guid;
            workflowActionType.WorkflowForm.PersonEntrySpouseAttributeGuid = spouseAttribute.Guid;
            workflowActionType.WorkflowForm.PersonEntryFamilyAttributeGuid = familyAttribute.Guid;
            workflowActionType.WorkflowForm.AllowPersonEntry = true;
            workflowActionType.WorkflowForm.PersonEntryRecordStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            workflowActionType.WorkflowForm.PersonEntryConnectionStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
            workflowActionType.WorkflowForm.PersonEntryGroupLocationTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            workflowActionType.WorkflowForm.Actions = "Submit^^^Your information has been submitted successfully.";

            var workFlowSection = new WorkflowActionFormSection();
            workflowActionType.WorkflowForm.FormSections.Add( workFlowSection );
            workFlowSection.Guid = Guid.NewGuid();
            workflowActivityType.Order = 0;

            var systemEmail = new SystemCommunicationService( new RockContext() ).Get( Rock.SystemGuid.SystemCommunication.WORKFLOW_FORM_NOTIFICATION.AsGuid() );
            if ( systemEmail != null )
            {
                workflowActionType.WorkflowForm.NotificationSystemCommunicationId = systemEmail.Id;
            }

            workflowActionType.EntityTypeId = formBuilderEntityTypeId.Value;
            workflowActionType.Name = "Form Builder";
            workflowActionType.Order = 0;
            rockContext.SaveChanges();

            if ( GetAttributeValue( AttributeKeys.FormBuilderPage ).IsNotNullOrWhiteSpace() )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams["WorkflowTypeId"] = workflowType.Id.ToString();
                NavigateToLinkedPage( AttributeKeys.FormBuilderPage, qryParams );
            }
            else
            {
                ListCategoryForms();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ListCategoryForms();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSortBy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSortBy_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindFormListRepeater( hfSelectedCategory.ValueAsInt() );
        }

        /// <summary>
        /// Handles when items are bound to the rForms repeater.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rForms_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lbBuilder = e.Item.FindControl( "lbBuilder" ) as LinkButton;
            var lbCommunications = e.Item.FindControl( "lbCommunications" ) as LinkButton;
            var lbSettings = e.Item.FindControl( "lbSettings" ) as LinkButton;
            var lbCopy = e.Item.FindControl( "lbCopy" ) as LinkButton;
            var lbDelete = e.Item.FindControl( "lbDelete" ) as LinkButton;

            if ( lbBuilder != null )
            {
                lbBuilder.Enabled = _isAuthorizedToEdit;
            }

            if ( lbCommunications != null )
            {
                lbCommunications.Enabled = _isAuthorizedToEdit;
            }

            if ( lbSettings != null )
            {
                lbSettings.Enabled = _isAuthorizedToEdit;
            }

            if ( lbCopy != null )
            {
                lbCopy.Visible = _isAuthorizedToEdit;
            }

            if ( lbDelete != null )
            {
                lbDelete.Visible = _isAuthorizedToEdit;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rForms control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rForms_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var workflowTypeId = e.CommandArgument.ToString().AsInteger();

            if ( e.CommandName == "Submissions" )
            {
                if ( GetAttributeValue( AttributeKeys.SubmissionsPage ).IsNotNullOrWhiteSpace() )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.WorkflowTypeId] = workflowTypeId.ToString();
                    NavigateToLinkedPage( AttributeKeys.SubmissionsPage, qryParams );
                }
            }
            else if ( e.CommandName == "Builder" )
            {
                if ( GetAttributeValue( AttributeKeys.FormBuilderPage ).IsNotNullOrWhiteSpace() )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.WorkflowTypeId] = workflowTypeId.ToString();
                    qryParams[PageParameterKey.Tab] = "FormBuilder";
                    NavigateToLinkedPage( AttributeKeys.FormBuilderPage, qryParams );
                }
            }
            else if ( e.CommandName == "Communications" )
            {
                if ( GetAttributeValue( AttributeKeys.FormBuilderPage ).IsNotNullOrWhiteSpace() )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.WorkflowTypeId] = workflowTypeId.ToString();
                    qryParams[PageParameterKey.Tab] = "Communications";
                    NavigateToLinkedPage( AttributeKeys.FormBuilderPage, qryParams );
                }
            }
            else if ( e.CommandName == "Settings" )
            {
                if ( GetAttributeValue( AttributeKeys.FormBuilderPage ).IsNotNullOrWhiteSpace() )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.WorkflowTypeId] = workflowTypeId.ToString();
                    qryParams[PageParameterKey.Tab] = "Settings";
                    NavigateToLinkedPage( AttributeKeys.FormBuilderPage, qryParams );
                }
            }
            else if ( e.CommandName == "Analytics" )
            {
                if ( GetAttributeValue( AttributeKeys.AnalyticsPage ).IsNotNullOrWhiteSpace() )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.WorkflowTypeId] = workflowTypeId.ToString();
                    NavigateToLinkedPage( AttributeKeys.AnalyticsPage, qryParams );
                }
            }
            else if ( e.CommandName == "Copy" )
            {
                CopyWorkflowType( workflowTypeId );
                BindFormListRepeater( hfSelectedCategory.ValueAsInt() );
            }
            else if ( e.CommandName == "Delete" )
            {
                DeleteForm( workflowTypeId );
                BindFormListRepeater( hfSelectedCategory.ValueAsInt() );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finds the first category.
        /// </summary>
        /// <returns></returns>
        private Category FindFirstCategory( int entityTypeId )
        {
            var categoryQry = new CategoryService( new RockContext() ).Queryable().AsNoTracking();

            var qry = categoryQry.Where( a => a.EntityTypeId == entityTypeId && a.ParentCategoryId == null );

            foreach ( var category in qry.OrderBy( c => c.Order ).ThenBy( c => c.Name ) )
            {
                // return first account they are authorized to view
                if ( category.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    return category;
                }
            }

            return null;
        }

        /// <summary>
        /// Lists the form based on category selected.
        /// </summary>
        protected void ListCategoryForms()
        {
            divFormListTopPanel.Visible = true;
            pnlCategory.Visible = false;
            pnlAddForm.Visible = false;
            pnlFormList.Visible = true;
            ddlSortBy.BindToEnum<FormOrder>();
            var categoryId = hfSelectedCategory.ValueAsInt();
            var category = new CategoryService( new RockContext() ).Get( categoryId );
            if ( category != null )
            {
                // Hide security button if individual does not have ADMINISTRATE on the category
                btnSecurity.Visible = category.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

                // Hide add (and edit) button if not authorized to EDIT this category
                _isAuthorizedToEdit = category.IsAuthorized( Authorization.EDIT, CurrentPerson );
                lbAddForm.Visible = _isAuthorizedToEdit;
                btnEdit.Visible = _isAuthorizedToEdit;
                btnDeleteCategory.Visible = _isAuthorizedToEdit;

                lTitle.Text = $"{category.Name} Form List";
                lDescription.Text = $"Below are a list of forms for the {category.Name} Category";
                btnSecurity.EntityId = category.Id;
                btnDeleteCategory.ToolTip = string.Empty;
                BindFormListRepeater( categoryId );
            }
        }

        /// <summary>
        /// Binds the form to repeater.
        /// </summary>
        private void BindFormListRepeater( int categoryId )
        {
            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );
            var workflowTypeQry = new WorkflowTypeService( rockContext )
                                .Queryable()
                                .Where( a => a.CategoryId == categoryId );

            var category = CategoryCache.Get( categoryId );
            if ( category != null )
            {
                _isAuthorizedToEdit = category.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            btnDeleteCategory.Enabled = !workflowTypeQry.Any();
            if ( workflowTypeQry.Any() )
            {
                btnDeleteCategory.ToolTip = "All forms and workflow types (not shown here) must be removed to enable deleting a category.";
                btnDeleteCategory.Attributes.Remove( "onclick" );
            }
            else
            {
                btnDeleteCategory.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Category.FriendlyTypeName );
                btnDeleteCategory.ToolTip = string.Empty;
            }

            workflowTypeQry = workflowTypeQry.Where( a => a.IsFormBuilder );

            var sortBy = ddlSortBy.SelectedValueAsEnum<FormOrder>( FormOrder.DateCreatedOldestFirst );

            switch ( sortBy )
            {
                case FormOrder.DateCreatedNewestFirst:
                    {
                        workflowTypeQry = workflowTypeQry.OrderByDescending( a => a.CreatedDateTime );
                    }

                    break;
                case FormOrder.DateCreatedOldestFirst:
                    {
                        workflowTypeQry = workflowTypeQry.OrderBy( a => a.CreatedDateTime );
                    }

                    break;
                case FormOrder.Name:
                    {
                        workflowTypeQry = workflowTypeQry.OrderBy( a => a.Name );
                    }

                    break;
                case FormOrder.SubmissionCountLeastToMost:
                case FormOrder.SubmissionCountMostToLeast:
                default:
                    break;
            }

            var workflowTypes = workflowTypeQry.ToList();
            var formResults = new List<FormResult>();
            foreach ( var workflowType in workflowTypes )
            {
                var formResult = new FormResult
                {
                    Id = workflowType.Id,
                    Name = workflowType.Name,
                    CreatedDateTime = workflowType.CreatedDateTime,
                    CreatedBy = workflowType.CreatedByPersonName
                };

                formResults.Add( formResult );
                formResult.SubmissionCount = workflowService.Queryable().Where( a => a.WorkflowTypeId == workflowType.Id ).Count();
            }

            if ( sortBy == FormOrder.SubmissionCountLeastToMost )
            {
                formResults = formResults.OrderBy( a => a.SubmissionCount ).ToList();
            }
            else if ( sortBy == FormOrder.SubmissionCountMostToLeast )
            {
                formResults = formResults.OrderByDescending( a => a.SubmissionCount ).ToList();
            }

            rForms.DataSource = formResults;
            rForms.DataBind();
        }

        /// <summary>
        /// Binds the template to drop down list.
        /// </summary>
        private void BindTemplateDropdown()
        {
            ddlTemplate.Items.Clear();
            ddlTemplate.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                var workflowFormBuilderTemplateService = new WorkflowFormBuilderTemplateService( rockContext );

                // get all group types that have at least one role
                var templates = workflowFormBuilderTemplateService.Queryable().Where( a => a.IsActive ).ToList();

                foreach ( var template in templates )
                {
                    ddlTemplate.Items.Add( new ListItem( template.Name, template.Id.ToString().ToUpper() ) );
                }
            }
        }

        /// <summary>
        /// Shows the category details.
        /// </summary>
        /// <param name="category">The category.</param>
        private void ShowCategoryDetails( Category category )
        {
            hfCategoryId.Value = category.Id.ToString();
            lDescription.Text = string.Empty;
            divFormListTopPanel.Visible = false;
            pnlCategory.Visible = true;
            pnlFormList.Visible = false;

            if ( category.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( Category.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( Category.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            tbCategoryName.Text = category.Name;
            tbCategoryDescription.Text = category.Description;
            tbIconCssClass.Text = category.IconCssClass;
            cpHighlightColor.Text = category.HighlightColor;
        }

        /// <summary>
        /// Delete the workflow type
        /// </summary>
        private void DeleteForm( int workflowTypeId )
        {
            var rockContext = new RockContext();
            var service = new WorkflowTypeService( rockContext );
            var workflowType = service.Get( workflowTypeId );

            if ( workflowType != null )
            {
                if ( !workflowType.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this workflow type.", ModalAlertType.Information );
                    return;
                }

                service.Delete( workflowType );

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Copy the workflow type
        /// </summary>
        private void CopyWorkflowType( int workflowTypeId )
        {
            var rockContext = new RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( workflowTypeId );
            if ( workflowType != null )
            {
                var existingActivityTypes = workflowType.ActivityTypes.OrderBy( a => a.Order ).ToList();

                // Load the state objects for the source workflow type
                var existingWorkflowTypeAttributes = LoadWorkflowTypeAttributeForCopy( workflowType, rockContext );
                var existingWorkflowTypeActivityAttributes = LoadWorkflowTypeActivityAttributeForCopy( existingActivityTypes, rockContext );

                // clone the workflow type
                var newWorkflowType = workflowType.CloneWithoutIdentity();
                newWorkflowType.IsSystem = false;
                newWorkflowType.Name = workflowType.Name + " - Copy";

                workflowTypeService.Add( newWorkflowType );
                rockContext.SaveChanges();

                // Create temporary state objects for the new workflow type
                var newAttributesState = new List<Rock.Model.Attribute>();

                // Dictionary to keep the attributes and activity types linked between the source and the target based on their guids
                var guidXref = new Dictionary<Guid, Guid>();

                // Clone the workflow attributes
                foreach ( var attribute in existingWorkflowTypeAttributes )
                {
                    var newAttribute = attribute.Clone( false );
                    newAttribute.Id = 0;
                    newAttribute.Guid = Guid.NewGuid();
                    newAttribute.IsSystem = false;
                    newAttributesState.Add( newAttribute );

                    guidXref.Add( attribute.Guid, newAttribute.Guid );

                    foreach ( var qualifier in attribute.AttributeQualifiers )
                    {
                        var newQualifier = qualifier.Clone( false );
                        newQualifier.Id = 0;
                        newQualifier.Guid = Guid.NewGuid();
                        newQualifier.IsSystem = false;
                        newAttribute.AttributeQualifiers.Add( newQualifier );

                        guidXref.Add( qualifier.Guid, newQualifier.Guid );
                    }
                }

                // Save the workflow type attributes
                SaveAttributes( new Workflow().TypeId, "WorkflowTypeId", newWorkflowType.Id.ToString(), newAttributesState, rockContext );

                // Create new guids for all the existing activity types
                foreach ( var activityType in existingActivityTypes )
                {
                    guidXref.Add( activityType.Guid, Guid.NewGuid() );
                }

                foreach ( var activityType in existingActivityTypes )
                {
                    var newActivityType = activityType.Clone( false );
                    newActivityType.WorkflowTypeId = 0;
                    newActivityType.Id = 0;
                    newActivityType.Guid = guidXref[activityType.Guid];
                    newWorkflowType.ActivityTypes.Add( newActivityType );
                    rockContext.SaveChanges();

                    var newActivityAttributes = new List<Rock.Model.Attribute>();
                    foreach ( var attribute in existingWorkflowTypeActivityAttributes[activityType.Guid] )
                    {
                        var newAttribute = attribute.Clone( false );
                        newAttribute.Id = 0;
                        newAttribute.Guid = Guid.NewGuid();
                        newAttribute.IsSystem = false;
                        newActivityAttributes.Add( newAttribute );

                        guidXref.Add( attribute.Guid, newAttribute.Guid );

                        foreach ( var qualifier in attribute.AttributeQualifiers )
                        {
                            var newQualifier = qualifier.Clone( false );
                            newQualifier.Id = 0;
                            newQualifier.Guid = Guid.NewGuid();
                            newQualifier.IsSystem = false;
                            newAttribute.AttributeQualifiers.Add( newQualifier );

                            guidXref.Add( qualifier.Guid, newQualifier.Guid );
                        }
                    }

                    // Save ActivityType Attributes
                    SaveAttributes( new WorkflowActivity().TypeId, "ActivityTypeId", newActivityType.Id.ToString(), newActivityAttributes, rockContext );

                    foreach ( var actionType in activityType.ActionTypes )
                    {
                        var newActionType = actionType.Clone( false );
                        newActionType.Id = 0;
                        newActionType.ActivityTypeId = 0;
                        newActionType.WorkflowFormId = null;
                        newActionType.Guid = Guid.NewGuid();
                        newActivityType.ActionTypes.Add( newActionType );

                        if ( actionType.CriteriaAttributeGuid.HasValue &&
                            guidXref.ContainsKey( actionType.CriteriaAttributeGuid.Value ) )
                        {
                            newActionType.CriteriaAttributeGuid = guidXref[actionType.CriteriaAttributeGuid.Value];
                        }

                        Guid criteriaAttributeGuid = actionType.CriteriaValue.AsGuid();
                        if ( !criteriaAttributeGuid.IsEmpty() &&
                            guidXref.ContainsKey( criteriaAttributeGuid ) )
                        {
                            newActionType.CriteriaValue = guidXref[criteriaAttributeGuid].ToString();
                        }

                        if ( actionType.WorkflowForm != null )
                        {
                            newActionType.WorkflowForm = actionType.WorkflowForm.Clone( false );
                            newActionType.WorkflowForm.Id = 0;
                            newActionType.WorkflowForm.Guid = Guid.NewGuid();
                            WorkflowFormEditor.CopyEditableProperties( actionType.WorkflowForm, newActionType.WorkflowForm );

                            foreach ( var section in actionType.WorkflowForm.FormSections )
                            {
                                var newSection = section.Clone( false );
                                newSection.Id = 0;
                                newSection.Guid = Guid.NewGuid();
                                newActionType.WorkflowForm.FormSections.Add( newSection );
                                guidXref.Add( section.Guid, newSection.Guid );
                            }

                            rockContext.SaveChanges();

                            foreach ( var formAttribute in actionType.WorkflowForm.FormAttributes )
                            {
                                if ( guidXref.ContainsKey( formAttribute.Attribute.Guid ) )
                                {
                                    var attribute = AttributeCache.Get( guidXref[formAttribute.Attribute.Guid], rockContext );
                                    WorkflowActionFormSection section = null;
                                    if ( guidXref.ContainsKey( formAttribute.ActionFormSection.Guid ) )
                                    {
                                        section = newActionType.WorkflowForm.FormSections.FirstOrDefault( a => a.Guid == guidXref[formAttribute.ActionFormSection.Guid] );
                                    }

                                    var newFormAttribute = formAttribute.Clone( false );
                                    newFormAttribute.WorkflowActionFormId = 0;
                                    newFormAttribute.Id = 0;
                                    newFormAttribute.Guid = Guid.NewGuid();
                                    newFormAttribute.AttributeId = attribute.Id;
                                    newFormAttribute.ActionFormSectionId = section.Id;

                                    if ( newFormAttribute.FieldVisibilityRules != null )
                                    {
                                        var visibilityRules = newFormAttribute.FieldVisibilityRules.Clone();

                                        foreach ( var rule in visibilityRules.RuleList )
                                        {
                                            if ( rule.ComparedToFormFieldGuid != null && guidXref.ContainsKey( rule.ComparedToFormFieldGuid.Value ) )
                                            {
                                                rule.ComparedToFormFieldGuid = guidXref[rule.ComparedToFormFieldGuid.Value];
                                            }
                                        }

                                        newFormAttribute.FieldVisibilityRules = visibilityRules;
                                    }

                                    newActionType.WorkflowForm.FormAttributes.Add( newFormAttribute );
                                }
                            }
                        }

                        rockContext.SaveChanges();

                        newActionType.LoadAttributes( rockContext );
                        if ( actionType.Attributes != null && actionType.Attributes.Any() )
                        {
                            foreach ( var attributeKey in actionType.Attributes.Select( a => a.Key ) )
                            {
                                string value = actionType.GetAttributeValue( attributeKey );
                                Guid guidValue = value.AsGuid();
                                if ( !guidValue.IsEmpty() && guidXref.ContainsKey( guidValue ) )
                                {
                                    newActionType.SetAttributeValue( attributeKey, guidXref[guidValue].ToString() );
                                }
                                else
                                {
                                    newActionType.SetAttributeValue( attributeKey, value );
                                }
                            }

                            newActionType.SaveAttributeValues( rockContext );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<Rock.Model.Attribute> attributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ).ToList() )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attribute in attributes )
            {
                Helper.SaveAttributeEdits( attribute, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        private List<Rock.Model.Attribute> LoadWorkflowTypeAttributeForCopy( WorkflowType workflowType, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            return attributeService
                .GetByEntityTypeId( new Workflow().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( workflowType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        private Dictionary<Guid, List<Rock.Model.Attribute>> LoadWorkflowTypeActivityAttributeForCopy( List<WorkflowActivityType> activityTypes, RockContext rockContext )
        {
            var activityAttributes = new Dictionary<Guid, List<Rock.Model.Attribute>>();
            var attributeService = new AttributeService( rockContext );
            foreach ( var activityType in activityTypes )
            {
                var activityTypeAttributes = attributeService
                    .GetByEntityTypeId( new WorkflowActivity().TypeId, true ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "ActivityTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( activityType.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();

                activityAttributes.Add( activityType.Guid, activityTypeAttributes );

                foreach ( var actionType in activityType.ActionTypes )
                {
                    var action = EntityTypeCache.Get( actionType.EntityTypeId );
                    if ( action != null )
                    {
                        Rock.Attribute.Helper.UpdateAttributes( action.GetEntityType(), actionType.TypeId, "EntityTypeId", actionType.EntityTypeId.ToString(), rockContext );
                        actionType.LoadAttributes( rockContext );
                    }
                }
            }

            return activityAttributes;
        }

        private int? GetCategoryId()
        {
            var categoryId = PageParameter( PageParameterKey.CategoryId ).AsIntegerOrNull();
            if ( !categoryId.HasValue )
            {
                var preferences = GetBlockPersonPreferences();
                categoryId = preferences.GetValue( UserPreferenceKeys.CategoryId ).AsIntegerOrNull();
            }

            return categoryId;
        }

        #endregion

        #region Block Specific Enums

        /// <summary>
        /// Defines a set of ordering operations that can be performed on
        /// a collection of <see cref="Rock.Model.WorkflowType"/> objects.
        /// </summary>
        public enum FormOrder
        {
            /// <summary>
            /// Date Created (newest first)
            /// </summary>
            [Description( "Date Created (newest first)" )]
            DateCreatedNewestFirst = 0,

            /// <summary>
            /// Date Created (oldest first)
            /// </summary>
            [Description( "Date Created (oldest first)" )]
            DateCreatedOldestFirst = 1,

            /// <summary>
            /// Name
            /// </summary>
            [Description( "Name" )]
            Name = 2,

            /// <summary>
            /// Submission Count (least to most)
            /// </summary>
            [Description( "Submission Count (least to most)" )]
            SubmissionCountLeastToMost = 3,

            /// <summary>
            /// Submission Count (most to least)
            /// </summary>
            [Description( "Submission Count (most to least)" )]
            SubmissionCountMostToLeast = 4,
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        ///
        /// </summary>
        public class FormResult
        {
            /// <summary>
            /// Gets or sets the form identifier.
            /// </summary>
            /// <value>
            /// The form identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the submission count.
            /// </summary>
            /// <value>
            /// The submission count.
            /// </value>
            public int SubmissionCount { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Created Date Time.
            /// </summary>
            /// <value>
            /// The Created Date Time.
            /// </value>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the Created By.
            /// </summary>
            /// <value>
            /// The Created By.
            /// </value>
            public string CreatedBy { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description
            {
                get
                {
                    string description = string.Empty;
                    if ( CreatedBy.IsNotNullOrWhiteSpace() )
                    {
                        description = $"Created By {CreatedBy}";
                    }

                    if ( CreatedDateTime.HasValue )
                    {
                        if ( description.IsNullOrWhiteSpace() )
                        {
                            description = "Created";
                        }

                        description += $" {CreatedDateTime.ToElapsedString()}";
                    }

                    return description;
                }
            }
        }

        #endregion Supporting Classes
    }
}