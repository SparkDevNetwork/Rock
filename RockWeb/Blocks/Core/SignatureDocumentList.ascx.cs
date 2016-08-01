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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing signature documents
    /// </summary>
    [DisplayName( "Signature Document List" )]
    [Category( "Core" )]
    [Description( "Block for viewing values for a signature document type." )]

    [LinkedPage( "Detail Page" )]
    [ContextAware( typeof( Person ) )]
    public partial class SignatureDocumentList : RockBlock, ISecondaryBlock
    {
        #region Properties 

        protected Person TargetPerson { get; private set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSettings );

            // in case this is used as a Person Block, set the TargetPerson 
            TargetPerson = ContextEntity<Person>();

            gSignatureDocuments.DataKeyNames = new string[] { "Id" };
            gSignatureDocuments.Actions.ShowAdd = true;
            gSignatureDocuments.Actions.AddClick += gSignatureDocuments_Add;
            gSignatureDocuments.GridRebind += gSignatureDocuments_GridRebind;
            gSignatureDocuments.Actions.ShowAdd = true;
            gSignatureDocuments.IsDeleteEnabled = true;
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
                BindGrid();
            }
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
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gSignatureDocuments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocuments_Add( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "SignatureDocumentTemplateId", PageParameter( "SignatureDocumentTemplateId" ) );
            qryParams.Add( "signatureDocumentId", "0" );
            if ( TargetPerson != null )
            {
                qryParams.Add( "personId", TargetPerson.Id.ToString() );
            }
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Edit event of the gSignatureDocuments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocuments_Edit( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "SignatureDocumentTemplateId", PageParameter( "SignatureDocumentTemplateId" ) );
            qryParams.Add( "signatureDocumentId", e.RowKeyId.ToString() );
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Delete event of the gSignatureDocuments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocuments_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var signatureDocumentService = new SignatureDocumentService( rockContext );

            SignatureDocument document = signatureDocumentService.Get( e.RowKeyId );

            if ( document != null )
            {
                if ( !UserCanEdit && !document.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    mdGridWarningValues.Show( "Sorry, you're not authorized to delete this signature document.", ModalAlertType.Alert );
                    return;
                }

                string errorMessage;
                if ( !signatureDocumentService.CanDelete( document, out errorMessage ) )
                {
                    mdGridWarningValues.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                signatureDocumentService.Delete( document );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSignatureDocuments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSignatureDocuments_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the signature documents grid.
        /// </summary>
        protected void BindGrid()
        {
            var qry = new SignatureDocumentService( new RockContext() )
                .Queryable().AsNoTracking();

            if ( TargetPerson != null )
            {
                qry = qry.Where( d =>
                    ( d.AppliesToPersonAlias != null && d.AppliesToPersonAlias.PersonId == TargetPerson.Id ) ||
                    ( d.AssignedToPersonAlias != null && d.AssignedToPersonAlias.PersonId == TargetPerson.Id ) ||
                    ( d.SignedByPersonAlias != null && d.SignedByPersonAlias.PersonId == TargetPerson.Id ) );
            }
            else
            {
                int? documentTypeId = PageParameter( "SignatureDocumentTemplateId" ).AsIntegerOrNull();
                if ( documentTypeId.HasValue )
                {
                    qry = qry.Where( d =>
                        d.SignatureDocumentTemplateId == documentTypeId.Value );

                    var typeColumn = gSignatureDocuments.ColumnsOfType<RockBoundField>().Where( f => f.HeaderText == "Document Type" ).First();
                    typeColumn.Visible = false;
                }
            }

            SortProperty sortProperty = gSignatureDocuments.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( d => d.LastInviteDate );
            }

            gSignatureDocuments.DataSource = qry.Select( d => new
            {
                d.Id,
                d.Name,
                d.DocumentKey,
                d.AppliesToPersonAlias,
                d.AssignedToPersonAlias,
                d.SignedByPersonAlias,
                d.Status,
                d.LastInviteDate,
                d.SignatureDocumentTemplate,
                FileText = d.BinaryFileId.HasValue ? "<i class='fa fa-file-text-o fa-lg'></i>" : "",
                FileId = d.BinaryFileId ?? 0
            } ).ToList();
            gSignatureDocuments.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}