// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Merge Template List" )]
    [Category( "Core" )]
    [Description( "Displays a list of all merge templates." )]

    [LinkedPage( "Detail Page" )]
    [EnumField( "Merge Templates Ownership", "Set this to limit to merge templates depending on ownership type", typeof( MergeTemplateOwnership ), true, "Personal" )]
    public partial class MergeTemplateList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMergeTemplates.DataKeyNames = new string[] { "Id" };
            gMergeTemplates.Actions.AddClick += gMergeTemplates_Add;
            gMergeTemplates.GridRebind += gMergeTemplates_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gMergeTemplates.Actions.ShowAdd = canAddEditDelete;
            gMergeTemplates.IsDeleteEnabled = canAddEditDelete;

            this.BlockUpdated += MergeTemplateList_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upMergeTemplateList );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the MergeTemplateList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void MergeTemplateList_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
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

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gMergeTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMergeTemplates_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "MergeTemplateId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMergeTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMergeTemplates_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "MergeTemplateId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gMergeTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMergeTemplates_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            MergeTemplateService service = new MergeTemplateService( rockContext );
            MergeTemplate item = service.Get( e.RowKeyId );
            if ( item != null )
            {
                string errorMessage;
                if ( !service.CanDelete( item, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( item );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMergeTemplates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMergeTemplates_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var service = new MergeTemplateService( new RockContext() );
            SortProperty sortProperty = gMergeTemplates.SortProperty;

            var qry = service.Queryable();

            var mergeTemplateOwnership = this.GetAttributeValue( "MergeTemplatesOwnership" ).ConvertToEnum<MergeTemplateOwnership>( MergeTemplateOwnership.Personal );

            var personColumn = gMergeTemplates.Columns.OfType<PersonField>().FirstOrDefault();
            personColumn.Visible = false;

            if ( this.IsUserAuthorized( Authorization.EDIT ) )
            {
                // show all merge templates regardless of block settings
                personColumn.Visible = true;
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Personal )
            {
                qry = qry.Where( a => a.PersonAliasId == this.CurrentPersonAliasId );
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.PersonalAndGlobal )
            {
                qry = qry.Where( a => a.PersonAliasId == this.CurrentPersonAliasId || !a.PersonAliasId.HasValue );
                personColumn.Visible = true;
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Global )
            {
                qry = qry.Where( a => !a.PersonAliasId.HasValue );
            }

            if ( sortProperty != null )
            {
                gMergeTemplates.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gMergeTemplates.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }

            gMergeTemplates.DataBind();
        }

        #endregion
    }
}