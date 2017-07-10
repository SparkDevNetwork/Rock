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
using church.ccv.SafetySecurity.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.church_ccv.SafetySecurity
{
    /// <summary>
    /// Block for users to create, edit, and view benevolence requests.
    /// </summary>
    [DisplayName( "Volunteer Screening Detail" )]
    [Category( "CCV > Safety and Security" )]
    [Description( "Block for users to create, edit, and volunteer screening instances." )]
    public partial class VolunteerScreeningDetail : Rock.Web.UI.RockBlock
    {
        private List<int> DocumentsState { get; set; }
        
        // Define the IDs for the various workflows needed
        const int sBackgroundCheck_WorkflowId = 26;
        const int sCharacterReference_WorkflowId = 203;
        const int sSendNewCharacterReference_WorkflowId = 213;
        const int sResendApplication_WorkflowId = 231;

        private int VSInstanceId { get; set; }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            this.AddConfigurationUpdateTrigger( upnlContent );

            gCharacterRefs.DataKeyNames = new string[] { "Id" };
            DocumentsState = new List<int>( );

            gCharacterRefs.Actions.ShowAdd = true;
            gCharacterRefs.Actions.AddClick += gCharacterRef_AddClick;

            // setup the legacy application file uploader's filetype
            fu_legAppFile.BinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
        }

        protected void gCharacterRef_AddClick( object sender, EventArgs e )
        {
            // get the volunteer screening instance
            using ( RockContext rockContext = new RockContext( ) )
            {
                // let it throw an exception if any of these objects come back null
                VolunteerScreening vsInstance = new Service<VolunteerScreening>( rockContext ).Get( VSInstanceId );
                
                // setup the person info / Application Instance Header details
                PersonAlias personAlias = new PersonAliasService( rockContext ).Get( vsInstance.PersonAliasId );
                Person person = personAlias.Person;

                Dictionary<string, string> pageParams = new Dictionary<string, string>( );
                pageParams.Add( "WorkflowTypeId", sSendNewCharacterReference_WorkflowId.ToString( ) );
                pageParams.Add( "ApplicationWorkflowId",vsInstance.Application_WorkflowId.ToString( ) );
                pageParams.Add( "ApplicantFirstName", person.FirstName );
                pageParams.Add( "ApplicantLastName", person.LastName );
                pageParams.Add( "VolunteerScreeningInstanceId", vsInstance.Id.ToString( ) );
                pageParams.Add( "ApplicantPrimaryAliasId", vsInstance.PersonAliasId.ToString( ) );
                
                PageReference pageRef = new PageReference( new Uri( ResolveRockUrlIncludeRoot("~/WorkflowEntry/" + sSendNewCharacterReference_WorkflowId.ToString( ) ) ), HttpContext.Current.Request.ApplicationPath );
                pageRef.Parameters = pageParams;

                NavigateToPage( pageRef );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            VSInstanceId = PageParameter( "VolunteerScreeningInstanceId" ).AsInteger();
            
            // load the VolunteerScreening instance
            if ( Page.IsPostBack == false )
            {
                ShowDetail( VSInstanceId );
            }
        }
        
        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            DocumentsState = ViewState["DocumentsState"] as List<int>;
            if ( DocumentsState == null )
            {
                DocumentsState = new List<int>();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DocumentsState"] = DocumentsState;

            return base.SaveViewState();
        }
        #endregion

        protected void ShowDetail( int vsInstanceId )
        {
            using ( RockContext rockContext = new RockContext( ) )
            {
                VolunteerScreening vsInstance = new Service<VolunteerScreening>( rockContext ).Get( vsInstanceId );
                if( vsInstance != null )
                {
                    // setup the person info / Application Instance Header details
                    PersonAlias personAlias = new PersonAliasService( rockContext ).Get( vsInstance.PersonAliasId );
                    Person person = personAlias.Person;

                    lPersonName.Text = "<a href=/Person/" + person.Id + ">" + person.FullName + "</a>";
                    lDateApplicationSent.Text = "Date Sent to Applicant: " + vsInstance.CreatedDateTime.Value.ToShortDateString( );

                    if( vsInstance.Type == VolunteerScreening.Types.Normal.ConvertToInt( ) )
                    {
                        pNewScreening.Visible = true;
                        pLegacy.Visible = false;
                                                
                        // setup the background check info
                        if( vsInstance.BGCheck_Result_Date.HasValue )
                        {
                            lBGCheck_Date.Text = vsInstance.BGCheck_Result_Date.Value.ToShortDateString( );
                        }
                        else
                        {
                            lBGCheck_Date.Text = "Pending";
                        }

                        if( vsInstance.BGCheck_Result_DocGuid.HasValue )
                        {
                            lBGCheck_Doc.Text = "<a href=/GetFile.ashx?guid=" + vsInstance.BGCheck_Result_DocGuid.Value + ">View Document</a>";
                        }
                        else
                        {
                            lBGCheck_Doc.Text = "Pending";
                        }

                        // if they have an actual Result, display it and hide links to view / kickoff a request
                        if( string.IsNullOrWhiteSpace( vsInstance.BGCheck_Result_Value ) == false )
                        {
                            lBGCheck_Link.Text = "Background Check Complete";

                            lBGCheck_Result.Text = vsInstance.BGCheck_Result_Value;
                        }
                        else
                        {
                            lBGCheck_Result.Text = "Pending";

                            // assume we'll link to launching a new background check, and all fields should be hidden
                            string bgCheckText = "<a href=/WorkflowEntry/" + sBackgroundCheck_WorkflowId.ToString( ) + "?PersonId=" + person.Id + "&VolunteerScreeningInstanceId=" + vsInstance.Id + ">Request Background Check</a>";

                            // but if there's a result, link them to the one in progress
                            List<int?> attribIds = new AttributeValueService( rockContext ).Queryable( ).AsNoTracking( ).Where( av => av.Attribute.Key == "VolunteerScreeningInstanceId" && av.ValueAsNumeric == vsInstance.Id ).Select( av => av.EntityId ).ToList( );
                            if( attribIds.Count > 0 )
                            { 
                                Workflow bgCheckWorkflow = new WorkflowService( rockContext ).Queryable( ).AsNoTracking( ).Where( wf => wf.WorkflowTypeId == sBackgroundCheck_WorkflowId && attribIds.Contains( wf.Id ) ).SingleOrDefault( );
                                if ( bgCheckWorkflow != null )
                                {
                                    // since there is one, let them view the date and doc (which may or may not be filled in)
                                    bgCheckText = "<a href=/WorkflowEntry/" + sBackgroundCheck_WorkflowId.ToString( ) + "/" + bgCheckWorkflow.Id + ">Background Check In Progress</a>";
                                }
                            }
                            
                            lBGCheck_Link.Text = bgCheckText;
                        }

                        // setup the application info
                        ShowApplicationInfo( rockContext, vsInstance, person );
                    }
                    else
                    {
                        pNewScreening.Visible = false;
                        pLegacy.Visible = true;
                    }
                }
            }
        }

        protected void ShowApplicationInfo( RockContext rockContext, VolunteerScreening vsInstance, Person person )
        {
            if( vsInstance.Application_WorkflowId.HasValue )
            {
                WorkflowService workflowService = new WorkflowService( rockContext );

                // first, get the application workflow
                Workflow applicationWorkflow = workflowService.Queryable( ).AsNoTracking( ).Where( wf => wf.Id == vsInstance.Application_WorkflowId ).SingleOrDefault( );

                if( applicationWorkflow.Status == "Completed" )
                {
                    lApplicationWorkflow.Text = "<a href=/page/1534?workflowId=" + vsInstance.Application_WorkflowId.Value + ">View Completed Application</a>";
                    lDateApplicationCompleted.Text = "Date Approved by CCV: " + vsInstance.ModifiedDateTime.Value.ToShortDateString( );
                }
                else
                {
                    // this text should only show if the modified time is greater, which means it was received back from the applicant.
                    if( vsInstance.ModifiedDateTime > vsInstance.CreatedDateTime )
                    {
                        lApplicationWorkflow.Text = "<a href=/WorkflowEntry/" + vsInstance.Application_WorkflowTypeId.Value + "/" + vsInstance.Application_WorkflowId.Value + ">Review Application</a>";
                        lDateApplicationCompleted.Text = "Date Returned to CCV: " + vsInstance.ModifiedDateTime.Value.ToShortDateString( );
                    }
                    else
                    {

                        lApplicationWorkflow.Text = "<a href=/WorkflowEntry/" + sResendApplication_WorkflowId.ToString( );

                        // now add the query params needed to re-send this application
                        lApplicationWorkflow.Text += string.Format( "?ApplicationWorkflowGuid={0}&ApplicantEmail={1}&ApplicantFirstName={2}&ApplicantLastName={3}&SourceVolunteerScreeningId={4}&ApplicationWorkflowTypeId={5}>", applicationWorkflow.Guid, person.Email, person.FirstName, person.LastName, vsInstance.Id, applicationWorkflow.WorkflowTypeId );
                        lApplicationWorkflow.Text += "Resend Application</a>";
                        lDateApplicationCompleted.Text = "Application has not yet been returned.";
                    }
                }
                
                // to know if we should show character references, see if any character reference workflows tied to this Screening Instance exist.
                List<Workflow> charRefWorkflows = new List<Workflow>( );
                List<int?> attribIds = new AttributeValueService( rockContext ).Queryable( ).AsNoTracking( ).Where( av => av.Attribute.Key == "VolunteerScreeningInstanceId" && av.ValueAsNumeric == vsInstance.Id ).Select( av => av.EntityId ).ToList( );
                if( attribIds.Count > 0 )
                { 
                    charRefWorkflows = workflowService.Queryable( ).AsNoTracking( ).Where( wf => wf.WorkflowTypeId == sCharacterReference_WorkflowId && attribIds.Contains( wf.Id ) ).ToList( );
                }

                if ( charRefWorkflows.Count > 0 )
                {
                    // load the attributes for all of the reference workflows
                    foreach( Workflow workflow in charRefWorkflows )
                    {
                        workflow.LoadAttributes( );
                    }

                    lNoCharacterRefs.Visible = false;
                    gCharacterRefs.Visible = true;

                    gCharacterRefs.DataSource = charRefWorkflows.Select( cr => new
                    {
                        // IsSystem is used as a flag to disable the delete button.
                        // we don't allow completed character references to be deleted, so 
                        // set it to true in that case
                        IsSystem = cr.Status == "Completed" ? true : false,

                        Id = cr.Id,
                        WorkflowId = cr.Id,
                        WorkflowText = "View Form",
                        
                        State = cr.Status == "Active" ? "Waiting for Response" : "Responded",

                        // setup the values for re-sending this reference
                        ResendReference = "Click to Resend",


                        CharacterReferenceWorkflowGuid = cr.Guid,
                        ApplicantFirstName = person.FirstName,
                        ApplicantLastName = person.LastName,
                        SourceVolunteerScreeningId = vsInstance.Id,
                        ReferenceEmail = cr.AttributeValues["EmailAddress"].Value,
                        Type = cr.AttributeValues["Type"].Value,
                        LastDateSent = cr.AttributeValues["LastSentDate"].Value,
                        
                        PersonText = cr.AttributeValues["FirstName"].Value + " " + cr.AttributeValues["LastName"].Value
                        
                    } );

                    gCharacterRefs.DataBind( );
                }
                else
                {
                    gCharacterRefs.Visible = false;
                    lNoCharacterRefs.Visible = true;
                }
            }
            else
            {
                lApplicationWorkflow.Text = "No Application Available";
                gCharacterRefs.Visible = false;
            }
        }

        protected void CharacterRef_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            WorkflowService workflowService = new WorkflowService( rockContext );
            Workflow workflow = workflowService.Get( e.RowKeyId );
            if ( workflow != null )
            {
                workflowService.Delete( workflow );
                rockContext.SaveChanges();

                ShowDetail( VSInstanceId );
            }
        }
                
        // ---- Legacy Application Document File Uploader ---
        protected void fu_legAppFile_FileUploaded( object sender, EventArgs e )
        {
        }

        protected void fu_legAppFile_FileRemoved( object sender, EventArgs e )
        {
        }
        // ----

        // ---- Legacy Character Reference File Uploader ----
        //protected void dl_legCharRefDocs_ItemDataBound( object sender, DataListItemEventArgs e )
        //{
        //    Guid binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
        //    var fileupDoc = e.Item.FindControl( "fu_legCharRefDocs" ) as Rock.Web.UI.Controls.FileUploader;
        //    if ( fileupDoc != null )
        //    {
        //        fileupDoc.BinaryFileTypeGuid = binaryFileTypeGuid;
        //    }
        //}

        //protected void fu_legCharRefDocs_FileUploaded( object sender, EventArgs e )
        //{
        //    var fileUpDoc = (Rock.Web.UI.Controls.FileUploader)sender;

        //    if ( fileUpDoc.BinaryFileId.HasValue )
        //    {
        //        DocumentsState.Add( fileUpDoc.BinaryFileId.Value );
        //        BindDocuments_LegCharRefDocs( true );
        //    }
        //}
        
        //protected void fu_legCharRefDocs_FileRemoved( object sender, FileUploaderEventArgs e )
        //{
        //    var fileUpDoc = (Rock.Web.UI.Controls.FileUploader)sender;
        //    if ( e.BinaryFileId.HasValue )
        //    {
        //        DocumentsState.Remove( e.BinaryFileId.Value );
        //        BindDocuments_LegCharRefDocs( true );
        //    }
        //}

        //protected void BindDocuments_LegCharRefDocs( bool canEdit )
        //{
        //    var ds = DocumentsState.ToList();

        //    if ( ds.Count() < 6 )
        //    {
        //        ds.Add( 0 );
        //    }

        //    dl_legCharRefDocs.DataSource = ds;
        //    dl_legCharRefDocs.DataBind();
        //}
        // ----
    }
}