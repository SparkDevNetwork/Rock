﻿// <copyright>
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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Detail Lava" )]
    [Category( "Groups" )]
    [Description( "Presents the details of a group using Lava" )]
    [BooleanField("Enable Debug", "Shows the fields available to merge in lava.", false)]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupDetail.lava' %}" )]
    public partial class GroupDetailLava : Rock.Web.UI.RockBlock
    {
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
                DisplayConent();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayConent();
        }

        #endregion

        #region Methods

        private void DisplayConent() {

            int groupId = -1;
            
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
            }

            if ( groupId != -1 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                var group = groupService.Queryable( "GroupLocations,Members,Members.Person" ).Where( g => g.Id == groupId ).FirstOrDefault();

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", group );
                mergeFields.Add( "CurrentPerson", CurrentPerson );
                var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
                globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                string template = GetAttributeValue( "LavaTemplate" );

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }

                lContent.Text = template.ResolveMergeFields( mergeFields );
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }

        }

        #endregion
    }
}