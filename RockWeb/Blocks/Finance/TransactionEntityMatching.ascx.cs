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
using System.Text;
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
    [TextField( "LimitToActiveGroups", category: "CustomSetting" )]
    [TextField( "Panel Title", "Set a specific title, or leave blank to have it based on the EntityType selection", required: false, order: 0 )]
    [TextField( "Entity Column Heading", "Set a column heading, or leave blank to have it based on the EntityType selection", required: false, order: 1 )]
    [BooleanField( "Show Dataview Filter", "Show a DataView filter that lists Dataviews that are based on Rock.Model.FinancialTranasactionDetail.", false, key: "ShowDataviewFilter", order: 2 )]
    [BooleanField( "Show Batch Filter", "", true, key: "ShowBatchFilter", order: 3 )]
    [IntegerField( "Max Number of Results", "", false, 1000, order: 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "TransactionTypeGuid", category: "CustomSetting" )]
    public partial class TransactionEntityMatching : RockBlockCustomSettings, ICustomGridColumns
    {
        private List<FinancialTransactionDetail> _financialTransactionDetailList;
        private int? _blockTransactionTypeId = null;

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
                    return EntityTypeCache.Get( entityTypeGuid.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the name of the entity type qualified.
        /// </summary>
        /// <value>
        /// The name of the entity type qualified.
        /// </value>
        private string _entityTypeQualifiedName
        {
            get
            {
                string result;
                if ( _transactionEntityType != null )
                {
                    result = _transactionEntityType.FriendlyName;
                    string entityTypeQualifierColumn = this.GetAttributeValue( "EntityTypeQualifierColumn" );
                    string entityTypeQualifierValue = this.GetAttributeValue( "EntityTypeQualifierValue" );
                    if ( entityTypeQualifierColumn == "GroupTypeId" )
                    {
                        var groupType = GroupTypeCache.Get( entityTypeQualifierValue.AsInteger() );
                        if ( groupType != null )
                        {
                            if ( _transactionEntityType.Guid == Rock.SystemGuid.EntityType.GROUP.AsGuid() )
                            {
                                result = groupType.Name;
                            }
                            else if ( _transactionEntityType.Guid == Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() )
                            {
                                //  result = groupType.Name + " " + groupType.GroupMemberTerm;
                            }
                        }
                    }
                    else if ( entityTypeQualifierColumn == "GroupId" )
                    {

                    }
                    else if ( entityTypeQualifierColumn == "DefinedTypeId" )
                    {
                        var definedType = DefinedTypeCache.Get( entityTypeQualifierValue.AsInteger() );
                        result = definedType.Name;
                    }
                }
                else
                {
                    result = null; 
                }

                return result;
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
            dvpDataView.EntityTypeId = EntityTypeCache.GetId<Rock.Model.FinancialTransactionDetail>();

            ApplyBlockProperties();

            if ( Page.IsPostBack )
            {
                // rebuild the Table Controls (unless this is postback from ddlBatch or dvpDataView)
                if ( this.Request.Params["__EVENTTARGET"] != ddlBatch.UniqueID && this.Request.Params["__EVENTTARGET"] != dvpDataView.UniqueID )
                {
                    // Bind the Grid so that the dynamic controls for the Entity get created on every postback
                    BindHtmlGrid( this.Request.Form[hfBatchId.UniqueID].AsIntegerOrNull(), this.Request.Form[hfDataViewId.UniqueID].AsIntegerOrNull() );
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
                hfBatchId.Value = this.GetBlockUserPreference( "BatchId" );
                ddlBatch.SetValue( hfBatchId.Value );
                hfDataViewId.Value = this.GetBlockUserPreference( "DataViewId" );
                dvpDataView.SetValue( hfDataViewId.Value.AsIntegerOrNull() );
                BindHtmlGrid( hfBatchId.Value.AsIntegerOrNull() , hfDataViewId.Value.AsIntegerOrNull() );
                LoadEntityDropDowns();
            }
        }

        /// <summary>
        /// Applies the block properties.
        /// </summary>
        private void ApplyBlockProperties()
        {
            var panelTitle = this.GetAttributeValue( "PanelTitle" );
            if ( string.IsNullOrEmpty( panelTitle ) )
            {
                if ( _transactionEntityType != null )
                {
                    lPanelTitle.Text = _entityTypeQualifiedName + " Matching";
                }
                else
                {
                    lPanelTitle.Text = "Matching";
                }
            }
            else
            {
                lPanelTitle.Text = panelTitle;
            }

            ddlBatch.Visible = this.GetAttributeValue( "ShowBatchFilter" ).AsBoolean();
            dvpDataView.Visible = this.GetAttributeValue( "ShowDataviewFilter" ).AsBoolean();
            nbBlockConfigurationWarning.Visible = false;
            int? transactionId = this.PageParameter( "TransactionId" ).AsIntegerOrNull();
            if ( transactionId.HasValue )
            {
                ddlBatch.Visible = false;
                dvpDataView.Visible = false;
            }
            else if ( !ddlBatch.Visible && !dvpDataView.Visible )
            {
                if ( !nbBlockConfigurationWarning.Visible )
                {
                    nbBlockConfigurationWarning.Text = "Please set at least one visible filter in the block properties.";
                    nbBlockConfigurationWarning.Visible = true;
                }
            }

            if ( _transactionEntityType == null )
            {
                nbBlockConfigurationWarning.Text = "Please set the Entity Type in block settings";
                nbBlockConfigurationWarning.Visible = true;
            }

            Guid? blockTransactionTypeGuid = this.GetAttributeValue( "TransactionTypeGuid" ).AsGuidOrNull();
            if ( blockTransactionTypeGuid.HasValue )
            {
                var transactionType = DefinedValueCache.Get( blockTransactionTypeGuid.Value );
                _blockTransactionTypeId = transactionType != null ? transactionType.Id : ( int? ) null;
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
            if ( _financialTransactionDetailList == null )
            {
                return;
            }

            int? entityTypeQualifierValue = this.GetAttributeValue( "EntityTypeQualifierValue" ).AsIntegerOrNull();
            var rockContext = new RockContext();

            Dictionary<int, int?> entityLookup = _financialTransactionDetailList.Where( a => a.EntityId.HasValue ).ToDictionary( k => k.Id, v => v.EntityId );

            if ( _transactionEntityType != null )
            {
                if ( _transactionEntityType.Id == EntityTypeCache.GetId<GroupMember>() )
                {
                    int? groupTypeId = entityTypeQualifierValue;
                    var groupsWithMembersList = new GroupService( new RockContext() ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.Members.Any() && a.IsActive ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).AsNoTracking()
                        .Select( a => new
                        {
                            a.Id,
                            a.Name
                        } )
                        .ToList();

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

                        var groupMemberId = entityLookup.GetValueOrNull( financialTransactionDetailId );
                        GroupMember groupMember = null;
                        if ( groupMemberId.HasValue )
                        {
                            groupMember = new GroupMemberService( rockContext ).Get( groupMemberId.Value );
                        }

                        if ( groupMember != null )
                        {
                            ddlGroup.SetValue( groupMember.GroupId.ToString() );
                            LoadGroupMembersDropDown( ddlGroup );
                            ddlGroupMember.SetValue( groupMember.Id );
                        }
                        else
                        {
                            // if there is no groupMember, make sure the controls don't have anything selected
                            ddlGroup.SetValue( (int?) null );
                            ddlGroupMember.Items.Clear();
                            ddlGroupMember.SetValue( ( int? ) null );
                        }

                        ddlGroupMember.Visible = ddlGroup.SelectedValue.AsIntegerOrNull().HasValue;
                    }
                }
                else if ( _transactionEntityType.Id == EntityTypeCache.GetId<Group>() )
                {
                    int? groupTypeId = entityTypeQualifierValue;
                    bool limitToActiveGroups = this.GetAttributeValue( "LimitToActiveGroups" ).AsBoolean();
                    var groupQry = new GroupService( new RockContext() ).Queryable().Where( a => a.GroupTypeId == groupTypeId.Value && a.IsActive );
                    if ( limitToActiveGroups )
                    {
                        groupQry = groupQry.Where( a => a.IsActive == true );
                    };

                    var groupList = groupQry.OrderBy( a => a.Order ).ThenBy( a => a.Name ).AsNoTracking().Select( a =>
                        new
                        {
                            a.Id,
                            a.Name
                        } )
                        .ToList();

                    foreach ( var ddlGroup in phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID.StartsWith( "ddlGroup_" ) ) )
                    {
                        ddlGroup.Items.Clear();
                        ddlGroup.Items.Add( new ListItem() );
                        foreach ( var group in groupList )
                        {
                            ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                        }

                        var financialTransactionDetailId = ddlGroup.ID.Replace( "ddlGroup_", string.Empty ).AsInteger();

                        var groupId = entityLookup.GetValueOrNull( financialTransactionDetailId );
                        if ( groupId.HasValue )
                        {
                            ddlGroup.SetValue( groupId );
                        }
                        else
                        {
                            // if there is no value, make sure the controls don't have anything selected
                            ddlGroup.SetValue( ( int? ) null );
                        }
                    }
                }
                else if ( _transactionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                {
                    int? definedTypeId = entityTypeQualifierValue;
                    var definedValueList = DefinedTypeCache.Get( definedTypeId.Value ).DefinedValues;

                    foreach ( var ddlDefinedValue in phTableRows.ControlsOfTypeRecursive<DefinedValuePicker>().Where( a => a.ID.StartsWith( "ddlDefinedValue_" ) ) )
                    {
                        ddlDefinedValue.DefinedTypeId = definedTypeId;

                        var financialTransactionDetailId = ddlDefinedValue.ID.Replace( "ddlDefinedValue_", string.Empty ).AsInteger();

                        var definedValueId = entityLookup.GetValueOrNull( financialTransactionDetailId );
                        if ( definedValueId.HasValue )
                        {
                            ddlDefinedValue.SetValue( definedValueId );
                        }
                        else
                        {
                            // if there is no value, make sure the controls don't have anything selected
                            ddlDefinedValue.SetValue( ( int? ) null );
                        }
                    }
                }
                else if ( _transactionEntityType.SingleValueFieldType != null && _transactionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    foreach ( var entityPicker in phTableRows.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "entityPicker_" ) ) )
                    {
                        var financialTransactionDetailId = entityPicker.ID.Replace( "entityPicker_", string.Empty ).AsInteger();

                        var entityId = entityLookup.GetValueOrNull( financialTransactionDetailId );

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
            ApplyBlockProperties();
            FilterChanged( sender, e );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FilterChanged( object sender, EventArgs e )
        {
            hfBatchId.Value = ddlBatch.SelectedValue;
            hfDataViewId.Value = dvpDataView.SelectedValue;
            this.SetBlockUserPreference( "DataViewId", hfDataViewId.Value );
            this.SetBlockUserPreference( "BatchId", hfBatchId.Value );
            BindHtmlGrid( hfBatchId.Value.AsIntegerOrNull(), hfDataViewId.Value.AsIntegerOrNull() );
            LoadEntityDropDowns();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the table controls.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        private void BindHtmlGrid( int? batchId, int? dataViewId )
        {
            _financialTransactionDetailList = null;
            RockContext rockContext = new RockContext();
            nbSaveSuccess.Visible = false;
            btnSave.Visible = false;

            List<DataControlField> tableColumns = new List<DataControlField>();
            tableColumns.Add( new RockLiteralField { ID = "lPerson", HeaderText = "Person" } );
            tableColumns.Add( new RockLiteralField { ID = "lAmount", HeaderText = "Amount" } );
            tableColumns.Add( new RockLiteralField { ID = "lAccount", HeaderText = "Account" } );
            tableColumns.Add( new RockLiteralField { ID = "lTransactionType", HeaderText = "Transaction Type" } );

            string entityColumnHeading = this.GetAttributeValue( "EntityColumnHeading" );
            if ( string.IsNullOrEmpty( entityColumnHeading ) )
            {
                if ( _transactionEntityType != null )
                {
                    entityColumnHeading = _entityTypeQualifiedName;
                }
            }

            tableColumns.Add( new RockLiteralField { ID = "lEntityColumn", HeaderText = entityColumnHeading } );

            var additionalColumns = this.GetAttributeValue( CustomGridColumnsConfig.AttributeKey ).FromJsonOrNull<CustomGridColumnsConfig>();
            if ( additionalColumns != null )
            {
                foreach ( var columnConfig in additionalColumns.ColumnsConfig )
                {
                    int insertPosition;
                    if ( columnConfig.PositionOffsetType == CustomGridColumnsConfig.ColumnConfig.OffsetType.LastColumn )
                    {
                        insertPosition = tableColumns.Count - columnConfig.PositionOffset;
                    }
                    else
                    {
                        insertPosition = columnConfig.PositionOffset;
                    }

                    var column = columnConfig.GetGridColumn();
                    tableColumns.Insert( insertPosition, column );
                    insertPosition++;
                }
            }

            StringBuilder headers = new StringBuilder();
            foreach ( var tableColumn in tableColumns )
            {
                if ( tableColumn.HeaderStyle.CssClass.IsNotNullOrWhiteSpace() )
                {
                    headers.AppendFormat( "<th class='{0}'>{1}</th>", tableColumn.HeaderStyle.CssClass, tableColumn.HeaderText );
                }
                else
                {
                    headers.AppendFormat( "<th>{0}</th>", tableColumn.HeaderText );
                }
            }

            lHeaderHtml.Text = headers.ToString();

            int? transactionId = this.PageParameter( "TransactionId" ).AsIntegerOrNull();

            if ( batchId.HasValue || dataViewId.HasValue || transactionId.HasValue )
            {
                var financialTransactionDetailQuery = new FinancialTransactionDetailService( rockContext ).Queryable()
                    .Include( a => a.Transaction )
                    .Include( a => a.Transaction.AuthorizedPersonAlias.Person );
                if ( batchId.HasValue )
                {
                    financialTransactionDetailQuery = financialTransactionDetailQuery.Where( a => a.Transaction.BatchId == batchId.Value );
                }

                if ( dataViewId.HasValue && dataViewId > 0 )
                {
                    var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                    List<string> errorMessages;
                    var transactionDetailIdsQry = dataView.GetQuery( null, rockContext, null, out errorMessages ).Select( a => a.Id );
                    financialTransactionDetailQuery = financialTransactionDetailQuery.Where( a => transactionDetailIdsQry.Contains( a.Id ) );
                }

                if ( transactionId.HasValue )
                {
                    financialTransactionDetailQuery = financialTransactionDetailQuery.Where( a => transactionId == a.TransactionId );
                }

                int maxResults = this.GetAttributeValue( "MaxNumberofResults" ).AsIntegerOrNull() ?? 1000;
                _financialTransactionDetailList = financialTransactionDetailQuery.OrderByDescending( a => a.Transaction.TransactionDateTime ).Take( maxResults ).ToList();
                phTableRows.Controls.Clear();
                btnSave.Visible = _financialTransactionDetailList.Any();
                string appRoot = this.ResolveRockUrl( "~/" );
                string themeRoot = this.ResolveRockUrl( "~~/" );

                foreach ( var financialTransactionDetail in _financialTransactionDetailList )
                {
                    var tr = new HtmlGenericContainer( "tr" );
                    foreach ( var tableColumn in tableColumns )
                    {
                        var literalControl = new LiteralControl();
                        if ( tableColumn is RockLiteralField )
                        {
                            tr.Controls.Add( literalControl );
                            var literalTableColumn = tableColumn as RockLiteralField;
                            if ( literalTableColumn.ID == "lPerson" )
                            {
                                literalControl.Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Transaction.AuthorizedPersonAlias );
                            }
                            else if ( literalTableColumn.ID == "lAmount" )
                            {
                                literalControl.Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Amount.FormatAsCurrency() );
                            }
                            else if ( literalTableColumn.ID == "lAccount" )
                            {
                                literalControl.Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Account.ToString() );
                            }
                            else if ( literalTableColumn.ID == "lTransactionType" )
                            {
                                literalControl.ID = "lTransactionType_" + financialTransactionDetail.Id.ToString();
                                literalControl.Text = string.Format( "<td>{0}</td>", financialTransactionDetail.Transaction.TransactionTypeValue );
                            }
                            else if ( literalTableColumn.ID == "lEntityColumn" )
                            {
                                var tdEntityControls = new HtmlGenericContainer( "td" ) { ID = "pnlEntityControls_" + financialTransactionDetail.Id.ToString() };
                                tr.Controls.Add( tdEntityControls );

                                if ( _transactionEntityType != null )
                                {
                                    if ( _transactionEntityType.Id == EntityTypeCache.GetId<GroupMember>() )
                                    {
                                        var ddlGroup = new RockDropDownList { ID = "ddlGroup_" + financialTransactionDetail.Id.ToString(), EnhanceForLongLists = true };
                                        ddlGroup.Label = "Group";
                                        ddlGroup.AutoPostBack = true;
                                        ddlGroup.SelectedIndexChanged += ddlGroup_SelectedIndexChanged;
                                        tdEntityControls.Controls.Add( ddlGroup );
                                        var ddlGroupMember = new RockDropDownList { ID = "ddlGroupMember_" + financialTransactionDetail.Id.ToString(), Visible = false, EnhanceForLongLists = true };
                                        ddlGroupMember.Label = "Group Member";
                                        tdEntityControls.Controls.Add( ddlGroupMember );
                                    }
                                    else if ( _transactionEntityType.Id == EntityTypeCache.GetId<Group>() )
                                    {
                                        var ddlGroup = new RockDropDownList { ID = "ddlGroup_" + financialTransactionDetail.Id.ToString(), EnhanceForLongLists = true };
                                        ddlGroup.AutoPostBack = false;
                                        tdEntityControls.Controls.Add( ddlGroup );
                                    }
                                    else if ( _transactionEntityType.Id == EntityTypeCache.GetId<DefinedValue>() )
                                    {
                                        var ddlDefinedValue = new DefinedValuePicker { ID = "ddlDefinedValue_" + financialTransactionDetail.Id.ToString(), EnhanceForLongLists = true };
                                        tdEntityControls.Controls.Add( ddlDefinedValue );
                                    }
                                    else if ( _transactionEntityType.SingleValueFieldType != null )
                                    {
                                        var entityPicker = _transactionEntityType.SingleValueFieldType.Field.EditControl( new Dictionary<string, Rock.Field.ConfigurationValue>(), "entityPicker_" + financialTransactionDetail.Id.ToString() );
                                        tdEntityControls.Controls.Add( entityPicker );
                                    }
                                }
                            }

                        }
                        else if ( tableColumn is LavaField )
                        {
                            tr.Controls.Add( literalControl );
                            var lavaField = tableColumn as LavaField;

                            Dictionary<string, object> mergeValues = new Dictionary<string, object>();
                            mergeValues.Add( "Row", financialTransactionDetail );

                            string lavaOutput = lavaField.LavaTemplate.ResolveMergeFields( mergeValues );

                            // Resolve any dynamic url references
                            lavaOutput = lavaOutput.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                            if ( lavaField.ItemStyle.CssClass.IsNotNullOrWhiteSpace() )
                            {
                                literalControl.Text = string.Format( "<td class='{0}'>{1}</td>", lavaField.ItemStyle.CssClass, lavaOutput );
                            }
                            else
                            {
                                literalControl.Text = string.Format( "<td>{0}</td>", lavaOutput );
                            }
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
                var financialTransactionDetailLookup = _financialTransactionDetailList.FirstOrDefault( a => a.Id == financialTransactionDetailId );

                // An un-match operation is only allowed if the entity type is already our target entity type.
                if ( financialTransactionDetailLookup.EntityTypeId != _transactionEntityType.Id && !entityId.HasValue )
                {
                    return;
                }

                if ( financialTransactionDetailLookup.EntityTypeId != _transactionEntityType.Id
                    || financialTransactionDetailLookup.EntityId != entityId
                    || _blockTransactionTypeId.HasValue && _blockTransactionTypeId != financialTransactionDetailLookup.Transaction.TransactionTypeValueId )
                {
                    var rockContext = new RockContext();
                    var financialTransactionDetail = new FinancialTransactionDetailService( rockContext ).Get( financialTransactionDetailId.Value );
                    financialTransactionDetail.EntityTypeId = _transactionEntityType.Id;
                    financialTransactionDetail.EntityId = entityId;

                    if ( _blockTransactionTypeId.HasValue && _blockTransactionTypeId != financialTransactionDetail.Transaction.TransactionTypeValueId )
                    {
                        financialTransactionDetail.Transaction.TransactionTypeValueId = _blockTransactionTypeId.Value;
                        var lTransactionType = phTableRows.ControlsOfTypeRecursive<LiteralControl>().Where( a => a.ID == "lTransactionType_" + financialTransactionDetail.Id.ToString() ).FirstOrDefault();
                        if ( lTransactionType != null )
                        {
                            lTransactionType.Text = string.Format( "<td>{0}</td>", DefinedValueCache.Get( financialTransactionDetail.Transaction.TransactionTypeValueId ) );
                        }
                    }

                    rockContext.SaveChanges();
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
                        var dllGroup = phTableRows.ControlsOfTypeRecursive<RockDropDownList>().Where( a => a.ID == "ddlGroup_" + financialTransactionDetailId.Value.ToString() );
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

            ddlTransactionType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() ).Id;

            DefinedValueCache blockTransactionType = null;
            Guid? blockTransactionTypeGuid = this.GetAttributeValue( "TransactionTypeGuid" ).AsGuidOrNull();
            if ( blockTransactionTypeGuid.HasValue )
            {
                blockTransactionType = DefinedValueCache.Get( blockTransactionTypeGuid.Value );
            }

            ddlTransactionType.SetValue( blockTransactionType != null ? blockTransactionType.Id : ( int? ) null );

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
                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                etpEntityType.SetValue( entityType != null ? entityType.Id : ( int? ) null );
            }

            UpdateControlsForEntityType();

            tbEntityTypeQualifierColumn.Text = this.GetAttributeValue( "EntityTypeQualifierColumn" );

            gtpGroupType.SetValue( this.GetAttributeValue( "EntityTypeQualifierValue" ) );
            cbLimitToActiveGroups.Checked = this.GetAttributeValue( "LimitToActiveGroups" ).AsBoolean();
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
                var entityType = EntityTypeCache.Get( etpEntityType.SelectedEntityTypeId.Value );
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
                blockTransactionType = DefinedValueCache.Get( selectedTransactionTypeId.Value );
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
                this.SetAttributeValue( "LimitToActiveGroups", cbLimitToActiveGroups.Checked.ToString());
            }
            else
            {
                this.SetAttributeValue( "EntityTypeQualifierValue", tbEntityTypeQualifierValue.Text );
            }

            this.SaveAttributeValues();

            mdSettings.Hide();
            pnlSettings.Visible = false;

            // reload the page to make sure we have a clean load with the correct entityType, etc
            NavigateToCurrentPageReference();
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
            cbLimitToActiveGroups.Visible = false;
            tbEntityTypeQualifierColumn.ReadOnly = false;
            tbEntityTypeQualifierColumn.Visible = false;
            tbEntityTypeQualifierValue.Visible = false;

            if ( etpEntityType.SelectedEntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Get( etpEntityType.SelectedEntityTypeId.Value );
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
                        cbLimitToActiveGroups.Visible = true;
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