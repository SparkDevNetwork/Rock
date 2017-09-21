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
using Rock.Field;
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

    [EntityTypeField( "EntityTypeGuid", category: "CustomSetting" )]
    [TextField( "EntityTypeQualifierColumn", category: "CustomSetting" )]
    [TextField( "EntityTypeQualifierValue", category: "CustomSetting" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "TransactionTypeGuid", category: "CustomSetting" )]
    public partial class TransactionEntityMatching : RockBlockCustomSettings
    {
        /// <summary>
        /// Gets the type of the transaction entity.
        /// </summary>
        /// <value>
        /// The type of the transaction entity.
        /// </value>
        private EntityTypeCache _transactionEntityType
        {
            get
            {
                Guid? entityTypeGuid = this.GetAttributeValue( "EntityTypeGuid" ).AsGuidOrNull();
                if ( entityTypeGuid.HasValue )
                {
                    return EntityTypeCache.Read( entityTypeGuid.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            nbBlockConfigurationWarning.Visible = _transactionEntityType == null;
            lPanelTitle.Text = _transactionEntityType != null ? _transactionEntityType.FriendlyName + " Matching" : "Matching";

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
        /// Loads the entity drop downs.
        /// </summary>
        private void LoadEntityDropDowns()
        {
            int? entityTypeQualifierValue = this.GetAttributeValue( "EntityTypeQualifierValue" ).AsIntegerOrNull();
            var rockContext = new RockContext();

            if ( _transactionEntityType != null )
            {
                if ( _transactionEntityType.Id == EntityTypeCache.GetId<GroupMember>() )
                {
                    int? groupTypeId = entityTypeQualifierValue;
                    List<Group> groupsWithMembersList = new GroupService( new RockContext() ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.Members.Any() && a.IsActive ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).AsNoTracking().ToList();

                    foreach ( var ddlGroup in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlGroup_" ) ) )
                    {
                        ddlGroup.Items.Clear();
                        ddlGroup.Items.Add( new ListItem() );
                        foreach ( var group in groupsWithMembersList )
                        {
                            ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                        }

                        var financialTransactionDetailId = ddlGroup.ID.Replace( "ddlGroup_", string.Empty ).AsInteger();
                        var ddlGroupMember = phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID == "ddlGroupMember_" + financialTransactionDetailId.ToString() ).FirstOrDefault() as RockDropDownList;

                        var groupMemberId = new FinancialTransactionDetailService( rockContext ).Queryable().Where( a => a.Id == financialTransactionDetailId && a.EntityTypeId == _transactionEntityType.Id ).Select( a => (int?)a.EntityId ).FirstOrDefault();
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
                else if ( _transactionEntityType.Id == EntityTypeCache.GetId<Group>() )
                {
                    int? groupTypeId = entityTypeQualifierValue;
                    List<Group> groupList = new GroupService( new RockContext() ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.IsActive ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).AsNoTracking().ToList();

                    foreach ( var ddlGroup in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlGroup_" ) ) )
                    {
                        ddlGroup.Items.Clear();
                        ddlGroup.Items.Add( new ListItem() );
                        foreach ( var group in groupList )
                        {
                            ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                        }

                        var financialTransactionDetailId = ddlGroup.ID.Replace( "ddlGroup_", string.Empty ).AsInteger();

                        var groupId = new FinancialTransactionDetailService( rockContext ).Queryable().Where( a => a.Id == financialTransactionDetailId && a.EntityTypeId == _transactionEntityType.Id ).Select( a => (int?)a.EntityId ).FirstOrDefault();
                        if ( groupId.HasValue )
                        {
                            ddlGroup.SetValue( groupId );
                        }
                    }
                }
                else if ( _transactionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                {
                    int? definedTypeId = entityTypeQualifierValue;
                    var definedValueList = DefinedTypeCache.Read( definedTypeId.Value ).DefinedValues;

                    foreach ( var ddlDefinedValue in phTableRows.ControlsOfTypeRecursive<DefinedValuePicker>().Where( a => a.ID.StartsWith( "ddlDefinedValue_" ) ) )
                    {
                        ddlDefinedValue.DefinedTypeId = definedTypeId;

                        var financialTransactionDetailId = ddlDefinedValue.ID.Replace( "ddlDefinedValue_", string.Empty ).AsInteger();

                        var definedValueId = new FinancialTransactionDetailService( rockContext ).Queryable().Where( a => a.Id == financialTransactionDetailId && a.EntityTypeId == _transactionEntityType.Id ).Select( a => (int?)a.EntityId ).FirstOrDefault();
                        if ( definedValueId.HasValue )
                        {
                            ddlDefinedValue.SetValue( definedValueId );
                        }
                    }
                }
                else if ( _transactionEntityType.SingleValueFieldType != null && _transactionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    foreach ( var entityPicker in phTableRows.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "entityPicker_" ) ) )
                    {
                        var financialTransactionDetailId = entityPicker.ID.Replace( "entityPicker_", string.Empty ).AsInteger();

                        var entityId = new FinancialTransactionDetailService( rockContext ).Queryable().Where( a => a.Id == financialTransactionDetailId && a.EntityTypeId == _transactionEntityType.Id ).Select( a => (int?)a.EntityId ).FirstOrDefault();

                        ( _transactionEntityType.SingleValueFieldType.Field as IEntityFieldType ).SetEditValueFromEntityId( entityPicker, new Dictionary<string, ConfigurationValue>(), entityId );
                    }
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
            ddlBatch_SelectedIndexChanged( sender, e );
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

            LoadEntityDropDowns();
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
            nbSaveSuccess.Visible = false;
            btnSave.Visible = false;

            if ( _transactionEntityType != null )
            {
                lEntityHeaderText.Text = _transactionEntityType.FriendlyName;
            }

            if ( batchId.HasValue )
            {
                var financialTransactionDetailQuery = new FinancialTransactionDetailService( rockContext ).Queryable();

                var financialTransactionDetailList = financialTransactionDetailQuery.Where( a => a.Transaction.BatchId == batchId.Value ).OrderByDescending( a => a.Transaction.TransactionDateTime ).ToList();
                phTableRows.Controls.Clear();
                btnSave.Visible = financialTransactionDetailList.Any();
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
                    tr.Controls.Add( tdEntityControls );

                    if ( _transactionEntityType != null )
                    {
                        if ( _transactionEntityType.Id == EntityTypeCache.GetId<GroupMember>() )
                        {
                            var ddlGroup = new RockDropDownList { ID = "ddlGroup_" + financialTransactionDetail.Id.ToString() };
                            ddlGroup.Label = "Group";
                            ddlGroup.AutoPostBack = true;
                            ddlGroup.SelectedIndexChanged += ddlGroup_SelectedIndexChanged;
                            tdEntityControls.Controls.Add( ddlGroup );
                            var ddlGroupMember = new RockDropDownList { ID = "ddlGroupMember_" + financialTransactionDetail.Id.ToString(), Visible = false };
                            ddlGroupMember.Label = "Group Member";
                            tdEntityControls.Controls.Add( ddlGroupMember );
                        }
                        else if ( _transactionEntityType.Id == EntityTypeCache.GetId<Group>() )
                        {
                            var ddlGroup = new RockDropDownList { ID = "ddlGroup_" + financialTransactionDetail.Id.ToString() };
                            ddlGroup.AutoPostBack = true;
                            ddlGroup.SelectedIndexChanged += ddlGroup_SelectedIndexChanged;
                            tdEntityControls.Controls.Add( ddlGroup );
                        }
                        else if ( _transactionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                        {
                            var ddlDefinedValue = new DefinedValuePicker { ID = "ddlDefinedValue_" + financialTransactionDetail.Id.ToString() };
                            tdEntityControls.Controls.Add( ddlDefinedValue );
                        }
                        else if ( _transactionEntityType.SingleValueFieldType != null )
                        {
                            var entityPicker = _transactionEntityType.SingleValueFieldType.Field.EditControl( new Dictionary<string, Rock.Field.ConfigurationValue>(), "entityPicker_" + financialTransactionDetail.Id.ToString() );
                            tdEntityControls.Controls.Add( entityPicker );
                        }
                    }

                    phTableRows.Controls.Add( tr );

                    pnlTransactions.Visible = true;
                }
            }
            else
            {
                pnlTransactions.Visible = false;
            }
        }

        /// <summary>
        /// Assigns the entity to transaction detail.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="financialTransactionDetailId">The financial transaction detail identifier.</param>
        private void AssignEntityToTransactionDetail( int? entityId, int? financialTransactionDetailId )
        {
            if ( financialTransactionDetailId.HasValue )
            {
                var rockContext = new RockContext();
                var financialTransactionDetail = new FinancialTransactionDetailService( rockContext ).Get( financialTransactionDetailId.Value );
                financialTransactionDetail.EntityTypeId = _transactionEntityType.Id;
                financialTransactionDetail.EntityId = entityId;

                DefinedValueCache blockTransactionType = null;
                Guid? blockTransactionTypeGuid = this.GetAttributeValue( "TransactionTypeGuid" ).AsGuidOrNull();
                if ( blockTransactionTypeGuid.HasValue )
                {
                    blockTransactionType = DefinedValueCache.Read( blockTransactionTypeGuid.Value );
                }

                if ( blockTransactionType != null && blockTransactionType.Id != financialTransactionDetail.Transaction.TransactionTypeValueId )
                {
                    financialTransactionDetail.Transaction.TransactionTypeValueId = blockTransactionType.Id;
                    var lTransactionType = phTableRows.ControlsOfTypeRecursive<LiteralControl>().Where( a => a.ID == "lTransactionType_" + financialTransactionDetail.Id.ToString() ).FirstOrDefault();
                    if ( lTransactionType != null )
                    {
                        lTransactionType.Text = string.Format( "<td>{0}</td>", DefinedValueCache.Read( financialTransactionDetail.Transaction.TransactionTypeValueId ) );
                    }
                }

                rockContext.SaveChanges();

                var btnSaveEntity = phTableRows.ControlsOfTypeRecursive<LinkButton>().Where( a => a.ID == "btnSaveEntity_" + financialTransactionDetail.Id.ToString() ).FirstOrDefault();
                if ( btnSaveEntity != null )
                {
                    btnSaveEntity.ToolTip = "Last Modified at " + financialTransactionDetail.ModifiedDateTime.ToString();
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
                if ( _transactionEntityType.Id == EntityTypeCache.GetId<GroupMember>() )
                {
                    LoadGroupMembersDropDown( ddlGroup );
                }
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

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( _transactionEntityType != null )
            {
                if ( _transactionEntityType.Id == EntityTypeCache.GetId<GroupMember>() )
                {
                    foreach ( var ddlGroupMember in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlGroupMember_" ) ) )
                    {
                        int? financialTransactionDetailId = ddlGroupMember.ID.Replace( "ddlGroupMember_", string.Empty ).AsInteger();
                        int? groupMemberId = ddlGroupMember.SelectedValue.AsIntegerOrNull();
                        AssignEntityToTransactionDetail( groupMemberId, financialTransactionDetailId );
                    }
                }
                else if ( _transactionEntityType.Id == EntityTypeCache.GetId<Group>() )
                {
                    foreach ( var ddlGroup in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlGroup_" ) ) )
                    {
                        int? financialTransactionDetailId = ddlGroup.ID.Replace( "ddlGroup_", string.Empty ).AsInteger();
                        int? groupId = ddlGroup.SelectedValue.AsIntegerOrNull();
                        AssignEntityToTransactionDetail( groupId, financialTransactionDetailId );
                    }
                }
                else if ( _transactionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                {
                    foreach ( var ddlDefinedValue in phTableRows.ControlsOfTypeRecursive<DefinedValuePicker>().Where( a => a.ID.StartsWith( "ddlDefinedValue_" ) ) )
                    {
                        int? financialTransactionDetailId = ddlDefinedValue.ID.Replace( "ddlDefinedValue_", string.Empty ).AsInteger();
                        int? definedValueId = ddlDefinedValue.SelectedValue.AsIntegerOrNull();
                        AssignEntityToTransactionDetail( definedValueId, financialTransactionDetailId );
                    }
                }
                else if ( _transactionEntityType.SingleValueFieldType != null )
                {
                    foreach ( var entityPicker in phTableRows.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "entityPicker_" ) ) )
                    {
                        var entityFieldType = _transactionEntityType.SingleValueFieldType.Field as IEntityFieldType;
                        if ( entityFieldType != null )
                        {
                            int? financialTransactionDetailId = entityPicker.ID.Replace( "entityPicker_", string.Empty ).AsIntegerOrNull();
                            if ( financialTransactionDetailId.HasValue )
                            {
                                int? entityId = entityFieldType.GetEditValueAsEntityId( entityPicker, new Dictionary<string, ConfigurationValue>() );
                                AssignEntityToTransactionDetail( entityId, financialTransactionDetailId );
                            }
                        }
                    }
                }
            }

            nbSaveSuccess.Visible = true;
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

            DefinedValueCache blockTransactionType = null;
            Guid? blockTransactionTypeGuid = this.GetAttributeValue( "TransactionTypeGuid" ).AsGuidOrNull();
            if ( blockTransactionTypeGuid.HasValue )
            {
                blockTransactionType = DefinedValueCache.Read( blockTransactionTypeGuid.Value );
            }

            ddlTransactionType.SetValue( blockTransactionType != null ? blockTransactionType.Id : (int?)null );

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

            var entityTypeGuid = this.GetAttributeValue( "EntityTypeGuid" ).AsGuidOrNull();
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();
            etpEntityType.EntityTypes = new EntityTypeService( rockContext ).Queryable().Where( a => ( a.IsEntity && a.SingleValueFieldTypeId.HasValue ) || ( a.Id == entityTypeIdGroupMember ) ).OrderBy( t => t.FriendlyName ).AsNoTracking().ToList();

            if ( entityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Read( entityTypeGuid.Value );
                etpEntityType.SetValue( entityType != null ? entityType.Id : (int?)null );
            }

            UpdateControlsForEntityType();

            tbEntityTypeQualifierColumn.Text = this.GetAttributeValue( "EntityTypeQualifierColumn" );

            gtpGroupType.SetValue( this.GetAttributeValue( "EntityTypeQualifierValue" ) );
            ddlDefinedTypePicker.SetValue( this.GetAttributeValue( "EntityTypeQualifierValue" ) );
            tbEntityTypeQualifierValue.Text = this.GetAttributeValue( "EntityTypeQualifierValue" );

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            Guid? entityTypeGuid = null;
            if ( etpEntityType.SelectedEntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read( etpEntityType.SelectedEntityTypeId.Value );
                if ( entityType != null )
                {
                    entityTypeGuid = entityType.Guid;
                }
            }

            this.SetAttributeValue( "EntityTypeGuid", entityTypeGuid.ToString() );

            DefinedValueCache blockTransactionType = null;
            int? selectedTransactionTypeId = ddlTransactionType.SelectedValue.AsIntegerOrNull();
            if ( selectedTransactionTypeId.HasValue )
            {
                blockTransactionType = DefinedValueCache.Read( selectedTransactionTypeId.Value );
            }

            this.SetAttributeValue( "TransactionTypeGuid", blockTransactionType != null ? blockTransactionType.Guid.ToString() : null );
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

            // reload the page to make sure we have a clean load with the correct entityType, etc
            NavigateToCurrentPage();
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
                        tbEntityTypeQualifierColumn.Text = string.Empty;
                        tbEntityTypeQualifierValue.Text = string.Empty;
                        tbEntityTypeQualifierColumn.Visible = true;
                        tbEntityTypeQualifierValue.Visible = true;
                    }
                }
            }
        }

        #endregion
    }
}