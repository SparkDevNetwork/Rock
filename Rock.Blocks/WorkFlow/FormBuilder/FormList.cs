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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormList;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Data.Entity;

namespace Rock.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// List the existing forms for the selected category.
    /// </summary>
    [DisplayName( "Form List" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows the List of existing forms for the selected category." )]

    #region Block Attributes

    [LinkedPage(
        "Submissions Page",
        Description = "The page that shows the submissions for this form.",
        Order = 0,
        Key = AttributeKey.SubmissionsPage )]

    [LinkedPage(
        "Form Builder Page",
        Description = "The page that has the form builder editor.",
        Order = 1,
        Key = AttributeKey.FormBuilderPage )]

    [LinkedPage(
        "Analytics Page",
        Description = " The page that shows the analytics for this form.",
        Order = 2,
        Key = AttributeKey.AnalyticsPage )]

    #endregion Block Attributes

    // was [Rock.SystemGuid.BlockTypeGuid( "844BEA3B-B79B-40FD-9B05-02420C822840" )]
    [Rock.SystemGuid.BlockTypeGuid( "B7C76420-9B34-422A-B161-87BDB45DD50C" )]
    [Rock.SystemGuid.EntityTypeGuid( "19A77D95-1FC0-4FC7-9DB1-69D98B367B06")]
    public class FormList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AnalyticsPage = "AnalyticsPage";
            public const string FormBuilderPage = "FormBuilderPage";
            public const string SubmissionsPage = "SubmissionsPage";
        }

        private static class NavigationUrlKey
        {
            public const string AnalyticsPage = "AnalyticsPage";
            public const string FormBuilderPage = "FormBuilderPage";
            public const string SubmissionsPage = "SubmissionsPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new FormListBag();

            box.Forms = GetForms();

            box.CanAddNewCategory = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
		/// Gets all Form Builder workflow types grouped by the Guid of their associated Category.
		/// </summary>
		private Dictionary<Guid, List<FormListItemBag>> GetForms()
        {
            var formsByCategoryGuid = new Dictionary<Guid, List<FormListItemBag>>();

            var workflowTypeService = new WorkflowTypeService( RockContext );
            var workflowService = new WorkflowService( RockContext );

            var workflowTypeQuery = workflowTypeService
                .Queryable()
                .AsNoTracking()
                .Where( wt => wt.IsFormBuilder );

            var workflowTypes = workflowTypeQuery
                .ToList()
                .Where( wt => wt.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();

            foreach ( var wt in workflowTypes )
            {
                CategoryCache category = null;

                if ( wt.CategoryId.HasValue )
                {
                    category = CategoryCache.Get( wt.CategoryId.Value );
                }

                if ( category == null )
                {
                    continue;
                }

                bool canEdit = false;

                /*
                     12/17/2025 - MSE

                     Determines EDIT authorization by first checking for explicit security on the
                     Workflow Type (Form). If none is defined, authorization falls back to the Category.

                     AuthorizedForEntity() is used because it returns null when no security is
                     configured. A true or false value indicates explicit security, while null
                     allows Category security to be evaluated.

                     Reason: Prevent unauthorized actions when an individual has EDIT permission
                     on the Category but is explicitly denied EDIT on the Form.
                */
                var formAuth = Authorization.AuthorizedForEntity( wt, Authorization.EDIT, RequestContext.CurrentPerson, false );
                if ( formAuth.HasValue )
                {
                    canEdit = formAuth.Value;
                }
                else if ( category != null )
                {
                    canEdit = category.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                }

                var submissionCount = workflowService.Queryable().Count( w => w.WorkflowTypeId == wt.Id );

                var form = new FormListItemBag
                {
                    Id = wt.Id,
                    Name = wt.Name,
                    Description = wt.Description,
                    Guid = wt.Guid,
                    CategoryGuid = category.Guid,
                    SubmissionCount = submissionCount,
                    Slug = wt.Slug,
                    CanEdit = canEdit,
                    CreatedDateTime = wt.CreatedDateTime,
                    CreatedByPersonName = wt.CreatedByPersonName
                };

                if ( !formsByCategoryGuid.TryGetValue( category.Guid, out var list ) )
                {
                    list = new List<FormListItemBag>();
                    formsByCategoryGuid[category.Guid] = list;
                }

                list.Add( form );
            }

            return formsByCategoryGuid;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.SubmissionsPage] = this.GetLinkedPageUrl( AttributeKey.SubmissionsPage ),
                [NavigationUrlKey.FormBuilderPage] = this.GetLinkedPageUrl( AttributeKey.FormBuilderPage ),
                [NavigationUrlKey.AnalyticsPage] = this.GetLinkedPageUrl( AttributeKey.AnalyticsPage )
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the list of available templates for creating new forms.
        /// </summary>
        /// <returns>A list of template items.</returns>
        [BlockAction]
        public BlockActionResult GetTemplates()
        {
            var templateService = new WorkflowFormBuilderTemplateService( RockContext );
            var templates = templateService.Queryable()
                .AsNoTracking()
                .Where( t => t.IsActive )
                .ToListItemBagList();

            return ActionOk( templates );
        }

        /// <summary>
        /// Gets the unique slug for a workflow type.
        /// </summary>
        /// <param name="name">The name of the workflow type.</param>
        /// <returns>A unique slug for the workflow type.</returns>
        [BlockAction]
        public BlockActionResult GetUniqueSlug( string name )
        {
            var workflowTypeService = new WorkflowTypeService( RockContext );
            var slug = workflowTypeService.GetUniqueSlug( name );
            return ActionOk( slug );
        }

        /// <summary>
        /// Creates a new form (workflow type) with the specified settings.
        /// </summary>
        /// <param name="name">The name of the form.</param>
        /// <param name="description">The description of the form.</param>
        /// <param name="slug">The unique slug for the form.</param>
        /// <param name="categoryGuid">The unique identifier of the category to assign the form to.</param>
        /// <param name="templateGuid">The unique identifier of the template to use for the form.</param>
        /// <returns>A result containing the ID of the newly created form.</returns>
        [BlockAction]
        public BlockActionResult CreateForm( string name, string description, string slug, Guid? categoryGuid, Guid? templateGuid )
        {
            var rockContext = RockContext;
            var service = new WorkflowTypeService( rockContext );
            var categoryService = new CategoryService( rockContext );

            var workflowType = new WorkflowType();

            var formBuilderSettings = new Rock.Workflow.FormBuilder.FormSettings();
            formBuilderSettings.CompletionAction = new Rock.Workflow.FormBuilder.FormCompletionActionSettings
            {
                Message = "Your information has been submitted successfully.",
                Type = Rock.Workflow.FormBuilder.FormCompletionActionType.DisplayMessage
            };

            workflowType.IsActive = true;
            workflowType.Name = name;
            workflowType.Description = description;
            workflowType.Slug = slug;

            if ( categoryGuid.HasValue )
            {
                var category = categoryService.Get( categoryGuid.Value );
                if ( category != null )
                {
                    // Shouldn't happen, since the "Add" button is guarded by the UI if not authorized.
                    if ( !category.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "You are not authorized to add a form to this category." );
                    }

                    workflowType.CategoryId = category.Id;
                }
            }

            if ( templateGuid.HasValue )
            {
                workflowType.FormBuilderTemplateId = new WorkflowFormBuilderTemplateService( rockContext ).GetId( templateGuid.Value );
            }

            workflowType.ProcessingIntervalSeconds = 365 * 24 * 60 * 60;
            workflowType.IsPersisted = false;
            workflowType.LoggingLevel = WorkflowLoggingLevel.None;
            workflowType.IsFormBuilder = true;
            workflowType.WorkTerm = "Form";
            workflowType.IsSystem = false;
            workflowType.FormBuilderSettingsJson = formBuilderSettings.ToJson();

            if ( !workflowType.IsValid )
            {
                return ActionBadRequest( workflowType.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" ) );
            }

            service.Add( workflowType );
            rockContext.SaveChanges();

            // Create temporary state objects for the new workflow type
            var newAttributesState = new List<Rock.Model.Attribute>();

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
            var workflowEntityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>();
            var attributeService = new AttributeService( rockContext );
            foreach ( var attribute in newAttributesState )
            {
                attribute.EntityTypeId = workflowEntityTypeId.Value;
                attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                attribute.EntityTypeQualifierValue = workflowType.Id.ToString();
                attributeService.Add( attribute );
            }

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
            workflowActionType.WorkflowForm.PersonEntryRecordSourceValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_WORKFLOW.AsGuid() );
            workflowActionType.WorkflowForm.PersonEntryConnectionStatusValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid() );
            workflowActionType.WorkflowForm.PersonEntryGroupLocationTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            workflowActionType.WorkflowForm.Actions = "Submit^^^Your information has been submitted successfully.";

            var workFlowSection = new WorkflowActionFormSection();
            workflowActionType.WorkflowForm.FormSections.Add( workFlowSection );
            workFlowSection.Guid = Guid.NewGuid();
            workflowActivityType.Order = 0;

            var systemEmail = new SystemCommunicationService( rockContext ).Get( Rock.SystemGuid.SystemCommunication.WORKFLOW_FORM_NOTIFICATION.AsGuid() );
            if ( systemEmail != null )
            {
                workflowActionType.WorkflowForm.NotificationSystemCommunicationId = systemEmail.Id;
            }

            workflowActionType.EntityTypeId = formBuilderEntityTypeId.Value;
            workflowActionType.Name = "Form Builder";
            workflowActionType.Order = 0;
            rockContext.SaveChanges();

            return ActionOk( workflowType.Id.ToString() );
        }

        /// <summary>
        /// Gets the details of the specified category for editing.
        /// </summary>
        /// <param name="categoryGuid">The unique identifier of the category.</param>
        /// <returns>A bag containing the category details.</returns>
        [BlockAction]
        public BlockActionResult GetCategory( Guid categoryGuid )
        {
            var categoryService = new CategoryService( RockContext );
            var category = categoryService.Get( categoryGuid );

            if ( category == null )
            {
                return ActionBadRequest( "Invalid category." );
            }

            // Check authorization
            if ( !category.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            var bag = new UpdateFormCategoryBag
            {
                CategoryGuid = category.Guid,
                Name = category.Name,
                Description = category.Description,
                IconCssClass = category.IconCssClass,
                HighlightColor = category.HighlightColor
            };

            if ( category.ParentCategoryId.HasValue )
            {
                bag.ParentCategoryGuid = category.ParentCategory.Guid;
            }

            return ActionOk( bag );
        }

        /// <summary>
        /// Gets the security status of the specified category.
        /// </summary>
        /// <param name="categoryGuid">The unique identifier of the category.</param>
        [BlockAction]
        public BlockActionResult GetCategorySecurity( Guid? categoryGuid )
        {
            var canEdit = false;
            var canDelete = false;
            if ( categoryGuid.HasValue )
            {
                var category = CategoryCache.Get( categoryGuid.Value );
                if ( category != null )
                {
                    canEdit = category.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                    canDelete = category.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
                }
            }

            var bag = new FormListCategorySecurityBag
            {
                CanEdit = canEdit,
                CanDelete = canDelete
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Adds a new category or edits an existing one.
        /// </summary>
        /// <param name="bag">The bag containing the category details.</param>
        /// <returns>A result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult AddOrEditCategory( UpdateFormCategoryBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Invalid request." );
            }

            Category category = null;
            CategoryService categoryService = new CategoryService( RockContext );

            if ( bag.CategoryGuid.HasValue )
            {
                category = categoryService.Get( bag.CategoryGuid.Value );
            }

            // Adding a new category.
            if ( category == null )
            {
                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    // Shouldn't happen, since this is guarded by the UI via the flag we send down
                    // on initialization.
                    return ActionForbidden( "You are not authorized to create a new category." );
                }

                // Add new category
                category = new Category();
                var entityTypeId = EntityTypeCache.GetId<WorkflowType>();
                category.EntityTypeId = entityTypeId.Value;
                category.IsSystem = false;
                category.Order = 0;
                categoryService.Add( category );
            }

            // Editing an existing category.
            if ( !category.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this category." );
            }

            category.Name = bag.Name;
            category.Description = bag.Description;
            category.IconCssClass = bag.IconCssClass;
            category.HighlightColor = bag.HighlightColor;

            if ( bag.ParentCategoryGuid.HasValue )
            {
                var parentCategory = CategoryCache.Get( bag.ParentCategoryGuid.Value );
                if ( parentCategory != null )
                {
                    category.ParentCategoryId = parentCategory.Id;
                }
            }
            else
            {
                category.ParentCategoryId = null;
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified category.
        /// </summary>
        /// <param name="categoryGuid">The unique identifier of the category to delete.</param>
        /// <returns>A result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult DeleteCategory( Guid categoryGuid )
        {
            var categoryService = new CategoryService( RockContext );
            var category = categoryService.Get( categoryGuid );

            if ( category == null )
            {
                return ActionBadRequest( "Invalid category." );
            }

            if ( !category.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to delete this category." );
            }

            // Check if has any child categories
            if ( categoryService.Queryable().Any( c => c.ParentCategoryId == category.Id ) )
            {
                return ActionBadRequest( "This category has child categories and cannot be deleted." );
            }

            // Check for associated forms (Workflow Types)
            var workflowTypeService = new WorkflowTypeService( RockContext );
            if ( workflowTypeService.Queryable().Any( wt => wt.CategoryId == category.Id ) )
            {
                return ActionBadRequest( "This category cannot be deleted because it contains forms. (Some forms may not be displayed here.)" );
            }

            categoryService.Delete( category );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the list of pages that link to the specified form (workflow type).
        /// </summary>
        /// <param name="workflowTypeGuid">The unique identifier of the workflow type.</param>
        /// <returns>A list of pages linking to the form.</returns>
        [BlockAction]
        public BlockActionResult GetFormLinks( Guid workflowTypeGuid )
        {
            var workflowTypeService = new WorkflowTypeService( RockContext );
            var workflowType = workflowTypeService.Get( workflowTypeGuid );

            if ( workflowType == null )
            {
                return ActionBadRequest( "Invalid form." );
            }

            var workflowEntryBlockType = BlockTypeCache.Get( Rock.SystemGuid.BlockType.WORKFLOW_ENTRY );
            var obsidianWorkflowEntryBlockType = BlockTypeCache.Get( Rock.SystemGuid.BlockType.OBSIDIAN_WORKFLOW_ENTRY );

            var pages = PageCache.All()
                .Where( p => p.Blocks.Any( b => b.BlockTypeId == workflowEntryBlockType.Id || b.BlockTypeId == obsidianWorkflowEntryBlockType.Id ) )
                .ToList();

            var filteredPages = new List<PageCache>();

            foreach ( var page in pages.Where( p => p.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) ) )
            {
                var workflowEntryBlocks = page.Blocks.Where( b => b.BlockTypeId == workflowEntryBlockType.Id || b.BlockTypeId == obsidianWorkflowEntryBlockType.Id ).ToList();

                // Only show pages with a "Workflow Entry" block on them that are not configured to show a single specific workflow type.
                foreach ( var block in workflowEntryBlocks )
                {
                    block.LoadAttributes();
                    var workflowTypeAttribute = block.Attributes.FirstOrDefault( a => a.Value.EntityTypeQualifierColumn == "BlockTypeId" && a.Value.EntityTypeQualifierValue == workflowEntryBlockType.Id.ToString() );

                    // Add page if the Workflow Entry block's workflowType block setting is not configured to show a single specific workflow type.
                    if ( workflowTypeAttribute.Key.IsNullOrWhiteSpace() || block.GetAttributeValue( workflowTypeAttribute.Key ).IsNullOrWhiteSpace() )
                    {
                        if ( block.GetAttributeValue( "EnableForFormSharing" ).AsBoolean() )
                        {
                            filteredPages.Add( page );
                            break;
                        }
                    }
                }
            }

            var results = filteredPages.Select( page => new FormListLinkToFormBag
            {
                PageId = page.Id,
                SiteAndPage = $"{page.Site} > {page.InternalName}",
                FriendlyRouteUrl = page.PageRoutes.FirstOrDefault()?.Route != null
                    ? GetFullUrl( page, page.PageRoutes.FirstOrDefault().Route ).Replace( "%7B", "{" ).Replace( "%7D", "}" )
                    : null,
                RouteUrl = BuildRouteURL( page, workflowType )
            } ).ToList();

            return ActionOk( results );
        }

        /// <summary>
        /// Deletes the specified form (workflow type).
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <returns>A result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult DeleteForm( Guid workflowTypeGuid )
        {
            var workflowTypeService = new WorkflowTypeService( RockContext );
            var workflowType = workflowTypeService.Get( workflowTypeGuid );

            if ( workflowType == null )
            {
                return ActionBadRequest( "Invalid form." );
            }

            if ( !workflowType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to delete this form." );
            }

            workflowTypeService.Delete( workflowType );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Clones the specified form (workflow type).
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <returns>A result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult CloneForm( Guid workflowTypeGuid )
        {
            var workflowTypeService = new WorkflowTypeService( RockContext );
            var workflowType = workflowTypeService.Get( workflowTypeGuid );

            if ( workflowType == null )
            {
                return ActionBadRequest( "Invalid form." );
            }

            if ( !workflowType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to copy this form." );
            }

            var existingActivityTypes = workflowType.ActivityTypes.OrderBy( a => a.Order ).ToList();
            var existingWorkflowTypeAttributes = LoadWorkflowTypeAttributeForCopy( workflowType, RockContext );
            var existingWorkflowTypeActivityAttributes = LoadWorkflowTypeActivityAttributeForCopy( existingActivityTypes, RockContext );

            // Clone the workflow type
            var newWorkflowType = workflowType.CloneWithoutIdentity();
            newWorkflowType.IsSystem = false;
            newWorkflowType.Name = workflowType.Name + " - Copy";

            workflowTypeService.Add( newWorkflowType );
            RockContext.SaveChanges();

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
            SaveAttributes( new Rock.Model.Workflow().TypeId, "WorkflowTypeId", newWorkflowType.Id.ToString(), newAttributesState, RockContext );

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
                RockContext.SaveChanges();

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
                SaveAttributes( new WorkflowActivity().TypeId, "ActivityTypeId", newActivityType.Id.ToString(), newActivityAttributes, RockContext );

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

                        RockContext.SaveChanges();

                        foreach ( var formAttribute in actionType.WorkflowForm.FormAttributes )
                        {
                            if ( formAttribute.Attribute == null || !guidXref.ContainsKey( formAttribute.Attribute.Guid ) )
                            {
                                continue;
                            }

                            var attribute = AttributeCache.Get( guidXref[formAttribute.Attribute.Guid], RockContext );
                            if ( attribute == null )
                            {
                                continue;
                            }

                            WorkflowActionFormSection section = null;
                            if ( formAttribute.ActionFormSection != null && guidXref.ContainsKey( formAttribute.ActionFormSection.Guid ) )
                            {
                                section = newActionType.WorkflowForm.FormSections.FirstOrDefault( a => a.Guid == guidXref[formAttribute.ActionFormSection.Guid] );
                            }

                            if ( section == null )
                            {
                                continue;
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

                    RockContext.SaveChanges();

                    newActionType.LoadAttributes( RockContext );
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

                        newActionType.SaveAttributeValues( RockContext );
                    }
                }
            }

            return ActionOk();
        }

        #endregion Block Actions

        #region Helper Methods

        /// <summary>
        /// Gets the full URL including the site domain.
        /// </summary>
        /// <param name="pageCache">The page cache object.</param>
        /// <param name="relativeUrl">The relative URL string.</param>
        /// <returns>The full absolute URL.</returns>
        private string GetFullUrl( PageCache pageCache, string relativeUrl )
        {
            // Use the site's domain and append the page's relative URL.
            // If the page is a fully formed URL (with scheme, domain, and port),
            // then it will override the site domain.
            var site = SiteCache.Get( pageCache.SiteId );
            var uri = new Uri( site.DefaultDomainUri, relativeUrl );

            return uri.AbsoluteUri;
        }

        /// <summary>
        /// Builds the route URL for a specific page and workflow type.
        /// </summary>
        /// <param name="page">The page cache object.</param>
        /// <param name="workflowType">The workflow type object.</param>
        /// <returns>The constructed route URL.</returns>
        private string BuildRouteURL( PageCache page, WorkflowType workflowType )
        {
            var route = page.PageRoutes.FirstOrDefault();
            var parameters = new Dictionary<string, string>()
            {
                { "WorkflowTypeId", workflowType?.Id.ToString() },
                { "WorkflowTypeGuid", workflowType?.Guid.ToString() },
                { "WorkflowTypeSlug", workflowType?.Slug },
                { "WorkflowId", "0" },
                { "WorkflowGuid", "" },
            };

            var pageReference = route != null ? new PageReference( page.Id, route.Id, parameters ) : new PageReference( page.Guid.ToString(), parameters );

            return GetFullUrl( page, pageReference.BuildUrl() );
        }

        /// <summary>
        /// Saves the attributes for an entity.
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

        /// <summary>
        /// Loads the workflow type attributes for copying.
        /// </summary>
        /// <param name="workflowType">The workflow type.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A list of attributes for the workflow type.</returns>
        private List<Rock.Model.Attribute> LoadWorkflowTypeAttributeForCopy( WorkflowType workflowType, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            return attributeService
                .GetByEntityTypeId( new Rock.Model.Workflow().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( workflowType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Loads the workflow type activity attributes for copying.
        /// </summary>
        /// <param name="activityTypes">The activity types.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A dictionary of activity type guids to their attributes.</returns>
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
                        Helper.UpdateAttributes( action.GetEntityType(), actionType.TypeId, "EntityTypeId", actionType.EntityTypeId.ToString(), rockContext );
                        actionType.LoadAttributes( rockContext );
                    }
                }
            }

            return activityAttributes;
        }

        #endregion
    }
}
