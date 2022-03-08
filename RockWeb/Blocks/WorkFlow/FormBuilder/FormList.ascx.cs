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

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
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
        Description = "The Page that shows the submissions for this form.",
        Order = 0,
        Key = AttributeKeys.SubmissionsPage )]

    [LinkedPage(
        "FormBuilder Detail Page",
        Description = "Page to edit using the form builder.",
        Order = 1,
        Key = AttributeKeys.FormBuilderDetailPage )]

    [LinkedPage(
        "Analytics Page",
        Description = "Page used to view the analytics for this form.",
        Order = 2,
        Key = AttributeKeys.AnalyticsPage )]

    #endregion Rock Attributes
    public partial class FormList : RockBlock
    {
        public static class AttributeKeys
        {
            public const string AnalyticsPage = "AnalyticsPage";
            public const string FormBuilderDetailPage = "FormBuilderDetailPage";
            public const string SubmissionsPage = "SubmissionsPage";
        }

        public const string CategoryNodePrefix = "C";

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
            base.OnLoad( e );

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
                    var categoryId = PageParameter( "CategoryId" ).AsIntegerOrNull();
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
                        ListCategoryForms();
                    }
                }
            }
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
            //Category category;

            //var rockContext = new RockContext();
            //CategoryService categoryService = new CategoryService( rockContext );

            //int categoryId = hfSelectedCategory.ValueAsInt();

            //if ( categoryId == 0 )
            //{
            //    category = new Category();
            //    category.IsSystem = false;
            //    category.EntityTypeId = entityTypeId;
            //    category.EntityTypeQualifierColumn = entityTypeQualifierProperty;
            //    category.EntityTypeQualifierValue = entityTypeQualifierValue;
            //    category.Order = 0;
            //    categoryService.Add( category );
            //}
            //else
            //{
            //    category = categoryService.Get( categoryId );
            //}

            //category.Name = tbName.Text;
            //category.Description = tbDescription.Text;
            //category.ParentCategoryId = cpParentCategory.SelectedValueAsInt();
            //category.IconCssClass = tbIconCssClass.Text;
            //category.HighlightColor = tbHighlightColor.Text;

            //List<int> orphanedBinaryFileIdList = new List<int>();

            //if ( !Page.IsValid )
            //{
            //    return;
            //}

            //// if the category IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of category didn't pass.
            //// So, make sure a message is displayed in the validation summary
            //cvCategory.IsValid = category.IsValid;

            //if ( !cvCategory.IsValid )
            //{
            //    cvCategory.ErrorMessage = category.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
            //    return;
            //}

            //BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            //foreach ( int binaryFileId in orphanedBinaryFileIdList )
            //{
            //    var binaryFile = binaryFileService.Get( binaryFileId );
            //    if ( binaryFile != null )
            //    {
            //        // marked the old images as IsTemporary so they will get cleaned up later
            //        binaryFile.IsTemporary = true;
            //    }
            //}

            //rockContext.SaveChanges();

            //var qryParams = new Dictionary<string, string>();
            //qryParams["CategoryId"] = category.Id.ToString();
            //qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );
            //NavigateToPage( RockPage.Guid, qryParams );
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
            ShowCategoryDetails( category );
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryChild_Click( object sender, EventArgs e )
        {
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

            workflowType.IsActive = true;
            workflowType.Name = tbFormName.Text;
            workflowType.Description = tbDescription.Text;
            workflowType.CategoryId = cpCategory.SelectedValueAsInt();
            workflowType.FormBuilderTemplateId = ddlTemplate.SelectedValueAsInt();
            workflowType.ProcessingIntervalSeconds = 365 * 24 * 60 * 60;
            workflowType.IsPersisted = false;
            workflowType.LoggingLevel = WorkflowLoggingLevel.None;
            workflowType.IsFormBuilder = true;
            workflowType.WorkTerm = "Work";
            if ( validationErrors.Any() )
            {
                nbValidationError.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul>",
                    validationErrors.AsDelimited( "</li><li>" ) );
                nbValidationError.Visible = true;

                return;
            }

            if ( !Page.IsValid || !workflowType.IsValid )
            {
                return;
            }

            service.Add( workflowType );
            rockContext.SaveChanges();

            var formBuilderEntityTypeId = EntityTypeCache.GetId<Rock.Workflow.Action.FormBuilder>();
            var workflowActivityType = new WorkflowActivityType();
            workflowType.ActivityTypes.Add( workflowActivityType );
            workflowActivityType.IsActive = true;
            workflowActivityType.Name = "Form Builder";
            workflowActivityType.Order = 0;
            var workflowActionType = new WorkflowActionType();
            workflowActivityType.ActionTypes.Add( workflowActionType );
            workflowActionType.EntityTypeId = formBuilderEntityTypeId.Value;
            workflowActionType.Name = "Form Builder";
            workflowActionType.Order = 0;
            rockContext.SaveChanges();

            if ( GetAttributeValue( AttributeKeys.FormBuilderDetailPage ).IsNotNullOrWhiteSpace() )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams["WorkflowTypeId"] = workflowType.Id.ToString();
                NavigateToLinkedPage( AttributeKeys.FormBuilderDetailPage, qryParams );
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
            BindFormList(hfSelectedCategory.ValueAsInt());
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
                lTitle.Text = $"{category.Name} Form List";
                lDescription.Text = $"Below are a list of forms for the {category.Name} Category";
                btnSecurity.EntityId = category.Id;
                btnDelete.ToolTip = string.Empty;
                BindFormList( categoryId );
            }
        }

        /// <summary>
        /// Binds the form to repeater.
        /// </summary>

        private void BindFormList( int categoryId )
        {
            var rockContext = new RockContext();
            var workflowTypes = new WorkflowTypeService( rockContext )
                                .Queryable()
                                .Where( a => a.IsFormBuilder && a.CategoryId == categoryId );

            var sortBy = ddlSortBy.SelectedValueAsEnum<FormOrder>( FormOrder.DateCreated );
            switch ( sortBy )
            {
                case FormOrder.DateCreated:
                    {
                        workflowTypes = workflowTypes.OrderBy( a => a.CreatedDateTime );
                    }
                    break;
                case FormOrder.Name:
                    {
                        workflowTypes = workflowTypes.OrderBy( a => a.Name );
                    }
                    break;
                case FormOrder.SubmissionCount:
                    {
                        //workflowTypes = workflowTypes.OrderBy( a => a. );
                    }
                    break;
                default:
                    break;
            }

            rForms.DataSource = workflowTypes.ToList();
            rForms.DataBind();

            btnDelete.Enabled = !workflowTypes.Any();
            if ( workflowTypes.Any() )
            {
                btnDelete.ToolTip = "All forms must be removed to enable deleting a category.";
            }
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
            tbHighlightColor.Text = category.HighlightColor;
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
            /// Date Created
            /// </summary>
            [Description( "Date Created" )]
            DateCreated = 0,

            /// <summary>
            /// Name
            /// </summary>
            [Description( "Name" )]
            Name = 1,

            /// <summary>
            /// Submission Count
            /// </summary>
            [Description( "Submission Count" )]
            SubmissionCount = 2
        }

        #endregion
    }
}