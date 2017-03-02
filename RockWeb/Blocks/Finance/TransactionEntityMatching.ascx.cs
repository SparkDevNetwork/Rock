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
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

/// <summary>
/// 
/// </summary>
namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Transaction Entity Matching" )]
    [Category( "Finance" )]
    [Description( "Used to assign an Entity to a Transaction Detail record" )]

    [EntityTypeField( "EntityTypeId", category: "CustomSetting" )]
    [TextField( "EntityTypeQualifierColumn", category: "CustomSetting" )]
    [TextField( "EntityTypeQualifierValue", category: "CustomSetting" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "TransactionTypeId", category: "CustomSetting" )]
    public partial class TransactionEntityMatching : RockBlockCustomSettings
    {
        private EntityTypeCache _transactionEntityType = null;
        private List<ListItem> _groupListItems = null;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int? entityTypeId = this.GetAttributeValue( "EntityTypeId" ).AsIntegerOrNull();
            if ( entityTypeId.HasValue )
            {
                _transactionEntityType = EntityTypeCache.Read( entityTypeId.Value );
            }

            gTransactionDetails.GridRebind += GTransactionDetails_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            BindGrid(false);

            var ddls= gTransactionDetails.ControlsOfTypeRecursive<RockDropDownList>();

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                
            }
        }

        private void GTransactionDetails_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid(true);
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var rockContext = new RockContext();
            var financialBatchList = new FinancialBatchService( rockContext ).Queryable()
                .Where( a => a.Status == BatchStatus.Open ).OrderBy( a => a.Name ).Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            ddlBatch.Items.Clear();
            ddlBatch.Items.Add( new ListItem() );
            foreach ( var batch in financialBatchList )
            {
                ddlBatch.Items.Add( new ListItem( batch.Name, batch.Id.ToString() ) );
            }

            _groupListItems = new List<ListItem>();
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
            BindGrid(true);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBatch_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid(true);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid(bool update)
        {
            RockContext rockContext = new RockContext();

            int? batchId = ddlBatch.SelectedValue.AsIntegerOrNull();
            if ( batchId.HasValue )
            {
                var financialTransactionDetailQuery = new FinancialTransactionDetailService( rockContext ).Queryable();

                financialTransactionDetailQuery = financialTransactionDetailQuery.Where( a => a.Transaction.BatchId == batchId.Value ).OrderByDescending( a => a.Transaction.TransactionDateTime );

                gTransactionDetails.DataSource = financialTransactionDetailQuery.ToList();
                gTransactionDetails.DataBind();
            }
            else
            {
                gTransactionDetails.DataSource = null;
                gTransactionDetails.DataBind();
            }

            if (update)
            {
                upnlContent.Update();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gTransactionDetails_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            FinancialTransactionDetail financialTransactionDetail = e.Row.DataItem as FinancialTransactionDetail;
            if ( financialTransactionDetail != null )
            {
                var lPersonName = e.Row.FindControl( "lPersonName" ) as Literal;
                var lAmount = e.Row.FindControl( "lAmount" ) as Literal;
                var lAccount = e.Row.FindControl( "lAccount" ) as Literal;
                var upEntity = e.Row.FindControl( "upEntity" ) as UpdatePanel;
                if ( financialTransactionDetail.Transaction.AuthorizedPersonAliasId.HasValue )
                {
                    lPersonName.Text = financialTransactionDetail.Transaction.AuthorizedPersonAlias.ToString();
                }
                lAmount.Text = financialTransactionDetail.Amount.FormatAsCurrency();
                lAccount.Text = financialTransactionDetail.Account.Name;

                if ( _transactionEntityType != null && _transactionEntityType.Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() )
                {
                    var ddlGroup = new RockDropDownList();
                    ddlGroup.ID = "ddlGroup_" + financialTransactionDetail.Id.ToString();
                    ddlGroup.AutoPostBack = true;
                    ddlGroup.SelectedIndexChanged += ddlGroup_SelectedIndexChanged;
                    if ( _groupListItems == null )
                    {
                        int? groupTypeId = this.GetAttributeValue( "EntityTypeQualifierValue" ).AsIntegerOrNull();
                        if ( groupTypeId.HasValue )
                        {
                            _groupListItems = new GroupService( new RockContext() ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.Members.Any() ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( a => new
                            {
                                a.Id,
                                a.Name
                            } ).ToList().Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToList();
                        }
                    }

                    ddlGroup.Items.Add( new ListItem() );
                    ddlGroup.Items.AddRange( _groupListItems.ToArray() );
                    var ddlGroupMember = new RockDropDownList();
                    ddlGroupMember.ID = "ddlGroupMember";

                    //upEntity.ContentTemplateContainer.Controls.Clear();
                    upEntity.ContentTemplateContainer.Controls.Add( ddlGroup );
                    upEntity.ContentTemplateContainer.Controls.Add( ddlGroupMember );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlGroup = sender as RockDropDownList;
            if ( ddlGroup != null )
            {
                var groupId = this.Request.Form[ddlGroup.UniqueID].AsIntegerOrNull();
                var ddlGroupMember = ddlGroup.Parent.FindControl( "ddlGroupMember" ) as RockDropDownList;
                ddlGroup.SetValue( groupId );
                if ( ddlGroupMember != null )
                {
                    ddlGroupMember.Items.Clear();
                    ddlGroupMember.Items.Add(new ListItem());
                    if ( groupId.HasValue )
                    {
                        var groupMemberListItems = new GroupMemberService( new RockContext() ).Queryable().Where( a => a.GroupId == groupId.Value )
                            .OrderBy( a => a.Person.FirstName ).ThenBy( a => a.Person.LastName )
                            .Select( a => new
                            {
                                a.Id,
                                a.Person.SuffixValueId,
                                a.Person.NickName,
                                a.Person.LastName
                            } ).ToList().Select( a => new ListItem( Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId ), a.Id.ToString() ) );

                        ddlGroupMember.Items.AddRange( groupMemberListItems.ToArray() );
                        ddlGroupMember.ParentUpdatePanel().Update();
                    }
                }
            }
        }

        #endregion

        #region Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlSettings.Visible = true;

            ddlTransactionType.DefinedTypeId = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() ).Id;

            ddlTransactionType.SetValue( this.GetAttributeValue( "TransactionTypeId" ).AsIntegerOrNull() );
            var rockContext = new RockContext();

            gtpGroupType.GroupTypes = new GroupTypeService( rockContext ).Queryable().OrderBy( a => a.Order ).ThenBy( a => a.Name ).AsNoTracking().ToList();
            ddlDefinedTypePicker.Items.Clear();
            ddlDefinedTypePicker.Items.Add( new ListItem() );
            var definedTypesList = new DefinedTypeService( rockContext ).Queryable().OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            foreach ( var definedType in definedTypesList )
            {
                ddlDefinedTypePicker.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            var entityTypeId = this.GetAttributeValue( "EntityTypeId" ).AsIntegerOrNull();
            etpEntityType.EntityTypes = new EntityTypeService( rockContext ).Queryable().Where( a => a.IsEntity ).OrderBy( t => t.FriendlyName ).AsNoTracking().ToList();
            etpEntityType.SetValue( entityTypeId );
            UpdateControlsForEntityType();

            gtpGroupType.SetValue( this.GetAttributeValue( "EntityTypeQualifierValue" ) );
            ddlDefinedTypePicker.SetValue( this.GetAttributeValue( "EntityTypeQualifierValue" ) );
            tbEntityTypeQualifierColumn.Text = this.GetAttributeValue( "EntityTypeQualifierColumn" );

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            this.SetAttributeValue( "EntityTypeId", etpEntityType.SelectedEntityTypeId.ToString() );
            this.SetAttributeValue( "TransactionTypeId", ddlTransactionType.SelectedValue );
            this.SetAttributeValue( "EntityTypeQualifierColumn", tbEntityTypeQualifierColumn.Text );

            if ( ddlDefinedTypePicker.Visible )
            {
                this.SetAttributeValue( "EntityTypeQualifierValue", ddlDefinedTypePicker.SelectedValue );
            }
            else if ( gtpGroupType.Visible )
            {
                this.SetAttributeValue( "EntityTypeQualifierValue", gtpGroupType.SelectedValue );
            }
            else
            {
                this.SetAttributeValue( "EntityTypeQualifierValue", tbEntityTypeQualifierValue.Text );
            }

            this.SaveAttributeValues();

            mdSettings.Hide();
            pnlSettings.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the etpEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateControlsForEntityType();
        }

        /// <summary>
        /// Updates the type of the controls for entity.
        /// </summary>
        private void UpdateControlsForEntityType()
        {
            ddlDefinedTypePicker.Visible = false;
            gtpGroupType.Visible = false;
            tbEntityTypeQualifierColumn.ReadOnly = false;
            tbEntityTypeQualifierColumn.Visible = false;
            tbEntityTypeQualifierValue.Visible = false;

            if ( etpEntityType.SelectedEntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Read( etpEntityType.SelectedEntityTypeId.Value );
                if ( entityTypeCache != null )
                {
                    if ( entityTypeCache.Id == EntityTypeCache.GetId<Rock.Model.DefinedValue>() )
                    {
                        ddlDefinedTypePicker.Visible = true;
                        tbEntityTypeQualifierColumn.Text = "DefinedTypeId";
                        tbEntityTypeQualifierColumn.ReadOnly = true;
                        tbEntityTypeQualifierValue.Visible = false;
                    }
                    else if ( entityTypeCache.Id == EntityTypeCache.GetId<Rock.Model.GroupMember>() )
                    {
                        gtpGroupType.Visible = true;
                        tbEntityTypeQualifierColumn.Text = "GroupTypeId";
                        tbEntityTypeQualifierColumn.ReadOnly = true;
                        tbEntityTypeQualifierValue.Visible = false;
                    }
                    else if ( entityTypeCache.Id == EntityTypeCache.GetId<Rock.Model.Group>() )
                    {
                        gtpGroupType.Visible = true;
                        tbEntityTypeQualifierColumn.Text = "GroupTypeId";
                        tbEntityTypeQualifierColumn.ReadOnly = true;
                        tbEntityTypeQualifierValue.Visible = false;
                    }
                    else
                    {
                        tbEntityTypeQualifierColumn.ReadOnly = false;
                        tbEntityTypeQualifierColumn.Visible = true;
                        tbEntityTypeQualifierValue.Visible = true;
                    }
                }
            }
        }

        #endregion



        protected void gTransactionDetails_RowCommand( object sender, GridViewCommandEventArgs e )
        {

        }
    }
}