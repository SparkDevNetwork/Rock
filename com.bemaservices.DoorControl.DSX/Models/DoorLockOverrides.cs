using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.bemaservices.DoorControl.DSX.Models
{
    public class DoorLockOverride
    {
        public DateTime StartTime { get; set; }
        public DoorLockActions StartTimeAction { get; set; }
        public DateTime EndTime { get; set; }
        public DoorLockActions EndTimeAction { get; set; }
        public int LocationId { get; set; }
        public int? ReservationId { get; set; }
        public int OverrideGroup { get; set; }
        public string RoomName { get; set; }
    }
}
