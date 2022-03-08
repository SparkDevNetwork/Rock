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
                    lbAddCategoryRoot.Enabled = true;
                    lbAddCategoryChild.Enabled = false;
                    var category = FindFirstCategory( cachedEntityType.Id );
                    if ( category != null )
                    {
                        hfSelectedCategory.Value = category.Id.ToString();
                    }

                    btnSecurity.EntityTypeId = category.EntityTypeId;

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

        /// <summary>
        /// Handles the Click event of the lbAddForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddForm_Click( object sender, EventArgs e )
        {
            pnlAddForm.Visible = true;
            pnlFormList.Visible = false;

        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryChild_Click( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryRoot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryRoot_Click( object sender, EventArgs e )
        {
            // if a rootCategory is set, set that as the parentCategory when they select "add top-level"
        }

        /// <summary>
        /// Handles the Click event of the btnStartBuilding control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnStartBuilding_Click( object sender, EventArgs e )
        {
            ListCategoryForms();
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
        /// Lists the form by categories.
        /// </summary>
        protected void ListCategoryForms()
        {
            pnlAddForm.Visible = false;
            pnlFormList.Visible = true;
            var categoryId = hfSelectedCategory.ValueAsInt();
            var rockContext = new RockContext();
            var category = new CategoryService( rockContext ).Get( categoryId );
            if ( category != null )
            {
                lTitle.Text = $"{category.Name} Form List";
                lDescription.Text = $"Below are a list of forms for the {category.Name} Category";
                btnSecurity.EntityId = category.Id;
            }
        }

        #endregion
    }
}