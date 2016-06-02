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

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Takes a defined type and returns all defined values and merges them with a liquid template
    /// </summary>
    [DisplayName( "Family Members" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Displays the family members of a person" )]

    [BooleanField( "Can Display", "Whether the family member panel will be displayed.", true )]
    [BooleanField( "Can Edit", "Whether a user can edit his family members.", true )]
    [LinkedPage( "Detail Page", "The page to navigate to for details.", false, "", "", 0 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display content", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_centralaz/Widgets/Lava/FamilyMembers.lava' %}", "", 2, "LavaTemplate" )]
    [BooleanField( "Enable Debug", "Show merge data to help you see what's available to you.", order: 3 )]
    public partial class FamilyMembers : Rock.Web.UI.RockBlock
    {
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

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected void LoadContent()
        {
            if ( CurrentPerson != null )
            {
                if ( GetAttributeValue( "CanDisplay" ).AsBoolean() )
                {
                    RockContext rockContext = new RockContext();
                    Guid familyGroupGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                    int personId = this.CurrentPersonId.Value;

                    var memberService = new GroupMemberService( rockContext );
                    List<Family> families = memberService.Queryable( true )
                                .Where( m =>
                                    m.PersonId == personId &&
                                    m.Group.GroupType.Guid == familyGroupGuid )
                                .Select( m => new Family
                                {
                                    FamilyId = m.Group.Id,
                                    Name = m.Group.Name,
                                    CanEdit = m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) )
                                } )
                                .ToList();

                    foreach ( Family family in families )
                    {
                        var members = new GroupMemberService( rockContext ).Queryable( "Person", true )
                                    .Where( m =>
                                        m.GroupId == family.FamilyId &&
                                        m.PersonId != personId )
                                        .Select( m => new FamilyMember
                                        {
                                            MemberId = m.Id,
                                            Person = m.Person
                                        } )
                                    .OrderBy( fm => fm.Person.BirthDate )
                                    .ToList();

                        family.FamilyMembers = members;
                    }

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "Families", families );
                    mergeFields.Add( "DetailPage", LinkedPageUrl( "DetailPage", null ) );
                    mergeFields.Add( "CanEdit", GetAttributeValue( "CanEdit" ).AsBoolean() );

                    string template = GetAttributeValue( "LavaTemplate" );
                    lContent.Text = template.ResolveMergeFields( mergeFields );

                    // show debug info
                    if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                    {
                        lDebug.Visible = true;
                        lDebug.Text = mergeFields.lavaDebugInfo();
                    }
                }

            }
        }

        #endregion

        #region Helper Classes

        [DotLiquid.LiquidType( "FamilyId", "CanEdit", "Name", "FamilyMembers" )]
        public class Family
        {
            public int FamilyId { get; set; }

            public String Name { get; set; }

            public bool CanEdit { get; set; }

            public List<FamilyMember> FamilyMembers { get; set; }
        }

        [DotLiquid.LiquidType( "MemberId", "Person" )]
        public class FamilyMember
        {
            public int MemberId { get; set; }

            public Person Person { get; set; }
        }

        #endregion
    }
}