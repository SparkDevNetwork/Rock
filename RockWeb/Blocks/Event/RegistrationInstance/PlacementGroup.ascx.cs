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
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Event.RegistrationInstance
{
    [DisplayName( "Registration Instance Placement Group" )]
    [Category( "Event" )]
    [Description( "Block to manage group placement for Registration Instances." )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class PlacementGroup : RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            //public const string ShowEmailAddress = "ShowEmailAddress";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
        }

        #endregion PageParameterKeys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            // maybe JSON??
            public const string PlacementConfigurationJSON = "PlacementConfigurationJSON";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js", true );

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
                ShowDetails();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            hfRegistrationTemplateId.Value = this.PageParameter( PageParameterKey.RegistrationTemplateId );
            hfRegistrationInstanceId.Value = this.PageParameter( PageParameterKey.RegistrationTemplateId );

            var rockContext = new RockContext();

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            var registrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();

            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();

            List<Rock.Model.RegistrationInstance> registrationInstanceList;

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                registrationInstanceList = new List<Rock.Model.RegistrationInstance>();
                registrationInstanceList.Add( registrationInstance );
            }
            else if ( registrationTemplateId.HasValue )
            {
                registrationInstanceList = registrationInstanceService.Queryable().Where( a => a.RegistrationTemplateId == registrationTemplateId.Value ).OrderBy( a => a.Name ).ToList();
            }

            //rptPlacementGroups.DataSource = 
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

        }

        protected void btnAddPlacementGroup_Click( object sender, EventArgs e )
        {

        }

        protected void btnConfiguration_Click( object sender, EventArgs e )
        {

        }

        protected void rptPlacementGroups_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {

        }

        #endregion
    }
}