using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.SafetySecurity.Model
{
        [Table( "_church_ccv_SafetySecurity_VolunteerScreening" )]
        [DataContract]
        public class VolunteerScreening : Model<VolunteerScreening>, IRockEntity
        {
            public enum Types
            {
                Legacy,
                Normal
            }

            [DataMember]
            public int PersonAliasId { get; set; }

            [DataMember]
            public int Type { get; set; }

            [DataMember]
            public int? Application_WorkflowTypeId { get; set; }

            [DataMember]
            public int? Application_WorkflowId { get; set; }

            [DataMember]
            public DateTime? BGCheck_Result_Date { get; set; }

            [DataMember]
            public Guid? BGCheck_Result_DocGuid { get; set; }

            [DataMember]
            public string BGCheck_Result_Value { get; set; }

            // These are legacy values used for scanning in old applications
            [DataMember]
            public int? Legacy_Application_DocFileId { get; set; }
        
            [DataMember]
            public int? Legacy_CharacterReference1_DocFileId { get; set; }

            [DataMember]
            public int? Legacy_CharacterReference2_DocFileId { get; set; }

            [DataMember]
            public int? Legacy_CharacterReference3_DocFileId { get; set; }
            //
            
            public const string sState_InReviewWithCampus = "In Review with Campus";
            public const string sState_InReviewWithSecurity = "In Review with Security";
            public const string sState_Waiting = "Waiting for Applicant to Complete";
            public const string sState_Accepted = "Accepted by Security";

            public static string GetState( DateTime sentDate, DateTime completedDate, string workflowStatus )
            {
                // JHM 7-10-17
                // HORRIBLE HACK - If the application was sent before we ended testing, we need to support old states and attributes.
                // We need to do this because we have 100+ applications that were sent out (and not yet completed) during our testing phase. I was hoping for like, 10.
                // We can get rid of this when all workflows of type 202 are marked as 'completed'
                if ( sentDate < new DateTime( 2017, 7, 11 ) )
                {
                    return GetState_TestVersion( sentDate, completedDate, workflowStatus );
                }
                else
                {
                    // there are 4 overall states for the screening process:
                    // default - It's out there waiting for the applicant to complete
                    // Campus - it's been submitted and is under review by the campus / STARS
                    // Security - It's been approved by the campus and sent to security for review
                    // Completed - It's been accepted by security.
                    switch ( workflowStatus )
                    {
                        default: return sState_Waiting;
                        case "Campus": return sState_InReviewWithCampus;
                        case "Security": return sState_InReviewWithSecurity;
                        case "Completed": return sState_Accepted;
                    }
                }
            }

            public const string sState_HandedOff_TestVersion = "Handed off to Security";
            public const string sState_InReview_TestVersion = "Application in Review";
            public static string GetState_TestVersion( DateTime sentDate, DateTime completedDate, string workflowStatus )
            {
                // there are 3 overall states for the screening process:
                // Application Sent (modifiedDateTime == createdDateTime)
                // Application Completed and in Review (modifiedDateTime > createdDateTime)
                // Application Approved and now being reviewed by security (workflow == complete)
                if( workflowStatus == "Completed" )
                {
                    return sState_HandedOff_TestVersion;
                }
                else if ( completedDate > sentDate )
                {
                    return sState_InReview_TestVersion;
                }
                else
                {
                    return sState_Waiting;
                }
            }
        }
}
