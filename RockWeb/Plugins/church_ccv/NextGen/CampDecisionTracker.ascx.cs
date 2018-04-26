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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Web.UI.HtmlControls;

namespace RockWeb.Plugins.church_ccv.NextGen
{
    [DisplayName( "Camp Decision Tracker" )]
    [Category( "CCV > Next Gen" )]
    [Description( "Displays and tracks the various decisions that a student can make during Camp" )]
    public partial class CampDecisionTracker : Rock.Web.UI.RockBlock
    {
        const int sCampCoach_AttributeId = 16678;
        const int sCampEvent_GroupTypeId = 93;
        const int sDecisionsMade_AttributeId = 59115;
        
        static List<string> sCampDecisionsList;

        enum CampType
        {
            Kids,
            JuniorHigh,
            HighSchool
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            
            using ( RockContext rockContext = new RockContext( ) )
            {
                // Load the "Values" qualifier for the Camp Decisions attribute. This contains the actual choices that can be selected.
                AttributeService attribService = new AttributeService( rockContext );
                var decisionsMadeAttrib = attribService.Queryable( ).AsNoTracking( ).Where( a => a.Id == sDecisionsMade_AttributeId ).FirstOrDefault( );
                var decisionsVal = decisionsMadeAttrib.AttributeQualifiers.Where( aq => aq.Key == "values" ).FirstOrDefault( );

                sCampDecisionsList = decisionsVal.Value.Split(',').ToList( );
            }
        }
        
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            if ( !Page.IsPostBack )
            {
                using ( RockContext rockContext = new RockContext( ) )
                {
                    BuildStudentList( rockContext );
                }
            }
        }
        
        #region Events

        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                AddOrUpdateStudentDecisions( );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            using ( RockContext rockContext = new RockContext( ) )
            {
                BuildStudentList( rockContext );
            }
        }

        protected void rptStudents_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            // if the item being bound is an actual data item (vs the header or footer)
            if (e.Item.ItemType.Equals(ListItemType.AlternatingItem) || e.Item.ItemType.Equals(ListItemType.Item))
            {
                using ( RockContext rockContext = new RockContext( ) )
                {
                    // get the person associated with this repeater row
                    int? personId = DataBinder.Eval( e.Item.DataItem, "PersonId" ) as int?;
                    PersonService personService = new PersonService( rockContext );
                    Person personObj = personService.Get( personId.Value );

                    // set the image
                    Image studentImage = e.Item.FindControl("studentImg") as Image;
                    studentImage.ImageUrl = personObj.PhotoUrl;

                    //set the name
                    Literal pTag = e.Item.FindControl("studentName") as Literal;
                    pTag.Text = "<p>" + personObj.FullName + "</p>";

                    // set the group member id hidden field
                    int? groupMemberId = DataBinder.Eval( e.Item.DataItem, "GroupMemberId" ) as int?;
                    HiddenField hfGroupMemberId = e.Item.FindControl("studentGroupMemberId") as HiddenField;
                    hfGroupMemberId.Value = groupMemberId.Value.ToString( );
                    
                    
                    //set the decisions (and the actual values). First set the options
                    // to do this, first get the groupId so we can parse the decision checklist
                    int? groupId = DataBinder.Eval( e.Item.DataItem, "GroupId" ) as int?;
                    string campGroupName = (new GroupService( rockContext ).Get( groupId.Value )).Name;

                    List<string> campDecisions = GetDecisionsForCampType( sCampDecisionsList, campGroupName );
                    
                    CheckBoxList cblDecisions = e.Item.FindControl("studentDecisions") as CheckBoxList;
                    cblDecisions.DataSource = campDecisions;
                    cblDecisions.DataBind( );

                    // now set the values (if the student has made any)
                    AttributeValue decisionAV = DataBinder.Eval( e.Item.DataItem, "DecisionAV" ) as AttributeValue;
                    if( decisionAV != null )
                    {
                        cblDecisions.SetValues( decisionAV.Value.Split( ',' ) );

                        // and set the hidden field Attribute Value ID so we can directly update this attribute (vs creating a new one)
                        HiddenField hfAttribValueId = e.Item.FindControl("studentAttribValueId") as HiddenField;
                        hfAttribValueId.Value = decisionAV.Id.ToString( );
                    }
                }
            }
        }
        #endregion
        
        private void BuildStudentList( RockContext rockContext )
        {
            // get all personAlias guids for the logged in (Current) person. This is because our PA Guid is what stores as the Coach for the kid.
            var paQuery = new PersonAliasService( rockContext ).Queryable( ).AsNoTracking( );
            var paGuidList = paQuery.Where( pa => pa.PersonId == CurrentPerson.Id )
                                    .Select( pa => pa.Guid.ToString( ).ToLower( ) );

            // find all attribute values of type "Camp Coach" where the value is one of the PersonAlias Guids for the logged in (Current) person.
            // the EntityIds for each of these are the Person IDs of the students we're coaching
            var avQuery = new AttributeValueService( rockContext ).Queryable( ).AsNoTracking( );

            var studentPersonIds = avQuery.Where( av => av.AttributeId == sCampCoach_AttributeId && paGuidList.Contains( av.Value.ToLower( ) ) )
                                          .Select( av => av.EntityId );

            // get the "decisions made" values for each student. These are stored as Attribute Values tied to the Group Members of Camp Group types.
            
            // first, get all the camp groups for THIS YEAR
            var groupQuery = new GroupService( rockContext ).Queryable( ).AsNoTracking( );
            var campGroupIds = groupQuery.Where( g => g.GroupTypeId == sCampEvent_GroupTypeId && g.Name.Contains( DateTime.Now.Year.ToString( ) ) )
                                         .Select( cg => cg.Id );

            // next get the group Ids and groupMember Ids joined with the Person IDs for JUST students that we're a coach over
            var groupMemberQuery = new GroupMemberService( rockContext ).Queryable( ).AsNoTracking( );
            var groupMemberIdsWithPersonIds = groupMemberQuery.Where( gm => campGroupIds.Contains( gm.GroupId ) && studentPersonIds.Contains( gm.PersonId ) )
                                                              .Join( studentPersonIds, gm => gm.PersonId, 
                                                                                       spId => spId, 
                                                                                      ( gm, spId ) => new { gm = gm, spId } )
                                                              .Select( gm => new { GroupId = gm.gm.GroupId,
                                                                                   GroupMemberId = gm.gm.Id,
                                                                                   PersonId = gm.spId } );
            
            // finally, we'll tie the group ID, groupMember ID, personID and (if it exists) the decision AV into a single object.
            
            // first filter the av query down to just the decision AV that we care about (for performance)
            avQuery = avQuery.Where( av => av.AttributeId == sDecisionsMade_AttributeId );

            // now perform a 'left join' on students and their decision (if there is one)
            var studentList = groupMemberIdsWithPersonIds.GroupJoin( avQuery, gm => gm.GroupMemberId,
                                                                              a => a.EntityId,
                                                                              (gm, a) => new { gm, a } )
                                                         .Select( obj => new { PersonId = obj.gm.PersonId,
                                                                               GroupId = obj.gm.GroupId,
                                                                               GroupMemberId = obj.gm.GroupMemberId,
                                                                               DecisionAV = obj.a.FirstOrDefault( ) } );

            // now join to the person table simply so we can sort the kids by name (then we take only the PersonID, groupID, groupMemberID and decisionAV)
            var personQuery = new PersonService( rockContext ).Queryable( ).AsNoTracking( );
            var sortedList = studentList.Join( personQuery, s => s.PersonId, 
                                                            p => p.Id, 
                                                            (s, p ) => new { s = s, p = p } )
                                        .OrderBy( a => a.p.NickName )
                                        .Select( a => new { PersonId = a.p.Id,
                                                            GroupId = a.s.GroupId,
                                                            GroupMemberId = a.s.GroupMemberId,
                                                            DecisionAV = a.s.DecisionAV } )
                                        .ToList( );
            
            // now bind this data to our list, and in the DataBound pre-render callback, we'll 
            // render out the student info
            rptStudents.DataSource = sortedList;
            rptStudents.DataBind();
        }

        // Helper function to get all selected items as a comma delimited list.
        private string GetSelectedItems( CheckBoxList cblControl )
        {
            string selectedValues = "";

            for( int i = 0; i < cblControl.Items.Count; i++ )
            {
                // if selected, add the value and a comma
                if( cblControl.Items[i].Selected )
                {
                    selectedValues += cblControl.Items [ i ].Value + ",";
                }
            }

            // remove any trailing comma
            return selectedValues.TrimEnd(',');
        }

        private void AddOrUpdateStudentDecisions( )
        {
            // get all the student items
            var studentObjects = rptStudents.Controls.OfType<RepeaterItem>( );

            foreach ( var student in studentObjects )
            {
                // get the check box of decisions made by the student
                var cblDecisionsMade = student.Controls.OfType<CheckBoxList>( ).FirstOrDefault( );
                string decisionsVal = GetSelectedItems( cblDecisionsMade );

                // now write the Attribute Value Id for decisions made--even if it's empty
                using ( RockContext rockContext = new RockContext( ) )
                {
                    // first, see if there's an existing Attribute Value ID to use
                    var hfAttribValueId = student.Controls.OfType<HiddenField>( ).Where( c => c.ID == "studentAttribValueId" ).FirstOrDefault( );

                    int attribValueId = 0;
                    int.TryParse( hfAttribValueId.Value, out attribValueId );
                    
                    AttributeValueService avService = new AttributeValueService( rockContext );
                    AttributeValue avItem = null;

                    // if we have a valid ID, load that attribute value
                    if( attribValueId > 0 )
                    {
                        avService.TryGet( attribValueId, out avItem );
                    }
                    else
                    {
                        // otherwise, we'll create a new attribute value (which is tied to the group member ID)

                        // first get the hidden field holding this student's group member Id
                        var hfGroupMemberId = student.Controls.OfType<HiddenField>( ).Where( c => c.ID == "studentGroupMemberId" ).FirstOrDefault( );
                        int groupMemberId = int.Parse( hfGroupMemberId.Value );

                        // now create a new attribute value tied to the group member, and of attribute type "Decisions Made"
                        avItem = new AttributeValue( );
                        avItem.EntityId = groupMemberId;
                        avItem.AttributeId = sDecisionsMade_AttributeId;
                        avService.Add( avItem );
                    }

                    // now whether we created it or read it, set the value and save
                    avItem.Value = decisionsVal;
                    rockContext.SaveChanges( );
                }
            }
        }

        private List<string> GetDecisionsForCampType( List<string> availableDecisions, string campGroupName )
        {
            // based on the camp's group name, it's either a High School, Junior High, or Kids Camp.

            // Junior High and Kids camps have a subset of the full High School list. So,
            // we can default to the full list, and then simply trim off 1 or 2 based on the camp.
            int numDecisions = availableDecisions.Count;

            if ( campGroupName.ToLower( ).Contains( "junior high" ) )
            {
                numDecisions--; //junior high has one less decision that high school
            }
            else if ( campGroupName.ToLower( ).Contains( "kids" ) )
            {
                numDecisions -= 2; //kids has two less than high school
            }
            
            return availableDecisions.Take( numDecisions ).ToList( );
        }
    }
}