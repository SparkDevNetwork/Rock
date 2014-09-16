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
    [DisplayName("Content - Type Detail")]
    [Category("CMS")]
    [Description("Displays the details for a content type.")]
    public partial class ContentTypeDetail : RockBlock, IDetailBlock
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

            gContentItemAttributeTypes.DataKeyNames = new string[] { "Guid" };
            gContentItemAttributeTypes.Actions.ShowAdd = true;
            gContentItemAttributeTypes.Actions.AddClick += gContentItemAttributeType_Add;
            gContentItemAttributeTypes.GridRebind += gContentItemAttributeType_GridRebind;
            gContentItemAttributeTypes.EmptyDataText = Server.HtmlEncode( None.Text );
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
                ShowDetail( PageParameter( "contentTypeId" ).AsInteger() );
            }
        }

        #endregion

        #region AttributeTypes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gContentItemAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentItemAttributeType_Add( object sender, EventArgs e )
        {
            gContentItemAttributeType_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gContentItemAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentItemAttributeType_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gContentItemAttributeType_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the marketing campaign ad attribute type_ show edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void gContentItemAttributeType_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlContentTypeAttribute.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtContentTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for ad type " + tbName.Text );

            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtContentTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for ad type " + tbName.Text );
            }

            edtContentTypeAttributes.ReservedKeyNames = AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtContentTypeAttributes.SetAttributeProperties( attribute, typeof( ContentItem ) );
        }

        /// <summary>
        /// Handles the Delete event of the gContentItemAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gContentItemAttributeType_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindContentItemAttributeTypeGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentItemAttributeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentItemAttributeType_GridRebind( object sender, EventArgs e )
        {
            BindContentItemAttributeTypeGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveAttribute_Click( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtContentTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            AttributesState.RemoveEntity( attribute.Guid );
            AttributesState.Add( attribute );

            pnlDetails.Visible = true;
            pnlContentTypeAttribute.Visible = false;

            BindContentItemAttributeTypeGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlContentTypeAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the marketing campaign ad attribute type grid.
        /// </summary>
        private void BindContentItemAttributeTypeGrid()
        {
            gContentItemAttributeTypes.DataSource = AttributesState.OrderBy( a => a.Name ).ToList();
            gContentItemAttributeTypes.DataBind();
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
            ContentType contentType;

            ContentTypeService contentTypeService = new ContentTypeService( rockContext );

            int contentTypeId = int.Parse( hfContentTypeId.Value );

            if ( contentTypeId == 0 )
            {
                contentType = new ContentType();
                contentTypeService.Add( contentType );
            }
            else
            {
                contentType = contentTypeService.Get( contentTypeId );
            }

            contentType.Name = tbName.Text;
            contentType.RequiresApproval = cbRequireApproval.Checked;
            contentType.DateRangeType = (DateRangeTypeEnum)int.Parse( ddlDateRangeType.SelectedValue );

            if ( !contentType.IsValid )
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
                contentType = contentTypeService.Get( contentType.Guid );

                var entityTypeId = EntityTypeCache.Read( typeof( ContentItem ) ).Id;
                string qualifierColumn = "ContentTypeId";
                string qualifierValue = contentType.Id.ToString();

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
        /// <param name="contentTypeId">The marketing campaign ad type identifier.</param>
        public void ShowDetail( int contentTypeId )
        {
            pnlDetails.Visible = true;
            ContentType contentType = null;

            var rockContext = new RockContext();

            if ( !contentTypeId.Equals( 0 ) )
            {
                contentType = new ContentTypeService( rockContext ).Get( contentTypeId );
                lActionTitle.Text = "Content Type Detail".FormatAsHtmlTitle();
            }

            if ( contentType == null )
            {
                contentType = new ContentType { Id = 0 };
                lActionTitle.Text = "Content Type Detail".FormatAsHtmlTitle();
            }

            LoadDropDowns();

            // load data into UI controls
            AttributesState = new ViewStateList<Attribute>();

            hfContentTypeId.Value = contentType.Id.ToString();
            tbName.Text = contentType.Name;
            cbRequireApproval.Checked = contentType.RequiresApproval;
            ddlDateRangeType.SetValue( (int)contentType.DateRangeType );

            AttributeService attributeService = new AttributeService( rockContext );

            string qualifierValue = contentType.Id.ToString();
            var qry = attributeService.GetByEntityTypeId( new ContentItem().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "ContentTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            AttributesState.AddAll( qry.ToList() );
            BindContentItemAttributeTypeGrid();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ContentType.FriendlyTypeName );
            }

            if ( contentType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( ContentType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( ContentType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            ddlDateRangeType.Enabled = !readOnly;
            gContentItemAttributeTypes.Enabled = !readOnly;

            btnSave.Visible = !readOnly;
        }

        #endregion

    }
}