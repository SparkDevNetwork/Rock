using System;
using System.Data.SqlClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.BulkImport;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Crm.Attendance
{
    [TestClass]
    public class AttendanceBulkImportTest : DatabaseTestsBase
    {
        private AttendancesImport GetAttendancesImport()
        {
            var attendancesImportJSON = @"
{
  ""Attendances"": 
    [
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-01"",""StartDateTime"" : ""2014-01-01T10:17:23"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-01"",""StartDateTime"" : ""2014-01-01T10:17:23"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-08"",""StartDateTime"" : ""2014-01-08T10:29:21"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-08"",""StartDateTime"" : ""2014-01-08T10:29:21"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-15"",""StartDateTime"" : ""2014-01-15T10:22:29"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-15"",""StartDateTime"" : ""2014-01-15T10:22:29"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-22"",""StartDateTime"" : ""2014-01-22T10:34:29"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-22"",""StartDateTime"" : ""2014-01-22T10:34:29"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-29"",""StartDateTime"" : ""2014-01-29T10:19:32"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-01-29"",""StartDateTime"" : ""2014-01-29T10:19:32"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-02-05"",""StartDateTime"" : ""2014-02-05T10:18:30"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-02-05"",""StartDateTime"" : ""2014-02-05T10:18:30"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-02-12"",""StartDateTime"" : ""2014-02-12T10:30:27"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-02-12"",""StartDateTime"" : ""2014-02-12T10:30:27"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-02-26"",""StartDateTime"" : ""2014-02-26T10:21:10"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-02-26"",""StartDateTime"" : ""2014-02-26T10:21:10"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-03-19"",""StartDateTime"" : ""2014-03-19T10:19:39"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-03-19"",""StartDateTime"" : ""2014-03-19T10:19:39"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-03-26"",""StartDateTime"" : ""2014-03-26T10:29:22"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-03-26"",""StartDateTime"" : ""2014-03-26T10:29:22"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-02"",""StartDateTime"" : ""2014-04-02T10:19:21"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-02"",""StartDateTime"" : ""2014-04-02T10:19:21"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-09"",""StartDateTime"" : ""2014-04-09T10:25:20"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-09"",""StartDateTime"" : ""2014-04-09T10:25:20"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-16"",""StartDateTime"" : ""2014-04-16T10:20:08"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-16"",""StartDateTime"" : ""2014-04-16T10:20:08"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-23"",""StartDateTime"" : ""2014-04-23T10:21:58"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-23"",""StartDateTime"" : ""2014-04-23T10:21:58"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-30"",""StartDateTime"" : ""2014-04-30T10:21:30"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-04-30"",""StartDateTime"" : ""2014-04-30T10:21:30"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-07"",""StartDateTime"" : ""2014-05-07T10:19:34"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-07"",""StartDateTime"" : ""2014-05-07T10:19:34"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-14"",""StartDateTime"" : ""2014-05-14T10:22:00"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-14"",""StartDateTime"" : ""2014-05-14T10:22:00"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-21"",""StartDateTime"" : ""2014-05-21T10:22:12"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-21"",""StartDateTime"" : ""2014-05-21T10:22:12"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-28"",""StartDateTime"" : ""2014-05-28T10:30:29"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-05-28"",""StartDateTime"" : ""2014-05-28T10:30:29"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-06-04"",""StartDateTime"" : ""2014-06-04T10:16:21"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-06-04"",""StartDateTime"" : ""2014-06-04T10:16:21"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-06-18"",""StartDateTime"" : ""2014-06-18T08:57:12"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-06-18"",""StartDateTime"" : ""2014-06-18T08:57:12"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-06-25"",""StartDateTime"" : ""2014-06-25T10:22:56"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-06-25"",""StartDateTime"" : ""2014-06-25T10:22:56"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-02"",""StartDateTime"" : ""2014-07-02T10:26:22"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-02"",""StartDateTime"" : ""2014-07-02T10:26:22"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-09"",""StartDateTime"" : ""2014-07-09T10:38:21"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-09"",""StartDateTime"" : ""2014-07-09T10:38:21"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-16"",""StartDateTime"" : ""2014-07-16T10:23:21"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-16"",""StartDateTime"" : ""2014-07-16T10:23:21"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-23"",""StartDateTime"" : ""2014-07-23T10:25:33"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-23"",""StartDateTime"" : ""2014-07-23T10:25:33"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-30"",""StartDateTime"" : ""2014-07-30T10:40:33"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-07-30"",""StartDateTime"" : ""2014-07-30T10:40:33"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-08-06"",""StartDateTime"" : ""2014-08-06T10:26:45"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-08-06"",""StartDateTime"" : ""2014-08-06T10:26:45"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-09-10"",""StartDateTime"" : ""2014-09-10T10:22:17"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-09-10"",""StartDateTime"" : ""2014-09-10T10:22:17"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-09-24"",""StartDateTime"" : ""2014-09-24T10:17:12"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-09-24"",""StartDateTime"" : ""2014-09-24T10:17:12"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-01"",""StartDateTime"" : ""2014-10-01T10:24:12"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-01"",""StartDateTime"" : ""2014-10-01T10:24:12"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-08"",""StartDateTime"" : ""2014-10-08T10:23:59"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-08"",""StartDateTime"" : ""2014-10-08T10:23:59"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-15"",""StartDateTime"" : ""2014-10-15T10:18:37"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-15"",""StartDateTime"" : ""2014-10-15T10:18:37"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-22"",""StartDateTime"" : ""2014-10-22T08:54:08"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-22"",""StartDateTime"" : ""2014-10-22T08:54:08"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-29"",""StartDateTime"" : ""2014-10-29T10:30:42"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-10-29"",""StartDateTime"" : ""2014-10-29T10:30:42"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-11-05"",""StartDateTime"" : ""2014-11-05T10:29:41"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-11-05"",""StartDateTime"" : ""2014-11-05T10:29:41"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-11-12"",""StartDateTime"" : ""2014-11-12T10:17:08"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-11-12"",""StartDateTime"" : ""2014-11-12T10:17:08"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-03"",""StartDateTime"" : ""2014-12-03T10:30:47"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-03"",""StartDateTime"" : ""2014-12-03T10:30:47"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-10"",""StartDateTime"" : ""2014-12-10T10:29:07"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-10"",""StartDateTime"" : ""2014-12-10T10:29:07"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-24"",""StartDateTime"" : ""2014-12-24T10:29:38"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-24"",""StartDateTime"" : ""2014-12-24T10:29:38"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-31"",""StartDateTime"" : ""2014-12-31T10:17:30"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2014-12-31"",""StartDateTime"" : ""2014-12-31T10:17:30"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-01-21"",""StartDateTime"" : ""2015-01-21T10:16:40"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-01-21"",""StartDateTime"" : ""2015-01-21T10:16:40"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-01-28"",""StartDateTime"" : ""2015-01-28T10:30:57"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-01-28"",""StartDateTime"" : ""2015-01-28T10:30:57"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-02-04"",""StartDateTime"" : ""2015-02-04T10:25:14"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-02-04"",""StartDateTime"" : ""2015-02-04T10:25:14"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-02-11"",""StartDateTime"" : ""2015-02-11T10:27:15"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-02-11"",""StartDateTime"" : ""2015-02-11T10:27:15"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-02-18"",""StartDateTime"" : ""2015-02-18T10:22:33"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-02-18"",""StartDateTime"" : ""2015-02-18T10:22:33"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-03-04"",""StartDateTime"" : ""2015-03-04T10:41:49"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-03-04"",""StartDateTime"" : ""2015-03-04T10:41:49"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-03-25"",""StartDateTime"" : ""2015-03-25T10:22:49"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-03-25"",""StartDateTime"" : ""2015-03-25T10:22:49"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-04-01"",""StartDateTime"" : ""2015-04-01T10:27:44"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-04-01"",""StartDateTime"" : ""2015-04-01T10:27:44"",""PersonId"" : 7,""PersonAliasId"" : 13},
      {""GroupId"" : 29,""LocationId"" : 8,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-04-08"",""StartDateTime"" : ""2015-04-08T10:24:17"",""PersonId"" : 6,""PersonAliasId"" : 14},
      {""GroupId"" : 28,""LocationId"" : 7,""ScheduleId"" : 3,""OccurrenceDate"" : ""2015-04-08"",""StartDateTime"" : ""2015-04-08T10:24:17"",""PersonId"" : 7,""PersonAliasId"" : 13},
    ]
}";

            var attendancesImport = attendancesImportJSON.FromJsonOrNull<AttendancesImport>();
            if ( attendancesImport == null )
            {
                return attendancesImport;
            }

            var personIdPersonAliasIdList = TestDataHelper.GetPersonIdWithAliasIdList();

            // make sure the example data has real personIds and personAliasIds
            attendancesImport.Attendances.ForEach( a =>
            {
                var person = personIdPersonAliasIdList.GetRandomElement();
                a.PersonId = person.PersonId;
                a.PersonAliasId = person.PersonAliasId;
            } );

            return attendancesImport;
        }


        [TestMethod]
        public void Import_WithBothNullPersonIdAndNullPersonAliasId()
        {
            var attendancesImport = GetAttendancesImport();
            attendancesImport.Attendances.ForEach( a =>
            {
                a.PersonId = null;
                a.PersonAliasId = null;
            } );

            Assert.That.IsNotNull( attendancesImport );
            Exception exception = null;

            try
            {
                AttendanceService.BulkAttendanceImport( attendancesImport );

                // if this doesn't fail, the test fails
                Assert.That.IsTrue( false );
            }
            catch ( Exception ex )
            {
                exception = ex;
            }

            // Test passes if we get a SqlException
            Assert.That.IsTrue( exception.Message == "All Attendance records must have either a PersonId or PersonAliasId assigned.", exception.Message );
        }

        [TestMethod]
        public void Import_WithBadPersonIds()
        {
            var attendancesImport = GetAttendancesImport();
            Assert.That.IsNotNull( attendancesImport );

            Exception exception = null;

            try
            {
                var random = new Random();
                attendancesImport.Attendances.ForEach( a =>
                {
                    var randomId = random.Next();
                    a.PersonId = randomId;
                } );
                AttendanceService.BulkAttendanceImport( attendancesImport );

                // if this doesn't fail, the test fails
                Assert.That.IsTrue( false );
            }
            catch ( Exception ex )
            {
                exception = ex;
            }

            // Test passes if we get a SqlException
            Assert.That.IsTrue( exception is SqlException, exception.Message );
        }

        [TestMethod]
        public void Import_WithPersonId()
        {
            var attendancesImport = GetAttendancesImport();
            Assert.That.IsNotNull( attendancesImport );

            // don't include PersonAliasId value
            attendancesImport.Attendances.ForEach( a => a.PersonAliasId = null );

            try
            {
                AttendanceService.BulkAttendanceImport( attendancesImport );
                Assert.That.IsTrue( true );
            }
            catch ( Exception ex )
            {
                Assert.That.Fail( ex.Message );
            }
        }

        [TestMethod]
        public void Import_WithPersonAliasId()
        {
            var attendancesImport = GetAttendancesImport();
            Assert.That.IsNotNull( attendancesImport );

            // don't include PersonId value
            attendancesImport.Attendances.ForEach( a => a.PersonId = null );

            try
            {
                AttendanceService.BulkAttendanceImport( attendancesImport );
                Assert.That.IsTrue( true );
            }
            catch ( Exception ex )
            {
                Assert.That.Fail( ex.Message );
            }
        }
    }
}
