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
using church.ccv.SafetySecurity.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Plugins.church_ccv.SafetySecurity
{
    [DisplayName( "Volunteer Screening Character Reference Detail" )]
    [Category( "CCV > Safety and Security" )]
    [Description( "Displays a given Character Reference." )]
    
    public partial class VolunteerScreening_CharacterReferenceDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                using ( RockContext rockContext = new RockContext( ) )
                {
                    Render( rockContext );
                }
            }
        }
        
        #endregion
    

        #region Internal Methods

        private void Render( RockContext rockContext )
        {
            int charWorkflowId = PageParameter( "CharacterReferenceWorkflowId" ).AsInteger();

            // grab the character reference
            Workflow charRefWorkflow = new WorkflowService( rockContext ).Queryable( ).AsNoTracking( ).Where( wf => wf.Id == charWorkflowId ).SingleOrDefault( );

            charRefWorkflow.LoadAttributes( );
            
            lHeader.Text = "<h4>Information (Number: " + charRefWorkflow.AttributeValues["CompletionNumber"].Value + ")</h4>";

            // set the character reference type (professional, non-family, etc.)
            lType.Text = "Relationship: " + charRefWorkflow.AttributeValues["Type"];

            // grab the people associated
            IQueryable<PersonAlias> paQuery = new PersonAliasService( rockContext ).Queryable( ).AsNoTracking( );

            int applicantPrimaryAliasId = charRefWorkflow.AttributeValues["ApplicantPrimaryAliasId"].Value.AsInteger( );
            PersonAlias applicant = paQuery.Where( pa => pa.Id == applicantPrimaryAliasId ).SingleOrDefault( );
                        
            // set the applicant
            lApplicant.Text = string.Format( "<a href=\"/Person/{0}\">{1}</a>", applicant.Person.Id, applicant.Person.FullName );

            // set the reference
            lReference.Text = "Name: " + charRefWorkflow.AttributeValues["FirstName"] + " " + charRefWorkflow.AttributeValues["LastName"];
            lReferenceEmail.Text = "Email: " + charRefWorkflow.AttributeValues["EmailAddress"];
            lReferencePhoneNumber.Text = "Phone: " + charRefWorkflow.AttributeValues["PhoneNumber"];
            

            if ( charRefWorkflow.Status == "Completed" )
            {
                pNoResponse.Visible = false;
                pCharReferenceFeedback.Visible = true;

                // set the responses

                // Length Known
                lFeedback_LengthKnown.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_LengthKnown"].AttributeName + "</strong>" + "<br>" + 
                                                          charRefWorkflow.AttributeValues["Feedback_LengthKnown"];

                // Capacity Known
                lFeedback_CapacityKnown.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_CapacityKnown"].AttributeName + "</strong>" + "<br>" + 
                                                          charRefWorkflow.AttributeValues["Feedback_CapacityKnown"];

                // three to five traits
                lFeedback_ThreeToFiveTraits.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_3To5Traits"].AttributeName + "</strong>" + "<br>" + 
                                                   charRefWorkflow.AttributeValues["Feedback_3To5Traits"];

                // Moral Integrity
                lFeedback_MoralIntegrity.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_MoralIntegrity"].AttributeName + "</strong>" + "<br>" + 
                                                charRefWorkflow.AttributeValues["Feedback_MoralIntegrity"];

                lFeedback_MoralIntegrityReason.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_MoralIntegrity_Reason"].AttributeName + "</strong>" + "<br>" + 
                                                      charRefWorkflow.AttributeValues["Feedback_MoralIntegrity_Reason"];

                // Around Children
                lFeedback_AroundChildren.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_AroundChildren"].AttributeName + "</strong>" + "<br>" + 
                                                charRefWorkflow.AttributeValues["Feedback_AroundChildren"];

                lFeedback_AroundChildrenDesc.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_AroundChildren_Desc"].AttributeName + "</strong>" + "<br>" + 
                                                    charRefWorkflow.AttributeValues["Feedback_AroundChildren_Desc"];

                // Commitment
                lFeedback_Commitment.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_Commitment"].AttributeName + "</strong>" + "<br>" + 
                                            charRefWorkflow.AttributeValues["Feedback_Commitment"];

                lFeedback_CommitmentAdditional.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_Commitment_Additional"].AttributeName + "</strong>" + "<br>" + 
                                                      charRefWorkflow.AttributeValues["Feedback_Commitment_Additional"];

                // Trustworth with Children
                lFeedback_TrustworthyWithChildren.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_TrustworthyWithChildren"].AttributeName + "</strong>" + "<br>" + 
                                                         charRefWorkflow.AttributeValues["Feedback_TrustworthyWithChildren"];

                lFeedback_TrustworthyWithChildrenReason.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_TrustworthyWithChildren_Reason"].AttributeName + "</strong>" + "<br>" + 
                                                               charRefWorkflow.AttributeValues["Feedback_TrustworthyWithChildren_Reason"];

                // Overall rating
                lFeedback_OverallRating.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_OverallRating"].AttributeName + "</strong>" + "<br>" + 
                                                         charRefWorkflow.AttributeValues["Feedback_OverallRating"];

                // Additional Comments
                lFeedback_AdditionalComments.Text = "<strong>" + charRefWorkflow.AttributeValues["Feedback_AdditionalComments"].AttributeName + "</strong>" + "<br>" + 
                                                         charRefWorkflow.AttributeValues["Feedback_AdditionalComments"];
            }
            else
            {
                pNoResponse.Visible = true;
                pCharReferenceFeedback.Visible = false;
            }
        }
        
        #endregion
    }
}
