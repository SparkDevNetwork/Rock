// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace rocks.pillars.Jobs.Jobs
{
    /// <summary>
    /// Custom Rock Job
    /// </summary>
    [RegistrationTemplateField("Baptism Registration Template",
                    Description = "This is the registration template to use to create registraion instances for",
                    IsRequired = true,
                    Order = 0)]
    [LavaField("Registration Instance Name",
                Description = "(Lava) The name of the registration instance to be created",
                IsRequired = false,
                Order = 1)]
    [EventItemField("Event Item",
                    Description = "The event item used to create occurrences",
                    IsRequired = true,
                    Order = 2)]
    [IntegerField("Days After Registration Instance End Date to Inactivate",
                 Description = "How many days after the registration end date to inactivate the registration instance. Leaving this option blank will disable this setting",
                 IsRequired = false,
                 Order = 3)]
    [IntegerField("Weeks Ahead",
                Description = "How many weeks ahead should the job generate instances",
                IsRequired = true,
                Order = 4)]
    [IntegerField("Registration Start Date",
                Description = "How many days before the scheduled time will the registration open",
                IsRequired = true,
                Order = 5)]
    [DefinedTypeField("Baptism Configuration Defined Type",
                  Description = "This is the Defined Type setup to pull in the Baptism Configuration.",
                  IsRequired = true,
                  Order = 6)]
    [DefinedTypeField("Baptism Blackout Dates",
                Description = "This is the defined type that has the Baptism Blackout dates.",
                IsRequired = true,
                Order = 7)]
    [DayOfWeekField("Cutoff Day of Week",
        Description = "Day of the week prior to the event should registration stop",
        IsRequired = true,
        Order = 8)]
    [TimeField("Cutoff Time of Day",
        Description = "Time of day when registration should stop",
        IsRequired = true,
        Order = 9)]

    [DisallowConcurrentExecution]
    public class BaptismRegistrationGeneration : IJob
    {
        private Guid? _regTemplateGuid;
        private string _regInsName;
        private Guid? _eventItemGuid;
        private int? _inactiveDays;
        private int _weeksAhead;
        private int _regStartDate;
        private Guid? _bapConfDTGuid;
        private Guid? _blackoutDTGuid;
        private DayOfWeek? _dayOfWeek;
        private TimeSpan? _timeOfDay;

        private int _instancesAdded = 0;
        private int _instancesInactivated = 0;

        private RegistrationTemplate _regTemplate;

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _regTemplateGuid = dataMap.GetString("BaptismRegistrationTemplate").AsGuidOrNull();
            _regInsName = dataMap.GetString("RegistrationInstanceName");
            _eventItemGuid = dataMap.GetString("EventItem").AsGuidOrNull();
            _inactiveDays = dataMap.GetString("DaysAfterRegistrationInstanceEndDatetoInactivate").AsIntegerOrNull();
            _weeksAhead = dataMap.GetString("WeeksAhead").AsInteger();
            _regStartDate = dataMap.GetString("RegistrationStartDate").AsInteger();
            _bapConfDTGuid = dataMap.GetString("BaptismConfigurationDefinedType").AsGuidOrNull();
            _blackoutDTGuid = dataMap.GetString("BaptismBlackoutDates").AsGuidOrNull();
            _dayOfWeek = dataMap.GetString("CutoffDayofWeek").ConvertToEnum<DayOfWeek>();
            _timeOfDay = dataMap.GetString("CutoffTimeofDay").AsTimeSpan();

            _regTemplate = new RegistrationTemplateService(new RockContext()).Get(_regTemplateGuid.Value);

            var today = RockDateTime.Today;
            var weeksAheadDate = today.AddDays(_weeksAhead * 7);

            if (_inactiveDays.HasValue)
            {
                InactivateRegistrationInstancesAndGroups();
            }

            // Get the defined Values under the defined type baptism configuration
            if(_bapConfDTGuid.HasValue)
            { 
                var definedTypeBC = DefinedTypeCache.Get(_bapConfDTGuid.Value);
                var bapConfigs = definedTypeBC.DefinedValues;
                bapConfigs.LoadAttributes();

                var scheduledDates = GetScheduledDates(bapConfigs, today, weeksAheadDate);
                Dictionary<Guid, List<DateRange>> blackedoutDates = new Dictionary<Guid, List<DateRange>>();

                // Get the black out defined values under the defined type baptism blackouts

                var definedTypeBD = DefinedTypeCache.Get(_blackoutDTGuid.Value);

                if (definedTypeBD != null && definedTypeBD.DefinedValues.Count >= 0)
                {
                    blackedoutDates = RemoveBlackedOutDates(scheduledDates, definedTypeBD.DefinedValues);
                }

                // Determine type, campus and dates that we need registrations for
                var typeCampusScheduleDict = GetTypeCampusScheduleValues(bapConfigs);

                CreateRegistrationInstances(scheduledDates, typeCampusScheduleDict, blackedoutDates);

                context.UpdateLastStatusMessage(string.Format("Instances Inactivated: {0}<br/>Instances Add: {1}",
                    _instancesInactivated, _instancesAdded));
            }
            else
            {
                context.UpdateLastStatusMessage("The Baptism Configuration Defined Type doesn't exist or didn't have values ");
            }
        }

        private void InactivateRegistrationInstancesAndGroups()
        {
            var rockContext = new RockContext();
            var instanceService = new RegistrationInstanceService(rockContext);
            var groupService = new GroupService(rockContext);
            var today = RockDateTime.Today;

            var instancesToInactivate = instanceService.Queryable()
                    .Where(i => i.RegistrationTemplate.Guid == _regTemplateGuid && i.EndDateTime <= today)
                    .ToList();

            instancesToInactivate = instancesToInactivate.Where(i => (i.EndDateTime.Value - today).TotalDays >= _inactiveDays).ToList();

            foreach(var i in instancesToInactivate)
            {
                i.IsActive = false;

                _instancesInactivated++;
                
                foreach(var l in i.Linkages)
                {
                    if(l.Group != null)
                    {
                        groupService.Archive(l.Group, null, true);
                    }
                }
            }
        }

        private Dictionary<Guid, List<DateTime>> GetScheduledDates(List<DefinedValueCache> bapConfigs, DateTime today, DateTime weeksAhead)
        {
            var rockContext = new RockContext();
            var scheduleService = new ScheduleService(rockContext);

            var scheduleGuids = bapConfigs.SelectMany(c => c.GetAttributeValues("_rocks_pillars_ServiceTimes").AsGuidList()).Distinct().ToList();

            var schedules = scheduleService.GetByGuids(scheduleGuids).ToList();

            var scheduleDates = new Dictionary<Guid, List<DateTime>>();

            foreach (var sch in schedules)
            {
                var dates = sch.GetICalOccurrences(today, weeksAhead)
                    .Select(o => DateTime.SpecifyKind(o.Period.StartTime.Value, DateTimeKind.Local)).ToList();

                scheduleDates.Add(sch.Guid, dates);
            }

            return scheduleDates;
        }

        private Dictionary<Guid, List<DateRange>> RemoveBlackedOutDates(Dictionary<Guid, List<DateTime>> scheduledDates, List<DefinedValueCache> definedValues)
        {
            var blackedOutDates = new Dictionary<Guid, List<DateRange>>();
            definedValues.LoadAttributes();

            foreach(var val in definedValues)
            {
                var cmpGuids = val.GetAttributeValues("_rocks_pillars_Campus").AsGuidList();

                var dateRange = DateRange.FromDelimitedValues(val.GetAttributeValue("_rocks_pillars_DateRange"));

                //Only defined values that are current
                if(dateRange.End >= RockDateTime.Today)
                {
                    foreach (var cmpGuid in cmpGuids)
                    {
                        if (blackedOutDates.TryGetValue(cmpGuid, out var dateRanges))
                        {
                            dateRanges.Add(dateRange);
                        }
                        else
                        {
                            blackedOutDates.Add(cmpGuid, new List<DateRange> { dateRange });
                        }
                    }
                }
            }

            return blackedOutDates;
        }

        // This will go through the baptism config and return a dictionary of values
        // grouped by 'GroupId,CampusGuid,ScheduleGuid' with the value being the 
        // maximum registrants. Where GroupId is the Type of baptism
        private Dictionary<string,BaptismConfigValues> GetTypeCampusScheduleValues(List<DefinedValueCache> bapConfigs)
        {
            var typeCampusSch = new Dictionary<string, BaptismConfigValues>();
            var rockContext = new RockContext();
            var personService = new PersonAliasService(rockContext);

            foreach(var config in bapConfigs)
            {
                var type = config.GetAttributeValue("_rocks_pillars_Type");
                var campus = config.GetAttributeValue("_rocks_pillars_Campus");
                var schedules = config.GetAttributeValues("_rocks_pillars_ServiceTimes");
                var contactGuid = config.GetAttributeValue("_rocks_pillars_ContactPerson").AsGuid();
                var contactPhone = config.GetAttributeValue("_rocks_pillars_ContactPhone");

                var contact = personService.Get(contactGuid);

                var maxRegistrants = config.Value.AsInteger();

                var typeCampusKey = string.Format("{0},{1}", type, campus);

                foreach(var sch in schedules)
                {
                    var key = string.Format("{0},{1}", typeCampusKey, sch);

                    if(typeCampusSch.TryGetValue(key, out BaptismConfigValues val))
                    {
                        typeCampusSch[key].MaxRegistrants = val.MaxRegistrants > maxRegistrants ? 
                            val.MaxRegistrants : maxRegistrants;
                    }
                    else
                    {
                        typeCampusSch.Add(key, new BaptismConfigValues 
                        {
                            MaxRegistrants = maxRegistrants,
                            ContactPhone = contactPhone,
                            ContactPersonAliasId = contact?.Id,
                            ContactEmail = contact?.Person?.Email
                        } );
                    }
                }
            }

            return typeCampusSch;
        }

        private void CreateRegistrationInstances(Dictionary<Guid, List<DateTime>> scheduledDates, Dictionary<string, BaptismConfigValues> typeCampusScheduleDict, Dictionary<Guid, List<DateRange>> blackedoutDates)
        {
            var rockContext = new RockContext();
            var regInsService = new RegistrationInstanceService(rockContext);
            var eventItemService = new EventItemService(rockContext);

            var eventItem = eventItemService.Get(_eventItemGuid.Value);

            var campusGuids = typeCampusScheduleDict.Select(kvp => kvp.Key.Split(',')[1]).AsGuidList();
            var campuses = CampusCache.All().Where(c => campusGuids.Contains(c.Guid));

            foreach (var kvp in typeCampusScheduleDict)
            {
                var keySplit = kvp.Key.Split(',');

                var typeId = keySplit[0].AsInteger();
                var cmpGuid = keySplit[1].AsGuid();
                var schGuid = keySplit[2].AsGuid();

                var campus = campuses.First(c => c.Guid == cmpGuid);
                var dates = scheduledDates[schGuid];

                if(blackedoutDates.TryGetValue(cmpGuid, out var dateRanges))
                {
                    dates = dates.Where(d => dateRanges.Any(dr => d >= dr.Start && d < dr.End.Value.AddDays(1)) == false).ToList();
                }

                //Create or Find Mid Level Group (Campus)
                var cmpGroup = CreateCampusGroup(campus, typeId);

                foreach (var date in dates)
                {
                    var instanceName = GenerateInstanceName(cmpGroup, date);

                    //Create Registration Instance Date Group
                    var regInstGroup = CreateBaptismGroup(cmpGroup, date);

                    //Create Event Occurrence Item
                    var eventOcc = CreateEventItemOcc(regInstGroup, date, eventItem);

                    //Create the Registration Instance with start date and end date and name and max registrations number
                    var regInstance = CreateRegistrationInstance(instanceName, date, kvp.Value);

                    //Create linkage with the group and event item occurrence
                    LinkGroupWithEventAndReg(regInstance, eventOcc, regInstGroup);
                }
            }
        }

        private void LinkGroupWithEventAndReg(RegistrationInstance regInstance, EventItemOccurrence eventOcc, Group regInstGroup)
        {
            var rockContext = new RockContext();
            var linkageService = new EventItemOccurrenceGroupMapService(rockContext);

            var linkage = linkageService.Queryable()
                .AsNoTracking()
                .Where(l => l.EventItemOccurrenceId == eventOcc.Id && l.GroupId == regInstGroup.Id && l.RegistrationInstanceId == regInstance.Id)
                .FirstOrDefault();

            if(linkage == null)
            {
                linkage = new EventItemOccurrenceGroupMap
                {
                    RegistrationInstanceId = regInstance.Id,
                    EventItemOccurrenceId = eventOcc.Id,
                    GroupId = regInstGroup.Id,
                    PublicName = regInstance.Name,
                    UrlSlug = $"baptism-{ regInstance.Id }-{ eventOcc.Id }-{ regInstGroup.Id }" // Obsidian registations won't connect to the group without this
                };

                linkageService.Add(linkage);

                rockContext.SaveChanges();
            }
        }

        private RegistrationInstance CreateRegistrationInstance(string instanceName, DateTime date, BaptismConfigValues configValues)
        {
            var rockContext = new RockContext();
            var regInsService = new RegistrationInstanceService(rockContext);

            var instance = regInsService.Queryable()
                    .AsNoTracking()
                    .Where(i => i.Name.Equals(instanceName, StringComparison.OrdinalIgnoreCase) && i.RegistrationTemplate.Guid == _regTemplateGuid)
                    .FirstOrDefault();

            if(instance == null)
            {
                _instancesAdded++;

                instance = new RegistrationInstance
                {
                    Name = instanceName,
                    StartDateTime = date.AddDays(_regStartDate * -1),
                    //EndDateTime = date.Date.AddMinutes(-1),
                    EndDateTime = date.Date.StartOfDay().AddDays(-7).GetNextWeekday(_dayOfWeek.Value).Add(_timeOfDay.Value),
                    MaxAttendees = configValues.MaxRegistrants,
                    ContactEmail = configValues.ContactEmail,
                    ContactPhone = configValues.ContactPhone,
                    ContactPersonAliasId = configValues.ContactPersonAliasId,
                    ReminderSent = false,
                    RegistrationInstructions = string.Empty,
                    RegistrationTemplateId = _regTemplate.Id
                };

                regInsService.Add(instance);

                rockContext.SaveChanges();
            }

            return instance;
        }

        private string GenerateInstanceName(Group cmpGroup, DateTime date)
        {
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CampusGroup", cmpGroup );
            mergeFields.Add( "Date", date );

            return _regInsName.ResolveMergeFields(mergeFields);
        }

        private EventItemOccurrence CreateEventItemOcc(Group regInstGroup, DateTime date, EventItem eventItem)
        {
            var rockContext = new RockContext();
            var eventOccService = new EventItemOccurrenceService(rockContext);

            var eventOcc = regInstGroup.Linkages.FirstOrDefault()?.EventItemOccurrence;

            if(eventOcc == null)
            {
                var schedule = new Schedule
                {
                    EffectiveStartDate = date,
                    IsActive = true,
                    Order = 0,
                    iCalendarContent = GenerateIcalContent(date)
                };

                
                eventOcc = new EventItemOccurrence
                {
                    EventItemId = eventItem.Id,
                    CampusId = regInstGroup.CampusId,
                    Schedule = schedule
                };

                eventOccService.Add(eventOcc);

                rockContext.SaveChanges();
            }

            return eventOcc;
        }

        private string GenerateIcalContent(DateTime date)
        {
            var calEvent = new CalendarEvent();
            calEvent.DtStart = new CalDateTime(date);
            calEvent.DtStamp = new CalDateTime(RockDateTime.Now);
            calEvent.Uid = Guid.NewGuid().ToString();
            calEvent.DtStart.HasTime = true;
            calEvent.Duration = new TimeSpan(0, 0, 1);

            var calendar = new Calendar();
            calendar.Events.Add(calEvent);

            var iCalendarSerializer = new CalendarSerializer(calendar);
            return iCalendarSerializer.SerializeToString(calendar);
        }

        private Group CreateBaptismGroup(Group cmpGroup, DateTime date)
        {
            var rockContext = new RockContext();
            var groupService = new GroupService(rockContext);

            var groupName = date.ToString("M/d/yyyy - h:mmtt");

            // Try to find the group if it exists
            var group = groupService.Queryable()
                    .AsNoTracking()
                    .Where(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase) && g.ParentGroupId == cmpGroup.Id)
                    .FirstOrDefault();

            if(group == null)
            {
                group = new Group
                {
                    ParentGroupId = cmpGroup.Id,
                    GroupTypeId = cmpGroup.GroupTypeId,
                    Name = groupName,
                    CampusId = cmpGroup.CampusId,
                    IsActive = true,
                    IsArchived = false,
                    IsPublic = true,
                    IsSystem = false,
                    IsSecurityRole = false
                };

                groupService.Add(group);

                rockContext.SaveChanges();
            }

            return group;
        }

        private Group CreateCampusGroup(CampusCache campus, int typeId)
        {
            var rockContext = new RockContext();
            var groupService = new GroupService(rockContext);

            var parentGroup = groupService.Get(typeId);
            var group = groupService.Queryable().AsNoTracking()
                    .Where(g => g.ParentGroupId == typeId && g.Name == campus.Name)
                    .FirstOrDefault();

            if(group == null)
            {
                group = new Group
                {
                    ParentGroupId = typeId,
                    GroupTypeId = parentGroup.GroupTypeId,
                    Name = campus.Name,
                    CampusId = campus.Id,
                    IsActive = true,
                    IsArchived = false,
                    IsPublic = true,
                    IsSystem = false,
                    IsSecurityRole = false
                };

                groupService.Add(group);

                rockContext.SaveChanges();
            }

            return group;
        }

        public class BaptismConfigValues
        {
            public int MaxRegistrants { get; set; }
            public int? ContactPersonAliasId { get; set; }
            public string ContactPhone { get; set; }
            public string ContactEmail { get; set; }
        }
    }
}
