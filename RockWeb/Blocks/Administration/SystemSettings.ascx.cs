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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Block used to set values specific to system settings.
    /// </summary>
    [DisplayName( "System Settings" )]
    [Category( "Administration" )]
    [Description( "Block used to set values specific to system settings." )]

    [AttributeCategoryField( "Category", "The Attribute Category to display attributes from", false, "", true, "", "", 0 )]
    public partial class SystemSettings : Rock.Web.UI.RockBlock
    {

        #region Fields

        // View modes
        private readonly string VIEW_MODE_VIEW = "VIEW";
        private readonly string VIEW_MODE_EDIT = "EDIT";

        private bool _canAdministrate = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the view mode.
        /// </summary>
        /// <value>
        /// The view mode.
        /// </value>
        protected string ViewMode
        {
            get
            {
                var viewMode = ViewState["ViewMode"];
                if ( viewMode == null )
                {
                    return VIEW_MODE_VIEW;
                }
                else
                {
                    return viewMode.ToString();
                }
            }
            set
            {
                ViewState["ViewMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute list.
        /// </summary>
        /// <value>
        /// The attribute list.
        /// </value>
        protected List<int> AttributeList
        {
            get
            {
                List<int> attributeList = ViewState["AttributeList"] as List<int>;
                if ( attributeList == null )
                {
                    attributeList = new List<int>();
                    ViewState["AttributeList"] = attributeList;
                }
                return attributeList;
            }
            set { ViewState["AttributeList"] = value; }
        }

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
            this.AddConfigurationUpdateTrigger( upnlSystemSettings );

            _canAdministrate = IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE );
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
                BindData();
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CreateControls( false );
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
            AttributeList = null;
            BindData();
            CreateControls( true );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ViewMode = VIEW_MODE_EDIT;
            CreateControls( true );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                if ( ViewMode == VIEW_MODE_EDIT )
                {
                    var rockContext = new RockContext();
                    var attributeService = new AttributeService( rockContext );
                    foreach ( int attributeId in AttributeList )
                    {
                        var attributeCache = AttributeCache.Get( attributeId );
                        var attribute = attributeService.Get( attributeId );

                        if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            Control attributeControl = fsAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                            if ( attributeControl != null )
                            {
                                attribute.DefaultValue = attributeCache.FieldType.Field.GetEditValue( attributeControl, attributeCache.QualifierValues );
                                attribute = Helper.SaveAttributeEdits( attribute, null, "SystemSetting", string.Empty );

                                // Attribute will be null if it was not valid
                                if ( attribute == null )
                                {
                                    return;
                                }
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }

                ViewMode = VIEW_MODE_VIEW;
                CreateControls( false );
            }
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ViewMode = VIEW_MODE_VIEW;
            CreateControls( false );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bind the data based on the configured category setting.
        /// </summary>
        private void BindData()
        {
            AttributeList = new List<int>();

            Guid? categoryGuid = GetAttributeValue( "Category" ).AsGuidOrNull();
            if ( categoryGuid.HasValue )
            {
                var category = CategoryCache.Get( categoryGuid.Value );
                if ( category != null )
                {
                    if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
                    {
                        lCategoryName.Text = string.Format( "<i class='{0}'></i> {1}", category.IconCssClass, category.Name );
                    }
                    else
                    {
                        lCategoryName.Text = category.Name;
                    }


                    var attributeList = new AttributeService( new RockContext() ).GetByCategoryId( category.Id, false )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToCacheAttributeList();

                    attributeList.ForEach( a => AttributeList.Add( a.Id ) );
                }
            }

            CreateControls( true );
        }

        private void CreateControls( bool setValues )
        {
            nbEditModeMessage.Visible = false;
            fsAttributes.Controls.Clear();

            string validationGroup = string.Format( "vgAttributeValues_{0}", this.BlockId );
            valSummaryTop.ValidationGroup = validationGroup;
            btnSave.ValidationGroup = validationGroup;

            if (AttributeList.Count > 0)
            {
                foreach (int attributeId in AttributeList)
                {
                    var attribute = AttributeCache.Get( attributeId );
                    string formattedValue = string.Empty;

                    if (ViewMode != VIEW_MODE_EDIT || !attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ))
                    {
                        if (attribute.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName)
                        {
                            formattedValue = attribute.FieldType.Field.FormatValueAsHtml( fsAttributes, attribute.EntityTypeId, null, attribute.DefaultValue, attribute.QualifierValues, true );
                        }
                        else
                        {
                            formattedValue = attribute.FieldType.Field.FormatValueAsHtml( fsAttributes, attribute.EntityTypeId, null, attribute.DefaultValue, attribute.QualifierValues, false );
                        }

                        if (!string.IsNullOrWhiteSpace( formattedValue ))
                        {
                            if (attribute.FieldType.Class == typeof( Rock.Field.Types.MatrixFieldType ).FullName)
                            {
                                fsAttributes.Controls.Add( new RockLiteral { Label = attribute.Name, Text = formattedValue, CssClass = "matrix-attribute" } );
                            }
                            else
                            {
                                fsAttributes.Controls.Add( new RockLiteral { Label = attribute.Name, Text = formattedValue } );
                            }
                        }
                    }
                    else
                    {
                        attribute.AddControl( fsAttributes.Controls, attribute.DefaultValue, validationGroup, setValues, true );
                    }
                }
            }
            else
            {
                nbEditModeMessage.Text = "No Setting found under the selected category.";
                nbEditModeMessage.Visible = true;
            }

            pnlActions.Visible = ( ViewMode != VIEW_MODE_VIEW );
            pnlEditActions.Visible = (ViewMode == VIEW_MODE_VIEW && AttributeList.Count > 0);
        }

        #endregion

    }
}
