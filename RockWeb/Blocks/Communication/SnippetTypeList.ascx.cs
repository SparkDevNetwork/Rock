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
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// Lists Snippet Types
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    /// <seealso cref="SnippetType" />
    [DisplayName( "Snippet Type List" )]
    [Category( "Communication" )]
    [Description( "Shows a list of all snippet types." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "74BF9B7D-0F1C-4632-ACBB-A3E30061A237" )]
    public partial class SnippetTypeList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Key for the detail page
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Keys for page parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The document type identifier
            /// </summary>
            public const string SnippetTypeId = "SnippetTypeId";
        }

        #endregion Keys

        #region Lifecycle Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeGrid();

            // Set up Block Settings change notification
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upSnippetTypes );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            InitializeGrid();
            BindGrid();
        }

        #endregion Lifecycle Events

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gSnippetTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gSnippetTypes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.SnippetTypeId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSnippetTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSnippetTypes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.SnippetTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSnippetTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSnippetTypes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var snippetTypeService = GetSnippetTypeService();

            var snippetTypeId = e.RowKeyId;
            var snippetType = snippetTypeService.Get( snippetTypeId );

            if ( snippetType == null )
            {
                mdGridWarning.Show( "The snippet type could not be found.", ModalAlertType.Information );
                return;
            }

            if ( !snippetTypeService.CanDelete( snippetType, out string errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            snippetTypeService.Delete( snippetType );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSnippetTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSnippetTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var snippetTypeQuery = GetSnippetTypeService()
                .Queryable()
                .AsNoTracking();

            // Apply sorting if enabled
            var sortProperty = gSnippetTypes.SortProperty;
            if ( gSnippetTypes.AllowSorting && sortProperty != null )
            {
                snippetTypeQuery = snippetTypeQuery.Sort( sortProperty );
            }
            else
            {
                snippetTypeQuery = snippetTypeQuery.OrderBy( st => st.Name )
                    .ThenBy( st => st.Id );
            }

            gSnippetTypes.SetLinqDataSource( snippetTypeQuery );
            gSnippetTypes.DataBind();
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            gSnippetTypes.DataKeyNames = new string[] { "Id" };
            gSnippetTypes.GridRebind += gSnippetTypes_GridRebind;
            gSnippetTypes.RowItemText = "Snippet Type";

            var isUserAuthorized = IsUserAuthorized( Authorization.EDIT );
            var isDetailPageSet = IsDetailPageSet();

            var canDelete = isUserAuthorized;
            var canAddAndEdit = isUserAuthorized && isDetailPageSet;

            gSnippetTypes.Actions.ShowAdd = canAddAndEdit;
            gSnippetTypes.IsDeleteEnabled = canDelete;

            if ( canAddAndEdit )
            {
                gSnippetTypes.Actions.AddClick += gSnippetTypes_Add;
                gSnippetTypes.RowSelected += gSnippetTypes_Edit;
            }

            var securityField = gSnippetTypes.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( SnippetType ) ).Id;
            }
        }

        /// <summary>
        /// Determines whether the detail page attribute has a value.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is detail page set]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDetailPageSet()
        {
            return !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
        }

        #endregion Internal Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            return _rockContext ?? ( _rockContext = new RockContext() );
        }

        private RockContext _rockContext = null;

        /// <summary>
        /// Get the snippet type service
        /// </summary>
        /// <returns></returns>
        private SnippetTypeService GetSnippetTypeService()
        {
            if ( _snippetTypeService == null )
            {
                var rockContext = GetRockContext();
                _snippetTypeService = new SnippetTypeService( rockContext );
            }

            return _snippetTypeService;
        }

        private SnippetTypeService _snippetTypeService = null;

        #endregion Data Interface Methods
    }
}