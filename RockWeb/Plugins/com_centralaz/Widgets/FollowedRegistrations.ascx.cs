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
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

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

                int personId = this.CurrentPersonId.Value;
                IQueryable<IEntity> qryFollowedItems = followingService.GetFollowedItems( registrationEntityType.Id, personId );
                List<RegistrationInstanceCount> registrationList = new List<RegistrationInstanceCount>();

                //Grab existing stuff
                var json = this.GetUserPreference( "FollowedRegistrations" );
                if ( !String.IsNullOrWhiteSpace( json ) )
                {
                    registrationList = JsonConvert.DeserializeObject<List<RegistrationInstanceCount>>( json );
                }

                // Delete nonexisting items
                foreach ( var registrationInstanceCount in registrationList )
                {
                    var registration = registrationInstanceService.Get( registrationInstanceCount.InstanceId );
                    if ( registration != null )
                    {
                        var registrationTemplateAttributeIds = registrationTemplateFormFieldService.Queryable()
                            .Where( rtff => rtff.RegistrationTemplateForm.RegistrationTemplateId == registration.RegistrationTemplateId )
                            .Select( rtff => rtff.AttributeId )
                            .ToList();
                        foreach ( var instanceAttributeCount in registrationInstanceCount.InstanceAttributes )
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
                        registrationList.Add( registrationInstanceCount );
                    }

                    var registrationTemplateAttributeList = registrationTemplateFormFieldService.Queryable()
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
                var lName = registrationItem.FindControl( "lName" ) as Literal;
                var hfRegistrationId = registrationItem.FindControl( "hfRegistrationId" ) as HiddenField;

                var registrationInstanceCount = new RegistrationInstanceCount();
                registrationInstanceCount.InstanceName = lName.Text;
                registrationInstanceCount.InstanceId = hfRegistrationId.ValueAsInt();
                registrationInstanceCount.InstanceAttributes = new List<InstanceAttributeCount>();
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
        }

        #endregion

        #region Methods

        protected void LoadContent()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

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

                    foreach ( var registrationInstance in registrationList )
                    {
                        var registration = registrationInstanceService.Get( registrationInstance.InstanceId );
                        if ( registration != null )
                        {
                            var registrantIdList = registration.Registrations.SelectMany( r => r.Registrants ).Select( s => s.Id ).Distinct().ToList();
                            var personIdList = registration.Registrations.SelectMany( r => r.Registrants ).Select( s => s.PersonId ).Distinct().ToList();

                            foreach ( var registrationAttribute in registrationInstance.InstanceAttributes.Where( a => a.IsSelected ).ToList() )
                            {
                                var qualifier = attributeQualifierService.Queryable().Where( q => q.Key == "values" && q.AttributeId == registrationAttribute.AttributeId ).FirstOrDefault();
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

                                            attributeValueCount.Count = attributeValueService.Queryable().Where( av =>
                                                av.AttributeId == registrationAttribute.AttributeId &&
                                                av.EntityId.HasValue &&
                                                ( ( av.Attribute.EntityType.Guid == _registrantGuid && registrantIdList.Contains( av.EntityId.Value ) ) ||
                                                ( av.Attribute.EntityType.Guid != _registrantGuid && personIdList.Contains( av.EntityId.Value ) ) ) &&
                                                ( "," + av.Value + "," ).Contains( "," + attributeValueCount.ValueName + "," ) )
                                                .Count();
                                        }
                                        else
                                        {
                                            var value = nameAndValue[0];
                                            attributeValueCount.ValueName = nameAndValue[1];

                                            attributeValueCount.Count = attributeValueService.Queryable().Where( av =>
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

                mergeFields.Add( "FollowingItems", items );
                mergeFields.Add( "HasMore", hasMore );
                mergeFields.Add( "Quantity", quantity );

                mergeFields.Add( "LinkUrl", GetAttributeValue( "LinkUrl" ) );

                string template = GetAttributeValue( "LavaTemplate" );
                lContent.Text = template.ResolveMergeFields( mergeFields );

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
        }

        #endregion

        #region Helper Classes

        [DotLiquid.LiquidType( "InstanceName", "InstanceId", "InstanceAttributes", "Count" )]
        public class RegistrationInstanceCount
        {
            public string InstanceName { get; set; }
            public int InstanceId { get; set; }
            public List<InstanceAttributeCount> InstanceAttributes { get; set; }
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

        [DotLiquid.LiquidType( "ValueName", "Count" )]
        public class AttributeValueCount
        {
            public string ValueName { get; set; }
            public int Count { get; set; }
        }

        #endregion
    }
}