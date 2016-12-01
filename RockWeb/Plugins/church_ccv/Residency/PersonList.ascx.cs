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
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident List" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays a list of all of the residents for a period, along with a summary of their compentencies and projects" )]

    [LinkedPage( "Detail Page" )]
    [BooleanField( "Show Add", "", true )]
    [BooleanField( "Show Delete", "", true )]
    public partial class PersonList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.DataKeyNames = new string[] { "id" };
            gList.Actions.ShowAdd = true;
            gList.Actions.AddClick += gList_Add;
            gList.GridRebind += gList_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            gList.Actions.ShowAdd = canAddEditDelete && this.GetAttributeValue( "ShowAdd" ).AsBoolean();
            gList.IsDeleteEnabled = canAddEditDelete && this.GetAttributeValue( "ShowDelete" ).AsBoolean();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PersonId", 0, "GroupId", hfGroupId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PersonId", e.RowKeyId, "GroupId", hfGroupId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new Rock.Data.RockContext();
            var residencyContext = new ResidencyContext();

            var groupMemberService = new GroupMemberService( rockContext );
            int personId = e.RowKeyId;
            int groupId = hfGroupId.ValueAsInt();
            var group = new GroupService( rockContext ).Get( groupId );
            int? groupRoleId = null;
            if ( group != null )
            {
                groupRoleId = group.GroupType.DefaultGroupRoleId;
            }

            GroupMember groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( groupId, personId, groupRoleId ?? 0 );
            if ( groupMember != null )
            {
                // check if person can be removed from the Group and also check if person can be removed from all the person assigned competencies
                string errorMessage;
                if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                var competencyPersonService = new ResidencyService<CompetencyPerson>( residencyContext );
                var personCompetencyList = competencyPersonService.Queryable().Where( a => a.PersonId.Equals( groupMember.PersonId ) );
                foreach ( var item in personCompetencyList )
                {
                    if ( !competencyPersonService.CanDelete( item, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                }

                // if you made it this far, delete all person's assigned competencies, and finally delete from Group
                foreach ( var item in personCompetencyList )
                {
                    competencyPersonService.Delete( item );
                }

                residencyContext.SaveChanges();

                groupMemberService.Delete( groupMember );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var residencyContext = new ResidencyContext();

            var residencyGroupMemberService = new ResidencyService<Rock.Model.GroupMember>( residencyContext );

            int residencyGroupId = PageParameter( "groupId" ).AsInteger();
            hfGroupId.SetValue( residencyGroupId );

            var residencyGroupMemberList = residencyGroupMemberService.Queryable( "Person" )
                .Where( a => a.GroupId.Equals( residencyGroupId ) ).ToList();

            var competencyPersonService = new ResidencyService<CompetencyPerson>( residencyContext );
            var competencyPersonQry = competencyPersonService.Queryable( "Competency,CompetencyPersonProjects" ).GroupBy( a => a.PersonId );

            var competencyPersonProjectQry = new ResidencyService<CompetencyPersonProject>( residencyContext ).Queryable().GroupBy( a => a.CompetencyPerson.PersonId )
                .Select( x => new
            {
                PersonId = x.Key,
                MinAssessmentCountTotal = x.Sum( nn => nn.MinAssessmentCount ?? nn.Project.MinAssessmentCountDefault ),
                CompletedProjectAssessmentsTotal = x.Sum( dd => dd.CompetencyPersonProjectAssessments.Where( nn => nn.AssessmentDateTime != null ).Count() )
            } ).ToList();

            var groupMemberCompetencies = from groupMember in residencyGroupMemberList
                                          join competencyList in competencyPersonQry on groupMember.PersonId
                                          equals competencyList.Key into groupJoin
                                          from qryResult in groupJoin.DefaultIfEmpty()
                                          select new
                                          {
                                              GroupMember = groupMember,
                                              ResidentCompentencies = qryResult != null ? qryResult.ToList() : null
                                          };

            var dataResult = groupMemberCompetencies.Select( a => new
            {
                Id = a.GroupMember.PersonId,
                FullName = a.GroupMember.Person.FullName,
                CompetencyCount = a.ResidentCompentencies == null ? 0 : a.ResidentCompentencies.Count(),
                CompletedProjectAssessmentsTotal = competencyPersonProjectQry.Where( g => g.PersonId == a.GroupMember.PersonId ).Select( g => g.CompletedProjectAssessmentsTotal ).FirstOrDefault(),
                MinAssessmentCount = competencyPersonProjectQry.Where( g => g.PersonId == a.GroupMember.PersonId ).Select( g => g.MinAssessmentCountTotal ?? 0 ).FirstOrDefault()
            } );

            SortProperty sortProperty = gList.SortProperty;

            if ( sortProperty != null )
            {
                gList.DataSource = dataResult.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gList.DataSource = dataResult.ToList().OrderBy( s => s.FullName ).ToList();
            }

            gList.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}