namespace org.newpointe.ServiceUCalendar.Model
{
    public class Calendar : Rock.Data.Model<Calendar>
    {
            public string CategoryList { get; set; }
            public string ContactEmail { get; set; }
            public string ContactName { get; set; }
            public string ContactPhone { get; set; }
            public string DateModified { get; set; }
            public string DepartmentList { get; set; }
            public string DepartmentName { get; set; }
            public string Description { get; set; }
            public bool DisplayTimes { get; set; }
            public int EventId { get; set; }
            public string ExternalEventUrl { get; set; }
            public string ExternalImageUrl { get; set; }
            public string LocationAddress { get; set; }
            public string LocationAddress2 { get; set; }
            public string LocationCity { get; set; }
            public string LocationName { get; set; }
            public string LocationState { get; set; }
            public string LocationZip { get; set; }
            public string MaxDate { get; set; }
            public string MinDate { get; set; }
            public string Name { get; set; }
            public string OccurrenceEndTime { get; set; }
            public int OccurrenceId { get; set; }
            public string OccurrenceStartTime { get; set; }
            public string PublicEventUrl { get; set; }
            public int RegistrationEnabled { get; set; }
            public string RegistrationUrl { get; set; }
            public string ResourceEndTime { get; set; }
            public string ResourceList { get; set; }
            public string ResourceStartTime { get; set; }
            public string StatusDescription { get; set; }
            public string SubmittedBy { get; set; }
    }
    public class CalendarLite
    {
        public int? id { get; set; }
        public string title { get; set; }
        public int? url { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string @class { get; set; }
        public string departmentname { get; set; }
        public string description { get; set; }
        public string locationaddress { get; set; }
        public string locationaddress2 { get; set; }
        public string locationcity { get; set; }
        public string locationname { get; set; }
        public string locationstate { get; set; }
        public string locationzip { get; set; }
    }
}
