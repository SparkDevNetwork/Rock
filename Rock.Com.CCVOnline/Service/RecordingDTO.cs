using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Com.CCVOnline.Service
{
    public class RecordingDTO
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public DateTime? Date { get; set; }
        public int? CampusId { get; set; }
        public string Label { get; set; }
        public string App { get; set; }
        public string StreamName { get; set; }
        public string RecordingName { get; set; }
        public DateTime? StartTime { get; set; }
        public string StartResponse { get; set; }
        public DateTime? StopTime { get; set; }
        public string StopResponse { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public int? CreatedByPersonId { get; set; }
        public int? ModifiedByPersonId { get; set; }

        public RecordingDTO()
        {
        }
    }
}
