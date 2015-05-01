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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls.Communication;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for creating a new communication
    /// </summary>
    [DisplayName( "Template Detail" )]
    [Category( "Communication" )]
    [Description( "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people." )]
    public partial class TemplateDetail : RockBlock
    {
        #region Fields

        private bool _canEdit = false;

        #endregion

        #region Properties

        protected int? CommunicationTemplateId
        {
            get { return ViewState["CommunicationTemplateId"] as int?; }
            set { ViewState["CommunicationTemplateId"] = value; }
        }

        /// <summary>
        /// Gets or sets the medium entity type id.
        /// </summary>
        /// <value>
        /// The medium entity type id.
        /// </value>
        protected int? MediumEntityTypeId
        {
            get { return ViewState["MediumEntityTypeId"] as int?; }
            set { ViewState["MediumEntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the medium data.
        /// </summary>
        /// <value>
        /// The medium data.
        /// </value>
        protected Dictionary<string, string> MediumData
        {
            get 
            {
                var mediumData = ViewState["MediumData"] as Dictionary<string, string>;
                if ( mediumData == null )
                {
                    mediumData = new Dictionary<string, string>();
                    ViewState["MediumData"] = mediumData;
                }
                return mediumData;
            }

            set { ViewState["MediumData"] = value; }
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

            _canEdit = IsUserAuthorized( Authorization.EDIT );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                LoadMediumControl( false );
            }
            else
            {
                ShowDetail( PageParameter( "TemplateId" ).AsInteger() );
            }

        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            string pageTitle = "New Template";

            int? templateId = PageParameter( "TemplateId" ).AsIntegerOrNull();
            if ( templateId.HasValue )
            {
                var template = new CommunicationTemplateService( new RockContext() ).Get( templateId.Value );
                if ( template != null )
                {
                    pageTitle = template.Name;
                }
            }

            breadCrumbs.Add( new BreadCrumb( pageTitle, pageReference ) );
            RockPage.Title = pageTitle;

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbMedium control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbMedium_Click( object sender, EventArgs e )
        {
            GetMediumData();
            var linkButton = sender as LinkButton;
            if ( linkButton != null )
            {
                int mediumId = int.MinValue;
                if ( int.TryParse( linkButton.CommandArgument, out mediumId ) )
                {
                    MediumEntityTypeId = mediumId;
                    BindMediums();

                    LoadMediumControl( true );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                var service = new CommunicationTemplateService( rockContext );

                Rock.Model.CommunicationTemplate template = null;
                if ( CommunicationTemplateId.HasValue )
                {
                    template = service.Get( CommunicationTemplateId.Value );
                }

                bool newTemplate = false;
                if ( template == null )
                {
                    newTemplate = true;
                    template = new Rock.Model.CommunicationTemplate();
                    service.Add( template );
                }

                template.Name = tbName.Text;
                template.Description = tbDescription.Text;
                template.MediumEntityTypeId = MediumEntityTypeId;

                template.MediumData.Clear();
                GetMediumData();
                foreach(var keyVal in MediumData)
                {
                    if (!string.IsNullOrEmpty(keyVal.Value))
                    {
                        template.MediumData.Add(keyVal.Key, keyVal.Value);
                    }
                }

                if ( template.MediumData.ContainsKey( "Subject" ) )
                {
                    template.Subject = template.MediumData["Subject"];
                    template.MediumData.Remove( "Subject" );
                }
                else
                {
                    template.Subject = string.Empty;
                }

                if ( template != null )
                {
                    rockContext.SaveChanges();
                    NavigateToParentPage();
                }

                if ( newTemplate && !_canEdit )
                {
                    template.MakePrivate( Authorization.VIEW, CurrentPerson );
                    template.MakePrivate( Authorization.EDIT, CurrentPerson );
                }
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        private void ShowDetail( int templateId )
        {
            Rock.Model.CommunicationTemplate template = null;

            if ( !templateId.Equals( 0 ) )
            {
                template = new CommunicationTemplateService( new RockContext() )
                    .Queryable()
                    .Where( c => c.Id == templateId )
                    .FirstOrDefault();
                if ( template != null )
                {
                    lTitle.Text = template.Name.FormatAsHtmlTitle();
                }
            }

            if (template == null)
            {
                template = new Rock.Model.CommunicationTemplate();
                RockPage.PageTitle = "New Communication Template";
                lTitle.Text = "New Communication Template".FormatAsHtmlTitle();
            }

            CommunicationTemplateId = template.Id;

            tbName.Text = template.Name;
            tbDescription.Text = template.Description;

            MediumEntityTypeId = template.MediumEntityTypeId;
            BindMediums();

            MediumData = template.MediumData;
            MediumData.Add( "Subject", template.Subject );

            MediumControl control = LoadMediumControl( true );

        }

        /// <summary>
        /// Binds the mediums.
        /// </summary>
        private void BindMediums()
        {
            var mediums = new Dictionary<int, string>();
            foreach ( var item in MediumContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    mediums.Add( entityType.Id, item.Metadata.ComponentName );
                    if ( !MediumEntityTypeId.HasValue )
                    {
                        MediumEntityTypeId = entityType.Id;
                    }
                }
            }

            rptMediums.DataSource = mediums;
            rptMediums.DataBind();
        }

        /// <summary>
        /// Shows the medium.
        /// </summary>
        private MediumControl LoadMediumControl(bool setData)
        {
            phContent.Controls.Clear();

            // The component to load control for
            MediumComponent component = null;

            // Get the current medium type
            EntityTypeCache entityType = null;
            if ( MediumEntityTypeId.HasValue )
            {
                entityType = EntityTypeCache.Read( MediumEntityTypeId.Value );
            }

            foreach ( var serviceEntry in MediumContainer.Instance.Components )
            {
                var mediumComponent = serviceEntry.Value.Value;

                // Default to first component
                if ( component == null )
                {
                    component = mediumComponent;
                }

                // If invalid entity type, exit (and use first component found)
                if ( entityType == null )
                {
                    break;
                }
                else if ( entityType.Id == mediumComponent.EntityType.Id )
                {
                    component = mediumComponent;
                    break;
                }
            }

            if (component != null)
            {
                phContent.Controls.Clear();
                var mediumControl = component.GetControl( false );
                mediumControl.ID = "commControl";
                mediumControl.IsTemplate = true;
                mediumControl.ValidationGroup = btnSave.ValidationGroup;
                phContent.Controls.Add( mediumControl );

                if ( setData  )
                {
                    mediumControl.MediumData = MediumData;
                }
                
                // Set the medium in case it wasn't already set or the previous component type was not found
                MediumEntityTypeId = component.EntityType.Id;

                return mediumControl;
            }

            return null;
        }

        /// <summary>
        /// Gets the medium data.
        /// </summary>
        private void GetMediumData()
        {
            if ( phContent.Controls.Count == 1 && phContent.Controls[0] is MediumControl )
            {
                var mediumData = ( (MediumControl)phContent.Controls[0] ).MediumData;
                foreach ( var dataItem in mediumData )
                {
                    if ( MediumData.ContainsKey( dataItem.Key ) )
                    {
                        MediumData[dataItem.Key] = dataItem.Value;
                    }
                    else
                    {
                        MediumData.Add( dataItem.Key, dataItem.Value );
                    }
                }
            }
        }

        #endregion

    }
}
