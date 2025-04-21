using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn.Config
{
    public partial class ImportCheckInConfig : RockBlock
    {
        protected void Import_Click( object sender, EventArgs e )
        {
            if ( !fuSource.BinaryFileId.HasValue )
            {
                return;
            }

            JsonContainer container;

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = new BinaryFileService( rockContext ).Get( fuSource.BinaryFileId.Value );

                try
                {
                    container = binaryFile.ContentsToString().FromJsonOrThrow<JsonContainer>();

                    var importer = new JsonImporter();
                    importer.ImportContainer( container );

                    ltMessages.Text = importer.Messages
                        .Select( m => m.EncodeHtml() )
                        .JoinStrings( "<br>" );
                    pnlResults.Visible = true;
                }
                catch ( Exception ex )
                {
                    ltMessages.Text = "";

                    while ( ex != null )
                    {
                        ltMessages.Text += ex.Message.EncodeHtml()
                            + "<br>"
                            + "<br>"
                            + ex.StackTrace.EncodeHtml().ConvertCrLfToHtmlBr();

                        ex = ex.InnerException;

                        if ( ex != null )
                        {
                            ltMessages.Text += "<br><br>";
                        }
                    }

                    pnlResults.Visible = true;
                }
                finally
                {
                    RockCache.ClearAllCachedItems();

                    fuSource.BinaryFileId = null;
                    binaryFileService.Delete( binaryFile );
                    rockContext.SaveChanges();
                }
            }
        }

        #region Import

        private class JsonImporter
        {
            public List<string> Messages { get; } = new List<string>();

            public void ImportContainer( JsonContainer container )
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransactionIf( () =>
                    {
                        ImportDevices( container.Devices, rockContext );

                        ImportLocations( container.Locations, rockContext );

                        ImportDeviceLocations( container.DeviceLocations, rockContext );

                        ImportCampuses( container.Campuses, rockContext );

                        ImportScheduleCategories( container.ScheduleCategories, rockContext );

                        ImportSchedules( container.Schedules, rockContext );

                        ImportGroupTypes( container.GroupTypes, rockContext );

                        ImportGroupTypeAssociations( container.GroupTypeAssociations, rockContext );

                        ImportGroups( container.Groups, rockContext );

                        ImportGroupLocations( container.GroupLocations, rockContext );

                        ImportGroupLocationSchedules( container.GroupLocationSchedules, rockContext );

                        return true;
                    } );
                }
            }

            private void DetachAllEntities( RockContext rockContext )
            {
                var undetachedEntriesCopy = rockContext.ChangeTracker.Entries()
                    .Where( e => e.State != EntityState.Detached )
                    .ToList();

                foreach ( var entry in undetachedEntriesCopy )
                {
                    entry.State = EntityState.Detached;
                }
            }

            private void ImportDevices( List<JsonDevice> jsonDevices, RockContext rockContext )
            {
                var deviceService = new DeviceService( rockContext );
                var devices = BulkGet( deviceService.Queryable(), jsonDevices.Select( d => d.Guid ) );
                var devicesToAdd = new List<Device>( jsonDevices.Count - devices.Count );

                foreach ( var jsonDevice in jsonDevices )
                {
                    var device = devices.FirstOrDefault( d => d.Guid == jsonDevice.Guid );

                    if ( device == null )
                    {
                        device = new Device
                        {
                            Guid = jsonDevice.Guid
                        };

                        devicesToAdd.Add( device );
                    }

                    device.Name = jsonDevice.Name;
                    device.IsActive = jsonDevice.IsActive;
                    device.DeviceTypeValueId = DefinedValueCache.Get( jsonDevice.DeviceTypeValueGuid ).Id;
                }

                deviceService.AddRange( devicesToAdd );
                devices.AddRange( devicesToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );

                // Do a second pass to update the printer device identfiers now
                // that everything is imported.
                foreach ( var jsonDevice in jsonDevices )
                {
                    var device = devices.First( d => d.Guid == jsonDevice.Guid );

                    device.PrinterDeviceId = devices.FirstOrDefault( d => d.Guid == jsonDevice.PrinterDeviceGuid )?.Id;
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonDevices.Count:N0} devices ({devicesToAdd.Count:N0} new)" );
            }

            private void ImportLocations( List<JsonLocation> jsonLocations, RockContext rockContext )
            {
                var locationService = new LocationService( rockContext );
                var locations = BulkGet( locationService.Queryable(), jsonLocations.Select( d => d.Guid ) );
                var locationsToAdd = new List<Location>( jsonLocations.Count - locations.Count );

                foreach ( var jsonLocation in jsonLocations )
                {
                    var location = locations.FirstOrDefault( l => l.Guid == jsonLocation.Guid );

                    if ( location == null )
                    {
                        location = new Location
                        {
                            Guid = jsonLocation.Guid
                        };

                        locationsToAdd.Add( location );
                    }

                    location.Name = jsonLocation.Name;
                    location.IsActive = jsonLocation.IsActive;
                }

                locationService.AddRange( locationsToAdd );
                locations.AddRange( locationsToAdd );

                rockContext.SaveChanges();

                // Get all the devices that might be referenced by any locations.
                var deviceGuids = jsonLocations
                    .Select( l => l.PrinterDeviceGuid )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .Distinct();
                var devices = BulkGet( new DeviceService( rockContext ).Queryable(), deviceGuids );

                // Do a second pass to update the parent location identfiers now
                // that everything is imported.
                foreach ( var jsonLocation in jsonLocations )
                {
                    var location = locations.First( l => l.Guid == jsonLocation.Guid );

                    location.ParentLocationId = locations.FirstOrDefault( l => l.Guid == jsonLocation.ParentLocationGuid )?.Id;
                    location.PrinterDeviceId = devices.FirstOrDefault( d => d.Guid == jsonLocation.PrinterDeviceGuid )?.Id;
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonLocations.Count:N0} locations ({locationsToAdd.Count:N0} new)" );
            }

            private void ImportDeviceLocations( List<JsonDeviceLocation> jsonDeviceLocations, RockContext rockContext )
            {
                var deviceService = new DeviceService( rockContext );
                var locationService = new LocationService( rockContext );
                var deviceGuids = jsonDeviceLocations.Select( dl => dl.DeviceGuid ).Distinct();
                var locationGuids = jsonDeviceLocations.Select( dl => dl.LocationGuid ).Distinct();
                var devices = BulkGet( deviceService.Queryable(), deviceGuids );
                var locations = BulkGet( locationService.Queryable(), locationGuids );
                var addCount = 0;

                foreach ( var device in devices )
                {
                    var deviceLocationGuids = jsonDeviceLocations
                        .Where( dl => dl.DeviceGuid == device.Guid )
                        .Select( dl => dl.LocationGuid )
                        .Distinct()
                        .ToList();

                    var deviceLocations = locations
                        .Where( l => deviceLocationGuids.Contains( l.Guid ) )
                        .ToList();

                    foreach ( var deviceLocation in deviceLocations )
                    {
                        if ( !device.Locations.Any( l => l.Id == deviceLocation.Id ) )
                        {
                            device.Locations.Add( deviceLocation );
                            addCount++;
                        }
                    }
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonDeviceLocations.Count:N0} device locations ({addCount:N0} new)" );
            }

            private void ImportCampuses( List<JsonCampus> jsonCampuses, RockContext rockContext )
            {
                var campusService = new CampusService( rockContext );
                var locationService = new LocationService( rockContext );
                var campuses = BulkGet( campusService.Queryable(), jsonCampuses.Select( c => c.Guid ) );
                var campusesToAdd = new List<Campus>( jsonCampuses.Count - campuses.Count );

                var locationGuids = jsonCampuses
                    .Select( c => c.LocationGuid )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .Distinct()
                    .ToList();
                var locations = BulkGet( locationService.Queryable(), locationGuids );

                foreach ( var jsonCampus in jsonCampuses )
                {
                    var campus = campuses.FirstOrDefault( c => c.Guid == jsonCampus.Guid );

                    if ( campus == null )
                    {
                        campus = new Campus
                        {
                            Guid = jsonCampus.Guid
                        };

                        campusesToAdd.Add( campus );
                    }

                    campus.Name = jsonCampus.Name;
                    campus.ShortCode = jsonCampus.ShortCode;
                    campus.LocationId = locations.FirstOrDefault( l => l.Guid == jsonCampus.LocationGuid )?.Id;
                    campus.IsActive = jsonCampus.IsActive;
                    campus.Order = jsonCampus.Order;
                    campus.CampusStatusValueId = jsonCampus.CampusStatusValueGuid.HasValue
                        ? DefinedValueCache.Get( jsonCampus.CampusStatusValueGuid.Value )?.Id
                        : null;
                    campus.CampusTypeValueId = jsonCampus.CampusTypeValueGuid.HasValue
                        ? DefinedValueCache.Get( jsonCampus.CampusTypeValueGuid.Value )?.Id
                        : null;
                }

                campusService.AddRange( campusesToAdd );
                campuses.AddRange( campusesToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonCampuses.Count:N0} campuses ({campusesToAdd.Count:N0} new)" );
            }

            private void ImportScheduleCategories( List<JsonCategory> jsonCategories, RockContext rockContext )
            {
                var categoryService = new CategoryService( rockContext );
                var categories = BulkGet( categoryService.Queryable(), jsonCategories.Select( c => c.Guid ) );
                var categoriesToAdd = new List<Category>( jsonCategories.Count - categories.Count );

                foreach ( var jsonCategory in jsonCategories )
                {
                    var category = categories.FirstOrDefault( c => c.Guid == jsonCategory.Guid );

                    if ( category == null )
                    {
                        category = new Category
                        {
                            Guid = jsonCategory.Guid,
                            EntityTypeId = EntityTypeCache.GetId<Rock.Model.Schedule>().Value
                        };

                        categoriesToAdd.Add( category );
                    }

                    category.Name = jsonCategory.Name;
                    category.Order = jsonCategory.Order;
                }

                categoryService.AddRange( categoriesToAdd );
                categories.AddRange( categoriesToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );

                // Do a second pass to update the parent category identfiers now
                // that everything is imported.
                foreach ( var jsonCategory in jsonCategories )
                {
                    var category = categories.First( d => d.Guid == jsonCategory.Guid );

                    category.ParentCategoryId = categories.FirstOrDefault( c => c.Guid == jsonCategory.ParentCategoryGuid )?.Id;
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonCategories.Count:N0} schedule categories ({categoriesToAdd.Count:N0} new)" );
            }

            private void ImportSchedules( List<JsonSchedule> jsonSchedules, RockContext rockContext )
            {
                var scheduleService = new ScheduleService( rockContext );
                var categoryService = new CategoryService( rockContext );
                var schedules = BulkGet( scheduleService.Queryable(), jsonSchedules.Select( s => s.Guid ) );
                var schedulesToAdd = new List<Schedule>( jsonSchedules.Count - schedules.Count );

                var categoryGuids = jsonSchedules
                    .Select( c => c.CategoryGuid )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .Distinct()
                    .ToList();
                var categories = BulkGet( categoryService.Queryable(), categoryGuids );

                foreach ( var jsonSchedule in jsonSchedules )
                {
                    var schedule = schedules.FirstOrDefault( s => s.Guid == jsonSchedule.Guid );

                    if ( schedule == null )
                    {
                        schedule = new Schedule
                        {
                            Guid = jsonSchedule.Guid
                        };

                        schedulesToAdd.Add( schedule );
                    }

                    schedule.Name = jsonSchedule.Name;
                    schedule.iCalendarContent = jsonSchedule.iCalendarContent;
                    schedule.CheckInStartOffsetMinutes = jsonSchedule.CheckInStartOffsetMinutes;
                    schedule.CheckInEndOffsetMinutes = jsonSchedule.CheckInEndOffsetMinutes;
                    schedule.EffectiveStartDate = jsonSchedule.EffectiveStartDate;
                    schedule.EffectiveEndDate = jsonSchedule.EffectiveEndDate;
                    schedule.CategoryId = jsonSchedule.CategoryGuid.HasValue
                        ? categories.FirstOrDefault( c => c.Guid == jsonSchedule.CategoryGuid )?.Id
                        : null;
                    schedule.IsActive = jsonSchedule.IsActive;
                    schedule.Order = jsonSchedule.Order;
                }

                scheduleService.AddRange( schedulesToAdd );
                schedules.AddRange( schedulesToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonSchedules.Count:N0} schedules ({schedulesToAdd.Count:N0} new)" );
            }

            private void ImportGroupTypes( List<JsonGroupType> jsonGroupTypes, RockContext rockContext )
            {
                var groupTypeService = new GroupTypeService( rockContext );
                var groupTypes = BulkGet( groupTypeService.Queryable(), jsonGroupTypes.Select( gt => gt.Guid ) );
                var groupTypesToAdd = new List<GroupType>( jsonGroupTypes.Count - groupTypes.Count );

                foreach ( var jsonGroupType in jsonGroupTypes )
                {
                    var groupType = groupTypes.FirstOrDefault( gt => gt.Guid == jsonGroupType.Guid );

                    if ( groupType == null )
                    {
                        groupType = new GroupType
                        {
                            Guid = jsonGroupType.Guid,
                            TakesAttendance = true
                        };

                        groupTypesToAdd.Add( groupType );
                    }

                    groupType.Name = jsonGroupType.Name;
                    groupType.AttendanceRule = ( AttendanceRule ) jsonGroupType.AttendanceRule;
                    groupType.AttendancePrintTo = ( PrintTo ) jsonGroupType.AttendancePrintTo;
                    groupType.Order = jsonGroupType.Order;
                    groupType.GroupTypePurposeValueId = jsonGroupType.GroupTypePurposeValueGuid.HasValue
                        ? DefinedValueCache.Get( jsonGroupType.GroupTypePurposeValueGuid.Value )?.Id
                        : null;
                    groupType.LocationSelectionMode = ( GroupLocationPickerMode ) jsonGroupType.LocationSelectionMode;
                    groupType.AttendanceCountsAsWeekendService = jsonGroupType.AttendanceCountsAsWeekendService;
                }

                groupTypeService.AddRange( groupTypesToAdd );
                groupTypes.AddRange( groupTypesToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );

                // Get all group types since we may have not imported everything
                // that is referenced.
                var allGroupTypes = groupTypeService.Queryable()
                    .AsNoTracking()
                    .ToList();

                // Do a second pass to update the inherited group type identfiers now
                // that everything is imported.
                foreach ( var jsonGroupType in jsonGroupTypes )
                {
                    var groupType = groupTypes.First( gt => gt.Guid == jsonGroupType.Guid );

                    groupType.InheritedGroupTypeId = jsonGroupType.InheritedGroupTypeGuid.HasValue
                        ? allGroupTypes.FirstOrDefault( gt => gt.Guid == jsonGroupType.InheritedGroupTypeGuid.Value )?.Id
                        : null;
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                // Do a third pass to set all attribute values.
                foreach ( var jsonGroupType in jsonGroupTypes )
                {
                    var groupType = groupTypes.First( gt => gt.Guid == jsonGroupType.Guid );

                    groupType.LoadAttributes( rockContext );

                    if ( jsonGroupType.AttributeValues != null )
                    {
                        foreach ( var attr in jsonGroupType.AttributeValues )
                        {
                            groupType.SetAttributeValue( attr.Key, attr.Value );
                        }
                    }

                    groupType.SaveAttributeValues( rockContext );
                }

                DetachAllEntities( rockContext );

                // This is bad, but we need to force load the group types into
                // cache because the groups will try to hit cache when loading
                // the attribute values.
                GroupTypeCache.Clear();
                GroupTypeCache.All( rockContext );

                Messages.Add( $"Imported {jsonGroupTypes.Count:N0} group types ({groupTypesToAdd.Count:N0} new)" );
            }

            private void ImportGroupTypeAssociations( List<JsonGroupTypeAssociation> jsonGroupTypeAssociations, RockContext rockContext )
            {
                var groupTypeService = new GroupTypeService( rockContext );
                var groupTypeGuids = jsonGroupTypeAssociations.Select( gta => gta.GroupTypeGuid )
                    .Union( jsonGroupTypeAssociations.Select( gta => gta.ChildGroupTypeGuid ) )
                    .Distinct();
                var groupTypes = BulkGet( groupTypeService.Queryable(), groupTypeGuids );
                var addCount = 0;

                foreach ( var groupType in groupTypes )
                {
                    var childGroupTypeGuids = jsonGroupTypeAssociations
                        .Where( gta => gta.GroupTypeGuid == groupType.Guid )
                        .Select( gta => gta.ChildGroupTypeGuid )
                        .Distinct()
                        .ToList();

                    var childGroupTypes = groupTypes
                        .Where( gt => childGroupTypeGuids.Contains( gt.Guid ) )
                        .ToList();

                    foreach ( var childGroupType in childGroupTypes )
                    {
                        if ( !groupType.ChildGroupTypes.Any( gt => gt.Id == childGroupType.Id ) )
                        {
                            groupType.ChildGroupTypes.Add( childGroupType );
                            addCount++;
                        }
                    }
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonGroupTypeAssociations.Count:N0} group type associations ({addCount:N0} new)" );
            }

            private void ImportGroups( List<JsonGroup> jsonGroups, RockContext rockContext )
            {
                var groupService = new GroupService( rockContext );
                var groupTypeService = new GroupTypeService( rockContext );
                var groups = BulkGet( groupService.Queryable(), jsonGroups.Select( g => g.Guid ) );
                var groupsToAdd = new List<Group>( jsonGroups.Count - groups.Count );

                var groupTypes = groupTypeService.Queryable().ToList();

                foreach ( var jsonGroup in jsonGroups )
                {
                    var group = groups.FirstOrDefault( g => g.Guid == jsonGroup.Guid );

                    if ( group == null )
                    {
                        group = new Group
                        {
                            Guid = jsonGroup.Guid
                        };

                        groupsToAdd.Add( group );
                    }

                    group.Name = jsonGroup.Name;
                    group.IsActive = jsonGroup.IsActive;
                    group.Order = jsonGroup.Order;
                    group.GroupTypeId = groupTypes.First( gt => gt.Guid == jsonGroup.GroupTypeGuid ).Id;
                }

                groupService.AddRange( groupsToAdd );
                groups.AddRange( groupsToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );

                // Do a second pass to update the parent group identfiers now
                // that everything is imported.
                foreach ( var jsonGroup in jsonGroups )
                {
                    var group = groups.First( g => g.Guid == jsonGroup.Guid );

                    group.ParentGroupId = jsonGroup.ParentGroupGuid.HasValue
                        ? groups.FirstOrDefault( g => g.Guid == jsonGroup.ParentGroupGuid.Value )?.Id
                        : null;
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                // Do a third pass to update all the attribute values.
                // that everything is imported.
                foreach ( var jsonGroup in jsonGroups )
                {
                    var group = groups.First( g => g.Guid == jsonGroup.Guid );

                    if ( jsonGroup.AttributeValues != null )
                    {
                        group.LoadAttributes( rockContext );

                        foreach ( var attr in jsonGroup.AttributeValues )
                        {
                            group.SetAttributeValue( attr.Key, attr.Value );
                        }

                        group.SaveAttributeValues( rockContext );
                    }
                }

                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonGroups.Count:N0} groups ({groupsToAdd.Count:N0} new)" );
            }

            private void ImportGroupLocations( List<JsonGroupLocation> jsonGroupLocations, RockContext rockContext )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var groupService = new GroupService( rockContext );
                var locationService = new LocationService( rockContext );
                var groupGuids = jsonGroupLocations.Select( gl => gl.GroupGuid ).Distinct();
                var locationGuids = jsonGroupLocations.Select( gl => gl.LocationGuid ) .Distinct();
                var groupLocations = groupLocationService.Queryable().ToList();
                var groups = BulkGet( groupService.Queryable(), groupGuids );
                var locations = BulkGet( locationService.Queryable(), locationGuids );
                var groupLocationsToAdd = new List<GroupLocation>();

                foreach ( var jsonGroupLocation in jsonGroupLocations )
                {
                    var groupId = groups.FirstOrDefault( g => g.Guid == jsonGroupLocation.GroupGuid )?.Id;
                    var locationId = locations.FirstOrDefault( l => l.Guid == jsonGroupLocation.LocationGuid )?.Id;

                    if ( !groupId.HasValue || !locationId.HasValue )
                    {
                        continue;
                    }

                    var groupLocation = groupLocations.FirstOrDefault( gl => gl.GroupId == groupId && gl.LocationId == locationId );

                    if ( groupLocation == null )
                    {
                        groupLocation = new GroupLocation
                        {
                            GroupId = groupId.Value,
                            LocationId = locationId.Value
                        };

                        groupLocationsToAdd.Add( groupLocation );
                    }

                    groupLocation.Guid = jsonGroupLocation.Guid;
                    groupLocation.Order = jsonGroupLocation.Order;
                }

                groupLocationService.AddRange( groupLocationsToAdd );

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonGroupLocations.Count:N0} group locations ({groupLocationsToAdd.Count:N0} new)" );
            }

            private void ImportGroupLocationSchedules( List<JsonGroupLocationSchedule> jsonGroupLocationSchedules, RockContext rockContext )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var scheduleService = new ScheduleService( rockContext );
                var groupLocationGuids = jsonGroupLocationSchedules.Select( gls => gls.GroupLocationGuid ).Distinct();
                var scheduleGuids = jsonGroupLocationSchedules.Select( gls => gls.ScheduleGuid ).Distinct();
                var groupLocationSchedules = BulkGet( groupLocationService.Queryable(), groupLocationGuids );
                var schedules = BulkGet( scheduleService.Queryable(), scheduleGuids );
                var addCount = 0;

                foreach ( var groupLocationSchedule in groupLocationSchedules )
                {
                    var groupLocationScheduleGuids = jsonGroupLocationSchedules
                        .Where( gls => gls.GroupLocationGuid == groupLocationSchedule.Guid )
                        .Select( gls => gls.ScheduleGuid )
                        .Distinct()
                        .ToList();

                    var locationSchedules = schedules
                        .Where( s => groupLocationScheduleGuids.Contains( s.Guid ) )
                        .ToList();

                    foreach ( var locationSchedule in locationSchedules )
                    {
                        if ( !groupLocationSchedule.Schedules.Any( s => s.Id == locationSchedule.Id ) )
                        {
                            groupLocationSchedule.Schedules.Add( locationSchedule );
                            addCount++;
                        }
                    }
                }

                rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );
                DetachAllEntities( rockContext );

                Messages.Add( $"Imported {jsonGroupLocationSchedules.Count:N0} group location schedules ({addCount:N0} new)" );
            }

            private List<T> BulkGet<T>( IQueryable<T> queryable, IEnumerable<Guid> guids )
                where T : class, IEntity
            {
                var items = new List<T>();
                var guidList = guids.ToList();

                while ( guidList.Count > 0 )
                {
                    var guidsChunk = guidList.Take( 1000 ).ToList();
                    guidList = guidList.Skip( 1000 ).ToList();

                    items.AddRange( queryable.Where( i => guidsChunk.Contains( i.Guid ) ) );
                }

                return items;
            }
        }

        #endregion

        #region POCOs

        private class JsonContainer
        {
            public List<JsonDevice> Devices { get; set; }

            public List<JsonLocation> Locations { get; set; }

            public List<JsonDeviceLocation> DeviceLocations { get; set; }

            public List<JsonCampus> Campuses { get; set; }

            public List<JsonCategory> ScheduleCategories { get; set; }

            public List<JsonSchedule> Schedules { get; set; }

            public List<JsonGroupType> GroupTypes { get; set; }

            public List<JsonGroupTypeAssociation> GroupTypeAssociations { get; set; }

            public List<JsonGroup> Groups { get; set; }

            public List<JsonGroupLocation> GroupLocations { get; set; }

            public List<JsonGroupLocationSchedule> GroupLocationSchedules { get; set; }
        }

        private class JsonAttributeValue
        {
            public string Key { get; set; }

            public Guid AttributeGuid { get; set; }

            public string Value { get; set; }
        }

        private class JsonDevice
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public Guid DeviceTypeValueGuid { get; set; }

            public Guid? PrinterDeviceGuid { get; set; }

            public bool IsActive { get; set; }
        }

        private class JsonLocation
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public Guid? ParentLocationGuid { get; set; }

            public bool IsActive { get; set; }

            public int? SoftRoomThreshold { get; set; }

            public int? HardRoomThreshold { get; set; }

            public Guid? PrinterDeviceGuid { get; set; }
        }

        private class JsonDeviceLocation
        {
            public Guid DeviceGuid { get; set; }

            public Guid LocationGuid { get; set; }
        }

        private class JsonCampus
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public string ShortCode { get; set; }

            public Guid? LocationGuid { get; set; }

            public bool IsActive { get; set; }

            public int Order { get; set; }

            public Guid? CampusStatusValueGuid { get; set; }

            public Guid? CampusTypeValueGuid { get; set; }
        }

        private class JsonCategory
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public Guid? ParentCategoryGuid { get; set; }

            public int Order { get; set; }
        }

        private class JsonSchedule
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public string iCalendarContent { get; set; }

            public int? CheckInStartOffsetMinutes { get; set; }

            public int? CheckInEndOffsetMinutes { get; set; }

            public DateTime? EffectiveStartDate { get; set; }

            public DateTime? EffectiveEndDate { get; set; }

            public Guid? CategoryGuid { get; set; }

            public bool IsActive { get; set; }

            public int Order { get; set; }
        }

        private class JsonGroupType
        {
            public Guid Guid { set; get; }

            public string Name { get; set; }

            public int AttendanceRule { get; set; }

            public int AttendancePrintTo { get; set; }

            public int Order { get; set; }

            public Guid? GroupTypePurposeValueGuid { get; set; }

            public Guid? InheritedGroupTypeGuid { get; set; }

            public int LocationSelectionMode { get; set; }

            public bool AttendanceCountsAsWeekendService { get; set; }

            public List<JsonAttributeValue> AttributeValues { get; set; }
        }

        private class JsonGroupTypeAssociation
        {
            public Guid GroupTypeGuid { get; set; }

            public Guid ChildGroupTypeGuid { get; set; }
        }

        private class JsonGroup
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public Guid? ParentGroupGuid { get; set; }

            public Guid GroupTypeGuid { get; set; }

            public bool IsActive { get; set; }

            public int Order { get; set; }

            public List<JsonAttributeValue> AttributeValues { get; set; }
        }

        private class JsonGroupLocation
        {
            public Guid Guid { get; set; }

            public Guid GroupGuid { get; set; }

            public Guid LocationGuid { get; set; }

            public int Order { get; set; }
        }

        private class JsonGroupLocationSchedule
        {
            public Guid GroupLocationGuid { get; set; }

            public Guid ScheduleGuid { get; set; }
        }

        #endregion
    }
}
