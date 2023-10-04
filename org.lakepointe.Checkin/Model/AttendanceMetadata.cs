using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.lakepointe.Checkin.Model
{
    [DataContract]
    [Table( "_org_lakepointe_Checkin_AttendanceMetadata" )]
    public partial class AttendanceMetadata : Model<AttendanceMetadata>, IRockEntity
    {
        [DataMember]
        public int AttendanceId { get; set; }

        [DataMember]
        public int? CheckedOutByPersonAliasId { get; set; }

        [DataMember]
        public virtual Attendance Attendance { get; set; }

        [DataMember]
        public virtual PersonAlias CheckedOutByPersonAlias { get; set; }

        public static bool CheckedInDuringActiveSchedule( Attendance a )
        {
            return CheckedInDuringActiveSchedule( a, true );
        }

        public static  bool CheckedInDuringActiveSchedule( Attendance a, bool includeCurrentlyCheckedIn )
        {
            if ( a == null )
            {
                return false;
            }


            if ( a.Occurrence == null || a.Occurrence.Schedule == null )
            {
                return false;
            }

            // Get the current time (and adjust for a campus timezone)
            var currentDateTime = RockDateTime.Now;
            if ( a.Campus != null )
            {
                currentDateTime = a.Campus.CurrentDateTime;
            }
            else if ( a.CampusId.HasValue )
            {
                var campus = CampusCache.Get( a.CampusId.Value );
                if ( campus != null )
                {
                    currentDateTime = campus.CurrentDateTime;
                }
            }

            // if start date was not today return false
            if ( a.StartDateTime < currentDateTime.Date )
            {
                return false;
            }

            // check to see if they are currently checked in
            if ( a.IsCurrentlyCheckedIn )
            {
                if ( includeCurrentlyCheckedIn )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Schedule has ended because they are not currently checked in
            // and were not manually checked out
            if ( !a.EndDateTime.HasValue )
            {
                return false;
            }

            if ( a.EndDateTime < RockDateTime.Now.AddDays( -2 ) )
            {
                return false;
            }


            if ( a.EndDateTime > currentDateTime || a.EndDateTime < a.Occurrence.Schedule.GetNextCheckInStartTime( currentDateTime.Date ) )
            {
                return false;
            }

            return a.Occurrence.Schedule.WasScheduleOrCheckInActive( a.EndDateTime.Value );
        }
    }

    public partial class AttendanceMetadataConfiguration : EntityTypeConfiguration<AttendanceMetadata>
    {
        public AttendanceMetadataConfiguration()
        {
            this.HasRequired( a => a.Attendance ).WithMany().HasForeignKey( a => a.AttendanceId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.CheckedOutByPersonAlias ).WithMany().HasForeignKey( a => a.CheckedOutByPersonAliasId ).WillCascadeOnDelete( false );

            this.HasEntitySetName( "AttendanceMetadata" );
        }
    }




}
