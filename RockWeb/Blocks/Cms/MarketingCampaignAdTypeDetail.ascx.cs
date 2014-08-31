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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Marketing Campaign - Ad Type Detail")]
    [Category("CMS")]
    [Description("Displays the details for an ad type.")]
    public partial class MarketingCampaignAdTypeDetail : RockBlock, IDetailBlock
    {
        #region Child Grid States

        /// <summary>
        /// Gets or sets the state of the attributes.
        /// </summary>
        /// <value>
        /// The state of the attributes.
        /// </value>
        private ViewStateList<Attribute> AttributesState
        {
            get
            {
                return ViewState["AttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["AttributesState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMarketingCampaignAdAttributeTypes.DataKeyNames = new string[] { "Guid" };
            gMarketingCampaignAdAttributeTypes.Actions.ShowAdd = true;
            gMarketingCampaignAdAttributeTypes.Actions.AddClick += gMarketingCampaignAdAttributeType_Add;
            gMarketingCampaignAdAttributeTypes.GridRebind += gMarketingCampaignAdAttributeType_GridRebind;
            gMarketingCampaignAdAttributeTypes.EmptyDataText = Server.HtmlEncode( None.Text );
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
                ShowDetail( PageParameter( "marketingCampaignAdTypeId" ).AsInteger() );
            }
        }

        #endregion

        #region AttributeTypes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gMarketingCampaignAdAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAdAttributeType_Add( object sender, EventArgs e )
        {
            gMarketingCampaignAdAttributeType_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gMarketingCampaignAdAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAdAttributeType_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gMarketingCampaignAdAttributeType_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the marketing campaign ad attribute type_ show edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void gMarketingCampaignAdAttributeType_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlAdTypeAttribute.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtAdTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for ad type " + tbName.Text );

            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtAdTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for ad type " + tbName.Text );
            }

            edtAdTypeAttributes.ReservedKeyNames = AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtAdTypeAttributes.SetAttributeProperties( attribute, typeof( MarketingCampaignAd ) );
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaignAdAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gMarketingCampaignAdAttributeType_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindMarketingCampaignAdAttributeTypeGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaignAdAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAdAttributeType_GridRebind( object sender, EventArgs e )
        {
            BindMarketingCampaignAdAttributeTypeGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveAttribute_Click( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtAdTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            AttributesState.RemoveEntity( attribute.Guid );
            AttributesState.Add( attribute );

            pnlDetails.Visible = true;
            pnlAdTypeAttribute.Visible = false;

            BindMarketingCampaignAdAttributeTypeGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlAdTypeAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the marketing campaign ad attribute type grid.
        /// </summary>
        private void BindMarketingCampaignAdAttributeTypeGrid()
        {
            gMarketingCampaignAdAttributeTypes.DataSource = AttributesState.OrderBy( a => a.Name ).ToList();
            gMarketingCampaignAdAttributeTypes.DataBind();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            MarketingCampaignAdType marketingCampaignAdType;

            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService( rockContext );

            int marketingCampaignAdTypeId = int.Parse( hfMarketingCampaignAdTypeId.Value );

            if ( marketingCampaignAdTypeId == 0 )
            {
                marketingCampaignAdType = new MarketingCampaignAdType();
                marketingCampaignAdTypeService.Add( marketingCampaignAdType );
            }
            else
            {
                marketingCampaignAdType = marketingCampaignAdTypeService.Get( marketingCampaignAdTypeId );
            }

            marketingCampaignAdType.Name = tbName.Text;
            marketingCampaignAdType.DateRangeType = (DateRangeTypeEnum)int.Parse( ddlDateRangeType.SelectedValue );

            if ( !marketingCampaignAdType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                AttributeService attributeService = new AttributeService( rockContext );
                AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
                CategoryService categoryService = new CategoryService( rockContext );

                rockContext.SaveChanges();

                // get it back to make sure we have a good Id for it for the Attributes
                marketingCampaignAdType = marketingCampaignAdTypeService.Get( marketingCampaignAdType.Guid );

                var entityTypeId = EntityTypeCache.Read( typeof( MarketingCampaignAd ) ).Id;
                string qualifierColumn = "MarketingCampaignAdTypeId";
                string qualifierValue = marketingCampaignAdType.Id.ToString();

                // Get the existing attributes for this entity type and qualifier value
                var attributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

                // Delete any of those attributes that were removed in the UI
                var selectedAttributeGuids = AttributesState.Select( a => a.Guid );
                foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
                {
                    Rock.Web.Cache.AttributeCache.Flush( attr.Id );

                    attributeService.Delete( attr );
                }

                rockContext.SaveChanges();

                // Update the Attributes that were assigned in the UI
                foreach ( var attributeState in AttributesState )
                {
                    Rock.Attribute.Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
                }

            } );

            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlDateRangeType.BindToEnum<DateRangeTypeEnum>();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="marketingCampaignAdTypeId">The marketing campaign ad type identifier.</param>
        public void ShowDetail( int marketingCampaignAdTypeId )
        {
            pnlDetails.Visible = true;
            MarketingCampaignAdType marketingCampaignAdType = null;

            var rockContext = new RockContext();

            if ( !marketingCampaignAdTypeId.Equals( 0 ) )
            {
                marketingCampaignAdType = new MarketingCampaignAdTypeService( rockContext ).Get( marketingCampaignAdTypeId );
                lActionTitle.Text = "Ad Type Detail".FormatAsHtmlTitle();
            }

            if ( marketingCampaignAdType == null )
            {
                marketingCampaignAdType = new MarketingCampaignAdType { Id = 0 };
                lActionTitle.Text = "Ad Type Detail".FormatAsHtmlTitle();
            }

            LoadDropDowns();

            // load data into UI controls
            AttributesState = new ViewStateList<Attribute>();

            hfMarketingCampaignAdTypeId.Value = marketingCampaignAdType.Id.ToString();
            tbName.Text = marketingCampaignAdType.Name;
            ddlDateRangeType.SetValue( (int)marketingCampaignAdType.DateRangeType );

            AttributeService attributeService = new AttributeService( rockContext );

            string qualifierValue = marketingCampaignAdType.Id.ToString();
            var qry = attributeService.GetByEntityTypeId( new MarketingCampaignAd().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "MarketingCampaignAdTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            AttributesState.AddAll( qry.ToList() );
            BindMarketingCampaignAdAttributeTypeGrid();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MarketingCampaignAdType.FriendlyTypeName );
            }

            if ( marketingCampaignAdType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( MarketingCampaignAdType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( MarketingCampaignAdType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            ddlDateRangeType.Enabled = !readOnly;
            gMarketingCampaignAdAttributeTypes.Enabled = !readOnly;

            btnSave.Visible = !readOnly;
        }

        #endregion

    }
}