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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// </summary>
    [DisplayName( "Signature Document Template List" )]
    [Category( "Core" )]
    [Description( "Lists all the signature document templates and allows for managing them." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "2E413152-B790-4EC2-84A9-9B48D2717D63" )]
    public partial class SignatureDocumentTemplateList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gSignatureDocumentTemplate.DataKeyNames = new string[] { "Id" };
            gSignatureDocumentTemplate.Actions.ShowAdd = true;
            gSignatureDocumentTemplate.Actions.AddClick += gSignatureDocumentTemplate_Add;
            gSignatureDocumentTemplate.GridRebind += gSignatureDocumentTemplate_GridRebind;

            SecurityField securityField = gSignatureDocumentTemplate.ColumnsOfType<SecurityField>().First();
            securityField.EntityTypeId = EntityTypeCache.GetId<Rock.Model.SignatureDocumentTemplate>().Value;

            bool editAllowed = this.UserCanEdit;
            gSignatureDocumentTemplate.Actions.ShowAdd = editAllowed;
            gSignatureDocumentTemplate.IsDeleteEnabled = editAllowed;
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

        #endregion Control Methods

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gSignatureDocumentTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocumentTemplate_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "SignatureDocumentTemplateId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSignatureDocumentTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSignatureDocumentTemplate_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "SignatureDocumentTemplateId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSignatureDocumentTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocumentTemplate_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var signatureDocumentService = new SignatureDocumentService( rockContext );
            var signatureDocumentTemplateService = new SignatureDocumentTemplateService( rockContext );

            SignatureDocumentTemplate type = signatureDocumentTemplateService.Get( e.RowKeyId );

            if ( type != null )
            {
                if ( !type.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    mdGridWarning.Show( "Sorry, you're not authorized to delete this signature document template.", ModalAlertType.Alert );
                    return;
                }

                string errorMessage;
                if ( !signatureDocumentTemplateService.CanDelete( type, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                signatureDocumentTemplateService.Delete( type );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSignatureDocumentTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocumentTemplate_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Grid Events (main grid)

        #region Internal Methods

        /// <summary>
        /// Binds the grid for signature document templates.
        /// </summary>
        private void BindGrid()
        {
            var templateQry = new SignatureDocumentTemplateService( new RockContext() ).Queryable();

            SortProperty sortProperty = gSignatureDocumentTemplate.SortProperty;
            if ( sortProperty == null )
            {
                templateQry = templateQry.OrderBy( a => a.Name );
            }
            else if ( sortProperty.Property != "Documents" )
            {
                templateQry = templateQry.Sort( sortProperty );
            }

            var templates = templateQry.Select( t => new SignatureDocumentTemplateViewModel { Template = t, DocumentCount = t.Documents.Count() } ).ToList();

            if ( sortProperty != null && sortProperty.Property == "Documents" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    templates = templates.OrderBy( t => t.DocumentCount ).ToList();
                }
                else
                {
                    templates = templates.OrderByDescending( a => a.DocumentCount ).ToList();
                }
            }


            var authorizedTemplates = new List<SignatureDocumentTemplateViewModel>();
            foreach ( var template in templates )
            {
                if ( template.Template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    authorizedTemplates.Add( template );
                }
            }

            gSignatureDocumentTemplate.DataSource = authorizedTemplates
                .Select( a =>
                new
                {
                    a.Template.Id,
                    a.Template.Name,
                    a.Template.Description,
                    BinaryFileType = a.Template.BinaryFileType.Name,
                    a.Template.ProviderTemplateKey,
                    Documents = a.DocumentCount
                } )
                .ToList();

            gSignatureDocumentTemplate.DataBind();
        }

        #endregion Internal Methods
    }

    #region Helper Class

    /// <summary>
    /// Viewmodel that adds DocumentCount to <see cref="SignatureDocumentTemplate"/>.
    /// </summary>
    public class SignatureDocumentTemplateViewModel
    {
        public SignatureDocumentTemplate Template { get; set; }
        public int DocumentCount { get; set; }
    }

    #endregion Helper Class
}