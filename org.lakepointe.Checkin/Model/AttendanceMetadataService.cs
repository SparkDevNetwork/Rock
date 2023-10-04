using System.Linq;

using Rock.Data;

namespace org.lakepointe.Checkin.Model
{
    public partial class AttendanceMetadataService : Service<AttendanceMetadata>
    {
        public AttendanceMetadataService( RockContext context ) : base( context ) { }

        public bool CanDelete( AttendanceMetadata item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        public AttendanceMetadata GetByAttendanceId( int attendanceId )
        {
            return Queryable()
                .Where( a => a.AttendanceId == attendanceId )
                .FirstOrDefault();
        }
    }
}
