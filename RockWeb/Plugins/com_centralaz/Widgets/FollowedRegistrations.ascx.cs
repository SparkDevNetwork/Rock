// <copyright>
// Copyright by Central Christian Church
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using Rock.Security;
using System.Reflection;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Takes a entity type and displays a person's following items for that entity using a Lava template.
    /// </summary>
    [DisplayName( "Followed Registrations" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Displays a person's following registrations along with registrant totals and subtotals." )]
    [TextField( "Link URL", "The address to use for the link. The text '[Id]' will be replaced with the Id of the entity '[Guid]' will be replaced with the Guid.", false, "RegistrationInstance/[Id]", "", 1, "LinkUrl" )]
    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"<ul class='list-unstyled margin-b-md'>
{% for registrationInstance in FollowingItems %}
    {% if LinkUrl != '' %}
        <li><i class='fa fa-clipboard icon-fw'></i><a href=""{{ LinkUrl | Replace:'[Id]',registrationInstance.InstanceId }}""> {{ registrationInstance.InstanceName }} ({{registrationInstance.Count}})</a></li>
    {% else %}
        <li>{{ registrationInstance.InstanceName }}</li>
    {% endif %}
	<ul>
		{% for instanceAttribute in registrationInstance.InstanceAttributes %}			
			{% if instanceAttribute.AttributeValues | Size > 0 %}
				<li>{{ instanceAttribute.AttributeName }}</li>
				<ul>
					{% for attributeValue in instanceAttribute.AttributeValues %}
						<li>{{attributeValue.ValueName}} : {{attributeValue.Count}}</li>		
					{% endfor %}
				</ul>
			{% endif %}
		{% endfor %}
		{% for instanceFee in registrationInstance.InstanceFees %}			
			{% if instanceFee.FeeValues | Size > 0 %}

                {% assign firstFee = instanceFee.FeeValues | First %}
                {% if instanceFee.FeeValues | Size == 1 and instanceFee.FeeName == firstFee.ValueName %}
                    <li>{{firstFee.ValueName}} <small class='text-muted'>(fee)</small> : {{firstFee.Count}}</li>
                {% else %}
				    <li>{{ instanceFee.FeeName }} <small class='text-muted'>(fee)</small></li>
				    <ul>
					    {% for feeValue in instanceFee.FeeValues %}
						    <li>{{feeValue.ValueName}} : {{feeValue.Count}}</li>		
					    {% endfor %}
				    </ul>
			    {% endif %}
			{% endif %}
		{% endfor %}
    </ul>
{% endfor %}

{% if HasMore %}
    <li><i class='fa icon-fw''></i> <small>(showing top {{ Quantity }})</small></li>
{% endif %}
</ul>", "", 2, "LavaTemplate" )]
    [BooleanField( "Enable Debug", "Show merge data to help you see what's available to you.", order: 3 )]
    [IntegerField( "Max Results", "The maximum number of results to display.", true, 100, order: 4 )]
    public partial class FollowedRegistrations : Rock.Web.UI.RockBlock
    {
        #region Fields

        Guid _multiSelectGuid = Rock.SystemGuid.FieldType.MULTI_SELECT.AsGuid();
        Guid _singleSelectGuid = Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid();
        Guid _registrantGuid = EntityTypeCache.Read( "Rock.Model.RegistrationRegistrant" ).Guid; // Rock.Model.RegistrationRegistrant guid

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
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                LoadContent();
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
            LoadContent();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var registrationEntityType = EntityTypeCache.Read( "5CD9C0C8-C047-61A0-4E36-0FDB8496F066".AsGuid() );

            if ( registrationEntityType != null )
            {
                pnlEdit.Visible = true;
                pnlView.Visible = false;

                RockContext rockContext = new RockContext();
                var followingService = new FollowingService( rockContext );
                var attributeService = new AttributeService( rockContext );
                var attributeValueService = new AttributeValueService( rockContext );
                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                var registrationTemplateFormFieldService = new RegistrationTemplateFormFieldService( rockContext );
                var registrationTemplateFeeService = new RegistrationTemplateFeeService( rockContext );

                int personId = this.CurrentPersonId.Value;
                IQueryable<IEntity> qryFollowedItems = followingService.GetFollowedItems( registrationEntityType.Id, personId );
                List<RegistrationInstanceCount> registrationList = new List<RegistrationInstanceCount>();

                //Grab existing stuff
                var json = this.GetUserPreference( "FollowedRegistrations" );
                if ( !String.IsNullOrWhiteSpace( json ) )
                {
                    registrationList = JsonConvert.DeserializeObject<List<RegistrationInstanceCount>>( json );
                }

                // Delete non-existing items
                // Use registrationList.ToList() otherwise when an item is removed, an 'collection was modified' exception will be thrown.
                foreach ( var registrationInstanceCount in registrationList.ToList() )
                {
                    var registration = registrationInstanceService.Get( registrationInstanceCount.InstanceId );
                    if ( registration != null )
                    {
                        var registrationTemplateAttributeIds = registrationTemplateFormFieldService.Queryable().AsNoTracking()
                            .Where( rtff => rtff.RegistrationTemplateForm.RegistrationTemplateId == registration.RegistrationTemplateId )
                            .Select( rtff => rtff.AttributeId )
                            .ToList();

                        // Use ...ToList() otherwise when an item is removed, an 'collection was modified' exception will be thrown.
                        foreach ( var instanceAttributeCount in registrationInstanceCount.InstanceAttributes.ToList() )
                        {
                            if ( registrationTemplateAttributeIds.Contains( instanceAttributeCount.AttributeId ) )
                            {
                                var attribute = attributeService.Get( instanceAttributeCount.AttributeId );
                                if ( attribute != null )
                                {
                                    var qualifier = attribute.AttributeQualifiers.Where( q => q.Key == "values" ).FirstOrDefault();
                                    if ( qualifier != null && !String.IsNullOrWhiteSpace( qualifier.Value ) )
                                    {

                                    }
                                    else
                                    {
                                        registrationInstanceCount.InstanceAttributes.Remove( instanceAttributeCount );
                                    }
                                }
                                else
                                {
                                    registrationInstanceCount.InstanceAttributes.Remove( instanceAttributeCount );
                                }
                            }
                            else
                            {
                                registrationInstanceCount.InstanceAttributes.Remove( instanceAttributeCount );
                            }
                        }
                    }
                    else
                    {
                        registrationList.Remove( registrationInstanceCount );
                    }
                }

                // Add any followed items or attributes that are not pre-defined.
                var existingRegistrationList = registrationList.Select( r => r.InstanceId ).Distinct().ToList();

                foreach ( var item in qryFollowedItems.ToList() )
                {
                    var registration = registrationInstanceService.Get( item.Id );
                    var registrationInstanceCount = registrationList.Where( r => r.InstanceId == item.Id ).FirstOrDefault();
                    if ( registrationInstanceCount == null )
                    {
                        registrationInstanceCount = new RegistrationInstanceCount();
                        registrationInstanceCount.InstanceName = registration.Name;
                        registrationInstanceCount.InstanceId = registration.Id;
                        registrationInstanceCount.InstanceAttributes = new List<InstanceAttributeCount>();
                        registrationInstanceCount.InstanceFees = new List<InstanceFeeCount>();
                        registrationList.Add( registrationInstanceCount );
                    }

                    var registrationTemplateAttributeList = registrationTemplateFormFieldService.Queryable().AsNoTracking()
                            .Where( rtff =>
                                rtff.RegistrationTemplateForm.RegistrationTemplateId == registration.RegistrationTemplateId &&
                                ( rtff.Attribute.FieldType.Guid == _multiSelectGuid || rtff.Attribute.FieldType.Guid == _singleSelectGuid ) )
                            .Select( rtff => rtff.Attribute )
                            .ToList();

                    foreach ( var attribute in registrationTemplateAttributeList )
                    {
                        var instanceAttributeCount = registrationInstanceCount.InstanceAttributes.Where( i => attribute.Id == i.AttributeId ).FirstOrDefault();
                        if ( instanceAttributeCount == null )
                        {
                            var qualifier = attribute.AttributeQualifiers.Where( q => q.Key == "values" ).FirstOrDefault();
                            if ( qualifier != null && !String.IsNullOrWhiteSpace( qualifier.Value ) )
                            {
                                instanceAttributeCount = new InstanceAttributeCount();
                                instanceAttributeCount.AttributeName = attribute.Name;
                                instanceAttributeCount.AttributeId = attribute.Id;
                                instanceAttributeCount.InstanceId = registration.Id;
                                instanceAttributeCount.AttributeValues = new List<AttributeValueCount>();
                                registrationInstanceCount.InstanceAttributes.Add( instanceAttributeCount );
                            }
                        }
                    }

                    var registrationTemplateFeeList = registrationTemplateFeeService.Queryable().AsNoTracking()
                            .Where( rtf =>
                                rtf.RegistrationTemplateId == registration.RegistrationTemplateId &&
                                ( rtf.FeeType == RegistrationFeeType.Multiple || rtf.FeeType == RegistrationFeeType.Single ) )
                            .Select( rtf => rtf )
                            .ToList();

                    if ( registrationInstanceCount.InstanceFees == null )
                    {
                        registrationInstanceCount.InstanceFees = new List<InstanceFeeCount>();
                    }

                    foreach ( var fee in registrationTemplateFeeList )
                    {
                        var instanceFeeCount = registrationInstanceCount.InstanceFees.Where( i => fee.Id == i.RegistrationTemplateFeeId ).FirstOrDefault();
                        if ( instanceFeeCount == null )
                        {
                            //var qualifier =  fee. feeAttributeQualifiers.Where( q => q.Key == "values" ).FirstOrDefault();
                            //if ( qualifier != null && !String.IsNullOrWhiteSpace( qualifier.Value ) )
                            //{
                                instanceFeeCount = new InstanceFeeCount();
                                instanceFeeCount.FeeName = fee.Name;
                                instanceFeeCount.RegistrationTemplateFeeId = fee.Id;
                                instanceFeeCount.InstanceId = registration.Id;
                                instanceFeeCount.FeeValues = new List<AttributeValueCount>();
                                registrationInstanceCount.InstanceFees.Add( instanceFeeCount );
                            //}
                        }
                    }

                }

                rptFollowedRegistrations.DataSource = registrationList;
                rptFollowedRegistrations.DataBind();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            List<RegistrationInstanceCount> registrationList = new List<RegistrationInstanceCount>();

            foreach ( RepeaterItem registrationItem in rptFollowedRegistrations.Items )
            {
                var cblValues = registrationItem.FindControl( "cblValues" ) as CheckBoxList;
                var cblFeeValues = registrationItem.FindControl( "cblFeeValues" ) as CheckBoxList;
                var lName = registrationItem.FindControl( "lName" ) as Literal;
                var hfRegistrationId = registrationItem.FindControl( "hfRegistrationId" ) as HiddenField;

                var registrationInstanceCount = new RegistrationInstanceCount();
                registrationInstanceCount.InstanceName = lName.Text;
                registrationInstanceCount.InstanceId = hfRegistrationId.ValueAsInt();
                registrationInstanceCount.InstanceAttributes = new List<InstanceAttributeCount>();
                registrationInstanceCount.InstanceFees = new List<InstanceFeeCount>();
                registrationList.Add( registrationInstanceCount );

                foreach ( ListItem checkbox in cblValues.Items )
                {
                    var instanceAttributeCount = new InstanceAttributeCount();
                    instanceAttributeCount.AttributeName = checkbox.Text;
                    instanceAttributeCount.AttributeId = checkbox.Value.AsInteger();
                    instanceAttributeCount.IsSelected = checkbox.Selected;
                    instanceAttributeCount.InstanceId = registrationInstanceCount.InstanceId;
                    registrationInstanceCount.InstanceAttributes.Add( instanceAttributeCount );
                }

                foreach ( ListItem checkbox in cblFeeValues.Items )
                {
                    var instanceFeeCount = new InstanceFeeCount();
                    instanceFeeCount.FeeName = checkbox.Text;
                    instanceFeeCount.RegistrationTemplateFeeId = checkbox.Value.AsInteger();
                    instanceFeeCount.IsSelected = checkbox.Selected;
                    instanceFeeCount.InstanceId = registrationInstanceCount.InstanceId;
                    registrationInstanceCount.InstanceFees.Add( instanceFeeCount );
                }
            }

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            this.SetUserPreference( "FollowedRegistrations", JsonConvert.SerializeObject( registrationList, Formatting.None, jsonSetting ) );
            LoadContent();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            LoadContent();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptFollowedRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptFollowedRegistrations_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var cblValues = e.Item.FindControl( "cblValues" ) as CheckBoxList;
            var cblFeeValues = e.Item.FindControl( "cblFeeValues" ) as CheckBoxList;

            var lName = e.Item.FindControl( "lName" ) as Literal;
            var hfRegistrationId = e.Item.FindControl( "hfRegistrationId" ) as HiddenField;
            var registrationInstanceCount = e.Item.DataItem as RegistrationInstanceCount;

            hfRegistrationId.Value = registrationInstanceCount.InstanceId.ToString();
            lName.Text = registrationInstanceCount.InstanceName;
            cblValues.Items.Clear();
            foreach ( var attribute in registrationInstanceCount.InstanceAttributes )
            {
                ListItem listItem = new ListItem( attribute.AttributeName, attribute.AttributeId.ToString() );
                listItem.Selected = attribute.IsSelected;
                cblValues.Items.Add( listItem );
            }

            if ( registrationInstanceCount.InstanceFees != null )
            {
                foreach ( var fee in registrationInstanceCount.InstanceFees )
                {
                    ListItem listItem = new ListItem( fee.FeeName, fee.RegistrationTemplateFeeId.ToString() );
                    listItem.Selected = fee.IsSelected;
                    cblFeeValues.Items.Add( listItem );
                }
            }
        }

        #endregion

        #region Methods

        protected void LoadContent()
        {
            try
            {
                pnlEdit.Visible = false;
                pnlView.Visible = true;
                List<int> toRemove = new List<int>();
                var entityType = EntityTypeCache.Read( "5CD9C0C8-C047-61A0-4E36-0FDB8496F066".AsGuid() );

                if ( entityType != null )
                {
                    int personId = this.CurrentPersonId.Value;

                    RockContext rockContext = new RockContext();
                    var followingService = new FollowingService( rockContext );
                    var attributeValueService = new AttributeValueService( rockContext );
                    var attributeQualifierService = new AttributeQualifierService( rockContext );
                    var registrationInstanceService = new RegistrationInstanceService( rockContext );

                    IQueryable<IEntity> qryFollowedItems = followingService.GetFollowedItems( entityType.Id, personId );

                    var userPreference = this.GetUserPreference( "FollowedRegistrations" );
                    List<RegistrationInstanceCount> registrationList = new List<RegistrationInstanceCount>();

                    if ( !String.IsNullOrWhiteSpace( userPreference ) )
                    {
                        registrationList = JsonConvert.DeserializeObject<List<RegistrationInstanceCount>>( userPreference );

                        // remove item no longer followed
                        foreach ( var registrationInstance in registrationList )
                        {
                            if ( !qryFollowedItems.Any( f => f.Id == registrationInstance.InstanceId ) )
                            {
                                toRemove.Add( registrationInstance.InstanceId );
                            }
                        }
                        registrationList.RemoveAll( x => toRemove.Contains( x.InstanceId ) );

                        foreach ( var registrationInstance in registrationList )
                        {
                            var registration = registrationInstanceService.Get( registrationInstance.InstanceId );
                            if ( registration != null )
                            {
                                var registrantIdList = registration.Registrations.SelectMany( r => r.Registrants ).Select( s => s.Id ).Distinct().ToList();
                                var personIdList = registration.Registrations.SelectMany( r => r.Registrants ).Select( s => s.PersonId ).Distinct().ToList();

                                // Display subtotals for each matching attribute
                                if ( registrationInstance.InstanceAttributes != null )
                                {
                                    foreach ( var registrationAttribute in registrationInstance.InstanceAttributes.Where( a => a.IsSelected ).ToList() )
                                    {
                                        var qualifier = attributeQualifierService.Queryable().AsNoTracking().Where( q => q.Key == "values" && q.AttributeId == registrationAttribute.AttributeId ).FirstOrDefault();
                                        if ( qualifier != null && !String.IsNullOrWhiteSpace( qualifier.Value ) )
                                        {
                                            registrationAttribute.AttributeValues = new List<AttributeValueCount>();
                                            foreach ( var qualifierValue in qualifier.Value.SplitDelimitedValues( false ) )
                                            {
                                                var attributeValueCount = new AttributeValueCount();
                                                string[] nameAndValue = qualifierValue.Trim().Split( new char[] { '^' } );

                                                if ( nameAndValue.Length == 1 )
                                                {
                                                    attributeValueCount.ValueName = nameAndValue[0];

                                                    attributeValueCount.Count = attributeValueService.Queryable().AsNoTracking().Where( av =>
                                                        av.AttributeId == registrationAttribute.AttributeId &&
                                                        av.EntityId.HasValue &&
                                                        ( ( av.Attribute.EntityType.Guid == _registrantGuid && registrantIdList.Contains( av.EntityId.Value ) ) ||
                                                        ( av.Attribute.EntityType.Guid != _registrantGuid && personIdList.Contains( av.EntityId.Value ) ) ) &&
                                                        ( "," + av.Value + "," ).Contains( "," + attributeValueCount.ValueName + "," ) )
                                                        .Count();
                                                }
                                                else if ( nameAndValue.Length > 1 )
                                                {
                                                    var value = nameAndValue[0];
                                                    attributeValueCount.ValueName = nameAndValue[1];

                                                    attributeValueCount.Count = attributeValueService.Queryable().AsNoTracking().Where( av =>
                                                        av.AttributeId == registrationAttribute.AttributeId &&
                                                        av.EntityId.HasValue &&
                                                        ( ( av.Attribute.EntityType.Guid == _registrantGuid && registrantIdList.Contains( av.EntityId.Value ) ) ||
                                                        ( av.Attribute.EntityType.Guid != _registrantGuid && personIdList.Contains( av.EntityId.Value ) ) ) &&
                                                        ( "," + av.Value + "," ).Contains( "," + value + "," ) )
                                                        .Count();
                                                }

                                                registrationAttribute.AttributeValues.Add( attributeValueCount );
                                            }
                                        }
                                    }
                                }

                                // Display subtotals for each matching Fee
                                if ( registrationInstance.InstanceFees != null )
                                {
                                    var feeIds = registrationInstance.InstanceFees.Where( a => a.IsSelected ).Select( f => f.RegistrationTemplateFeeId ).ToList();
                                    var fees = registration.Registrations.SelectMany( r => r.Registrants )
                                        .SelectMany( rr => rr.Fees )
                                        .Where( f => feeIds.Contains( f.RegistrationTemplateFeeId ) );

                                    foreach ( var registrationFee in registrationInstance.InstanceFees.Where( f => f.IsSelected ).ToList() )
                                    {
                                        registrationFee.FeeValues = new List<AttributeValueCount>();

                                        // Process multiple type fees:
                                        var multipleTypeOptions = fees.Where( f => f.RegistrationTemplateFeeId == registrationFee.RegistrationTemplateFeeId && f.RegistrationTemplateFee.FeeType == RegistrationFeeType.Multiple )
                                            .OrderBy( f => f.RegistrationTemplateFee.Name )
                                            .Select( o => o.Option ).Distinct();

                                        foreach ( var feeOption in multipleTypeOptions )
                                        {
                                            var feeValueCount = new AttributeValueCount();
                                            feeValueCount.ValueName = feeOption;
                                            feeValueCount.Count = fees.Where( f => f.Option == feeOption && f.RegistrationTemplateFeeId == registrationFee.RegistrationTemplateFeeId && f.RegistrationTemplateFee.FeeType == RegistrationFeeType.Multiple )
                                                .Sum( f => f.Quantity );
                                            registrationFee.FeeValues.Add( feeValueCount );
                                        }

                                        // Process single type fees:
                                        var singleTypeOptions = fees.Where( f => f.RegistrationTemplateFeeId == registrationFee.RegistrationTemplateFeeId && f.RegistrationTemplateFee.FeeType == RegistrationFeeType.Single )
                                            .OrderBy( f => f.RegistrationTemplateFee.Order )
                                            .Select( f => f.RegistrationTemplateFee.Name ).Distinct();

                                        foreach ( var feeOption in singleTypeOptions )
                                        {
                                            var feeValueCount = new AttributeValueCount();
                                            feeValueCount.ValueName = feeOption;
                                            var matchingFees = fees.Where( f => f.RegistrationTemplateFee.Name == feeOption && f.RegistrationTemplateFeeId == registrationFee.RegistrationTemplateFeeId && f.RegistrationTemplateFee.FeeType == RegistrationFeeType.Single );
                                            feeValueCount.Count = matchingFees.Sum( f => f.Quantity );

                                            registrationFee.FeeValues.Add( feeValueCount );
                                        }
                                    }
                                }

                                registrationInstance.Count = registrantIdList.Count;
                            }
                        }
                    }

                    // Add any followed items that are not pre-defined.
                    var registrationListIds = registrationList.Select( r => r.InstanceId ).Distinct().ToList();
                    qryFollowedItems = qryFollowedItems.Where( i => !registrationListIds.Contains( i.Id ) );
                    foreach ( var item in qryFollowedItems.ToList() )
                    {
                        var registration = registrationInstanceService.Get( item.Id );

                        var registrationInstance = new RegistrationInstanceCount();
                        registrationInstance.Count = registration.Registrations.SelectMany( r => r.Registrants ).Select( s => s.Id ).Distinct().Count();
                        registrationInstance.InstanceName = registration.Name;
                        registrationInstance.InstanceId = registration.Id;
                        registrationList.Add( registrationInstance );
                    }

                    int quantity = GetAttributeValue( "MaxResults" ).AsInteger();
                    var items = registrationList.Take( quantity + 1 ).ToList();

                    bool hasMore = ( quantity < items.Count );
                    items = items.Take( quantity ).ToList();

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "FollowingItems", items );
                    mergeFields.Add( "HasMore", hasMore );
                    mergeFields.Add( "Quantity", quantity );

                    mergeFields.Add( "LinkUrl", GetAttributeValue( "LinkUrl" ) );

                    string template = GetAttributeValue( "LavaTemplate" );
                    lContent.Text = template.ResolveMergeFields( mergeFields );

                    // Resave followed registrations if we removed some that are no longer followed.
                    if ( toRemove.Count > 0 )
                    {
                        var jsonSetting = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                        };
                        this.SetUserPreference( "FollowedRegistrations", JsonConvert.SerializeObject( registrationList, Formatting.None, jsonSetting ) );
                    }

                    // show debug info
                    if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                    {
                        lDebug.Visible = true;
                        lDebug.Text = mergeFields.lavaDebugInfo();
                    }
                }
                else
                {
                    lContent.Text = string.Format( "<div class='alert alert-warning'>Please configure an entity in the block settings." );
                }

            } catch ( Exception ex )
            {
                lContent.Text = string.Format( "<div class='alert alert-warning'>Sorry. There is a problem in here that prevented the block from working. ({0})", ex.Message );
            }
        }

        #endregion

        #region Helper Classes

        [DotLiquid.LiquidType( "InstanceName", "InstanceId", "InstanceAttributes", "InstanceFees", "Count" )]
        public class RegistrationInstanceCount
        {
            public string InstanceName { get; set; }
            public int InstanceId { get; set; }
            public List<InstanceAttributeCount> InstanceAttributes { get; set; }
            public List<InstanceFeeCount> InstanceFees { get; set; }
            public int Count { get; set; }
        }

        [DotLiquid.LiquidType( "AttributeValues", "AttributeName", "AttributeId", "InstanceId", "IsSelected" )]
        public class InstanceAttributeCount
        {
            public List<AttributeValueCount> AttributeValues { get; set; }
            public String AttributeName { get; set; }
            public int AttributeId { get; set; }
            public int InstanceId { get; set; }
            public bool IsSelected { get; set; }
        }

        [DotLiquid.LiquidType( "FeeValues", "FeeName", "RegistrationTemplateFeeId", "InstanceId", "IsSelected" )]
        public class InstanceFeeCount
        {
            public List<AttributeValueCount> FeeValues { get; set; }
            public String FeeName { get; set; }
            public int RegistrationTemplateFeeId { get; set; }
            public int InstanceId { get; set; }
            public bool IsSelected { get; set; }
        }

        [DotLiquid.LiquidType( "ValueName", "Count" )]
        public class AttributeValueCount
        {
            public string ValueName { get; set; }
            public int Count { get; set; }
        }

        #endregion
    }
}