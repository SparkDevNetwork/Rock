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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            if ( Page.IsPostBack )
            {
                // rebuild the Table Controls (unless this is postback from ddlBatch)
                if ( this.Request.Params["__EVENTTARGET"] != ddlBatch.UniqueID )
                {
                    // Bind the Grid so that the dynamic controls for the Entity get created on every postback
                    CreateTableControls( this.Request.Form[hfBatchId.UniqueID].AsIntegerOrNull() );
                }
            }
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
                if ( _transactionEntityType != null )
                {
                    lEntityHeaderText.Text = _transactionEntityType.FriendlyName;
                }

                LoadDropDowns();
                CreateTableControls( null );
            }
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
        }

        /// <summary>
        /// Loads the group drop downs.
        /// </summary>
        private void LoadGroupDropDowns()
        {
            int? groupTypeId = this.GetAttributeValue( "EntityTypeQualifierValue" ).AsIntegerOrNull();
            var rockContext = new RockContext();

            List<Group> groupList = null;
            if ( groupTypeId.HasValue && _transactionEntityType != null && _transactionEntityType.Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() )
            {
                groupList = new GroupService( new RockContext() ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.Members.Any() ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).AsNoTracking().ToList();

                foreach ( var ddlGroup in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlGroup_" ) ) )
                {
                    ddlGroup.Items.Clear();
                    ddlGroup.Items.Add( new ListItem() );
                    foreach ( var group in groupList )
                    {
                        ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    }

                    var financialTransactionDetailId = ddlGroup.ID.Replace( "ddlGroup_", string.Empty ).AsInteger();
                    var ddlGroupMember = phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID == "ddlGroupMember_" + financialTransactionDetailId.ToString() ).FirstOrDefault() as RockDropDownList;

                    var groupMemberId = new FinancialTransactionDetailService( rockContext ).Queryable().Where( a => a.Id == financialTransactionDetailId ).Select( a => (int?)a.EntityId ).FirstOrDefault();
                    if ( groupMemberId.HasValue )
                    {
                        var groupMember = new GroupMemberService( rockContext ).Get( groupMemberId.Value );
                        if ( groupMember != null )
                        {
                            ddlGroup.SetValue( groupMember.GroupId.ToString() );
                            LoadGroupMembersDropDown( ddlGroup );
                            ddlGroupMember.SetValue( groupMember.Id );
                        }
                    }

                    ddlGroupMember.Visible = ddlGroup.SelectedValue.AsIntegerOrNull().HasValue;
                }
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
            // intentionally blank
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBatch_SelectedIndexChanged( object sender, EventArgs e )
        {
            hfBatchId.Value = ddlBatch.SelectedValue;
            CreateTableControls( hfBatchId.Value.AsIntegerOrNull() );

            LoadGroupDropDowns();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the table controls.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        private void CreateTableControls( int? batchId )
        {
            RockContext rockContext = new RockContext();

            if ( batchId.HasValue )
            {
                var financialTransactionDetailQuery = new FinancialTransactionDetailService( rockContext ).Queryable();

                var financialTransactionDetailList = financialTransactionDetailQuery.Where( a => a.Transaction.BatchId == batchId.Value ).OrderByDescending( a => a.Transaction.TransactionDateTime ).ToList();
                phTableRows.Controls.Clear();
                foreach ( var financialTransactionDetail in financialTransactionDetailList )
                {
                    var tr = new HtmlGenericContainer( "tr" );
                    tr.Controls.Add( new LiteralControl { Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Transaction.AuthorizedPersonAlias ) } );
                    tr.Controls.Add( new LiteralControl { Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Amount.FormatAsCurrency() ) } );
                    tr.Controls.Add( new LiteralControl { Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Account ) } );
                    tr.Controls.Add( new LiteralControl
                    {
                        ID = "lTransactionType_" + financialTransactionDetail.Id.ToString(),
                        Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Transaction.TransactionTypeValue )
                    } );

                    var tdEntityControls = new HtmlGenericContainer( "td" ) { ID = "pnlEntityControls_" + financialTransactionDetail.Id.ToString() };

                    if ( _transactionEntityType != null && _transactionEntityType.Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() )
                    {
                        var ddlGroup = new RockDropDownList { ID = "ddlGroup_" + financialTransactionDetail.Id.ToString() };
                        ddlGroup.Label = "Group";
                        ddlGroup.AutoPostBack = true;
                        ddlGroup.SelectedIndexChanged += ddlGroup_SelectedIndexChanged;
                        tdEntityControls.Controls.Add( ddlGroup );
                        var ddlGroupMember = new RockDropDownList { ID = "ddlGroupMember_" + financialTransactionDetail.Id.ToString(), Visible = false };
                        ddlGroupMember.Label = "Group Member";
                        ddlGroupMember.AutoPostBack = true;
                        ddlGroupMember.SelectedIndexChanged += ddlGroupMember_SelectedIndexChanged;
                        tdEntityControls.Controls.Add( ddlGroupMember );
                        tr.Controls.Add( tdEntityControls );
                        phTableRows.Controls.Add( tr );
                    }

                    pnlTransactions.Visible = true;
                }
            }
            else
            {
                pnlTransactions.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ddlGroupMember_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlGroupMember = sender as RockDropDownList;

            // remember the selected group member so that we can set it again after items get rebuilt in the ddlGroup_SelectedIndexChanged event
            var groupMemberId = ddlGroupMember.SelectedValue.AsIntegerOrNull();

            var financialTransactionDetailId = ddlGroupMember.ID.Replace( "ddlGroupMember_", string.Empty ).AsIntegerOrNull();
            AssignGroupMemberToEntity( groupMemberId, financialTransactionDetailId );
        }

        /// <summary>
        /// Assigns the group member to entity.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="financialTransactionDetailId">The financial transaction detail identifier.</param>
        private void AssignGroupMemberToEntity( int? groupMemberId, int? financialTransactionDetailId )
        {
            if ( financialTransactionDetailId.HasValue )
            {
                var rockContext = new RockContext();
                var financialTransactionDetail = new FinancialTransactionDetailService( rockContext ).Get( financialTransactionDetailId.Value );
                financialTransactionDetail.EntityTypeId = _transactionEntityType.Id;
                financialTransactionDetail.EntityId = groupMemberId;
                int? blockTransactionTypeId = this.GetAttributeValue( "TransactionTypeId" ).AsIntegerOrNull();
                if ( blockTransactionTypeId.HasValue && blockTransactionTypeId != financialTransactionDetail.Transaction.TransactionTypeValueId )
                {
                    financialTransactionDetail.Transaction.TransactionTypeValueId = blockTransactionTypeId.Value;
                    var lTransactionType = phTableRows.ControlsOfTypeRecursive<LiteralControl>().Where( a => a.ID == "lTransactionType_" + financialTransactionDetail.Id.ToString() ).FirstOrDefault();
                    if ( lTransactionType != null )
                    {
                        lTransactionType.Text = string.Format( "<td>{0}</td>", DefinedValueCache.Read( financialTransactionDetail.Transaction.TransactionTypeValueId ) );
                    }
                }

                rockContext.SaveChanges();
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
                LoadGroupMembersDropDown( ddlGroup );

                var financialTransactionDetailId = ddlGroup.ID.Replace( "ddlGroup_", string.Empty ).AsIntegerOrNull();
                AssignGroupMemberToEntity( null, financialTransactionDetailId );
            }
        }

        /// <summary>
        /// Loads the group members drop down.
        /// </summary>
        /// <param name="ddlGroup">The DDL group.</param>
        /// <param name="groupId">The group identifier.</param>
        private void LoadGroupMembersDropDown( RockDropDownList ddlGroup )
        {
            var ddlGroupMember = ddlGroup.Parent.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.ID.StartsWith( "ddlGroupMember_" ) ) as RockDropDownList;
            if ( ddlGroupMember != null )
            {
                int? groupId = ddlGroup.SelectedValue.AsIntegerOrNull();
                ddlGroupMember.Items.Clear();
                ddlGroupMember.Items.Add( new ListItem() );
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
                }

                ddlGroupMember.Visible = groupId.HasValue;
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
    }
}