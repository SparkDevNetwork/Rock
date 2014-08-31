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
        /// Gets or sets the channel entity type id.
        /// </summary>
        /// <value>
        /// The channel entity type id.
        /// </value>
        protected int? ChannelEntityTypeId
        {
            get { return ViewState["ChannelEntityTypeId"] as int?; }
            set { ViewState["ChannelEntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        protected Dictionary<string, string> ChannelData
        {
            get 
            {
                var channelData = ViewState["ChannelData"] as Dictionary<string, string>;
                if ( channelData == null )
                {
                    channelData = new Dictionary<string, string>();
                    ViewState["ChannelData"] = channelData;
                }
                return channelData;
            }

            set { ViewState["ChannelData"] = value; }
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
                LoadChannelControl( false );
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
        /// Handles the Click event of the lbChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbChannel_Click( object sender, EventArgs e )
        {
            GetChannelData();
            var linkButton = sender as LinkButton;
            if ( linkButton != null )
            {
                int channelId = int.MinValue;
                if ( int.TryParse( linkButton.CommandArgument, out channelId ) )
                {
                    ChannelEntityTypeId = channelId;
                    BindChannels();

                    LoadChannelControl( true );
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
                template.ChannelEntityTypeId = ChannelEntityTypeId;

                template.ChannelData.Clear();
                GetChannelData();
                foreach(var keyVal in ChannelData)
                {
                    if (!string.IsNullOrEmpty(keyVal.Value))
                    {
                        template.ChannelData.Add(keyVal.Key, keyVal.Value);
                    }
                }

                if ( template.ChannelData.ContainsKey( "Subject" ) )
                {
                    template.Subject = template.ChannelData["Subject"];
                    template.ChannelData.Remove( "Subject" );
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

            ChannelEntityTypeId = template.ChannelEntityTypeId;
            BindChannels();

            ChannelData = template.ChannelData;
            ChannelData.Add( "Subject", template.Subject );

            ChannelControl control = LoadChannelControl( true );

        }

        /// <summary>
        /// Binds the channels.
        /// </summary>
        private void BindChannels()
        {
            var channels = new Dictionary<int, string>();
            foreach ( var item in ChannelContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    channels.Add( entityType.Id, entityType.FriendlyName );
                    if ( !ChannelEntityTypeId.HasValue )
                    {
                        ChannelEntityTypeId = entityType.Id;
                    }
                }
            }

            rptChannels.DataSource = channels;
            rptChannels.DataBind();
        }

        /// <summary>
        /// Shows the channel.
        /// </summary>
        private ChannelControl LoadChannelControl(bool setData)
        {
            phContent.Controls.Clear();

            // The component to load control for
            ChannelComponent component = null;

            // Get the current channel type
            EntityTypeCache entityType = null;
            if ( ChannelEntityTypeId.HasValue )
            {
                entityType = EntityTypeCache.Read( ChannelEntityTypeId.Value );
            }

            foreach ( var serviceEntry in ChannelContainer.Instance.Components )
            {
                var channelComponent = serviceEntry.Value.Value;

                // Default to first component
                if ( component == null )
                {
                    component = channelComponent;
                }

                // If invalid entity type, exit (and use first component found)
                if ( entityType == null )
                {
                    break;
                }
                else if ( entityType.Id == channelComponent.EntityType.Id )
                {
                    component = channelComponent;
                    break;
                }
            }

            if (component != null)
            {
                phContent.Controls.Clear();
                var channelControl = component.Control;
                channelControl.ID = "commControl";
                channelControl.IsTemplate = true;
                channelControl.ValidationGroup = btnSave.ValidationGroup;
                phContent.Controls.Add( channelControl );

                if ( setData  )
                {
                    channelControl.ChannelData = ChannelData;
                }
                
                // Set the channel in case it wasn't already set or the previous component type was not found
                ChannelEntityTypeId = component.EntityType.Id;

                return channelControl;
            }

            return null;
        }

        /// <summary>
        /// Gets the channel data.
        /// </summary>
        private void GetChannelData()
        {
            if ( phContent.Controls.Count == 1 && phContent.Controls[0] is ChannelControl )
            {
                var channelData = ( (ChannelControl)phContent.Controls[0] ).ChannelData;
                foreach ( var dataItem in channelData )
                {
                    if ( ChannelData.ContainsKey( dataItem.Key ) )
                    {
                        ChannelData[dataItem.Key] = dataItem.Value;
                    }
                    else
                    {
                        ChannelData.Add( dataItem.Key, dataItem.Value );
                    }
                }
            }
        }

        #endregion

    }
}
