using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Serialization;
using DotLiquid;

using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace org.lakepointe.Checkin.Lava
{
    public class RockFilters : IRockStartup
    {
        public int StartupOrder { get; } = 0;

        public virtual void OnStartup()
        {
            LavaService.RegisterFilters( this.GetType() );
            Template.RegisterFilter( this.GetType() );
        }

        public static List<Person> CanCheckinRelationships( object input )
        {
            return CanCheckinRelationships( input, null );
        }
        public static List<Person> CanCheckinRelationships( object input, object checkinPersonObj )
        {
            var person = GetPerson( input );
            var checkInByPerson = GetPerson( checkinPersonObj );

            var checkinPeople = new List<Person>();
            if ( person == null )
            {
                return checkinPeople;
            }
            var rockContext = new RockContext();

            if ( checkInByPerson != null )
            {
                if ( checkInByPerson.Id != person.Id )
                {
                    checkinPeople.Add( checkInByPerson );
                }
            }

            var attendanceSvc = new AttendanceService( rockContext );
            int? checkinPersonAliasId = null;

            var today = RockDateTime.Today;
            var tomorrow = today.AddDays( 1 );
            // get the person who most recently checked them in
            checkinPersonAliasId = attendanceSvc.Queryable().AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == person.Id )
                .Where( a => a.StartDateTime > today )
                .Where( a => a.StartDateTime < tomorrow )
                .Where( a => a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .Select( a => a.CheckedInByPersonAliasId )
                .FirstOrDefault();

            if ( checkinPersonAliasId.HasValue )
            {
                var checkedInBy = new PersonAliasService( rockContext ).Get( checkinPersonAliasId.Value ).Person;

                if ( checkedInBy.Id != person.Id )
                {
                    // make sure we don't add the same person twice (checked in by and checked out by same)
                    if ( ( checkinPeople.Count < 1 ) || ( checkinPeople[0].Id != checkedInBy.Id ) )
                    {
                        checkinPeople.Add( checkedInBy );
                    }
                }
            }

            checkinPeople.AddRange(
                GetCanCheckinRelatedPeople( person.Id ).
                Where( p => !checkinPeople.Select( p1 => p1.Id ).Contains( p.Id ) ) );


            return checkinPeople;

        }

        public static List<Person> CanCheckinRelationships( object input, bool useCache )
        {
            return CanCheckinRelationships( input, null, useCache );
        }

        public static List<Person> CanCheckinRelationships( object input, object checkinByPersonObj, bool useCache )
        {
            var person = GetPerson( input );

            if ( person == null )
            {
                return new List<Person>();
            }
            List<Person> checkinRelationships = null;
            var checkInByPerson = GetPerson( checkinByPersonObj );
            string cacheKey = String.Format( "org_lakepointe.Checkin.CanCheckinRelationships_{0}_{1}", person.Id.ToString(),
                ( checkInByPerson == null ) ? String.Empty : checkInByPerson.Id.ToString() );
            if ( useCache )
            {
                var rockContext = new RockContext();
                checkinRelationships = RockCache.Get( cacheKey ) as List<Person>;

                if ( checkinRelationships != null )
                {
                    return checkinRelationships;
                }
            }

            checkinRelationships = CanCheckinRelationships( input, checkinByPersonObj );

            if ( checkinRelationships.Count > 0 && useCache )
            {
                RockCache.AddOrUpdate( cacheKey, null, checkinRelationships, DateTime.Now.AddSeconds( 30 ) );
            }

            return checkinRelationships;

        }

        private static List<Person> GetCanCheckinRelatedPeople( int personId )
        {
            var rockContext = new RockContext();
            List<Person> checkinPeople = new List<Person>();
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

            if ( knownRelationshipGroupType != null )
            {
                var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() );
                var inactivePersonRecordDV = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                if ( ownerRole != null )
                {
                    var canCheckInRoleIds = GetCanCheckInRoles();
                    var checkInRoles = new GroupTypeRoleService( rockContext ).Queryable().AsNoTracking()
                        .Where( r => canCheckInRoleIds.Contains( r.Id ) )
                        .OrderBy( r => r.Order );

                    var groupMemberService = new GroupMemberService( rockContext );

                    foreach ( var role in checkInRoles )
                    {
                        var checkinKRGroupIds = groupMemberService.Queryable().AsNoTracking()
                            .Where( gm => gm.Group.GroupTypeId == knownRelationshipGroupType.Id )
                            .Where( gm => gm.GroupRoleId == role.Id )
                            .Where( gm => gm.PersonId == personId )
                            .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( gm => gm.GroupId );

                        var people = groupMemberService.Queryable().AsNoTracking()
                            .Where( gm => checkinKRGroupIds.Contains( gm.GroupId ) )
                            .Where( gm => gm.GroupRoleId == ownerRole.Id )
                            .Where( gm => gm.Person.RecordStatusValueId != inactivePersonRecordDV.Id )
                            .Select( gm => gm.Person )
                            .ToList();

                        foreach ( var p in people )
                        {
                            if ( checkinPeople.Count( c => c.Id == p.Id ) == 0 )
                            {
                                checkinPeople.Add( p );
                            }
                        }

                    }

                }
            }

            return checkinPeople;
        }

        private static Person GetPerson( object input )
        {
            if ( input != null )
            {
                if ( input is int )
                {
                    return new PersonService( new RockContext() ).Get( ( int ) input );
                }

                var person = input as Person;
                if ( person != null )
                {
                    return person;
                }

                var checkinPerson = input as Rock.CheckIn.CheckInPerson;
                if ( checkinPerson != null )
                {
                    return checkinPerson.Person;
                }
            }

            return null;
        }

        private static List<int> GetCanCheckInRoles()
        {

            string cacheKey = "Rock.FindRelationships.Roles";

            List<int> roles = RockCache.Get( cacheKey ) as List<int>;

            if ( roles == null )
            {
                var rockContext = new RockContext();

                roles = new List<int>();

                foreach ( var role in new GroupTypeRoleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) ) )
                {
                    role.LoadAttributes( rockContext );
                    if ( role.Attributes.ContainsKey( "CanCheckin" ) )
                    {
                        bool canCheckIn = false;
                        if ( bool.TryParse( role.GetAttributeValue( "CanCheckin" ), out canCheckIn ) && canCheckIn )
                        {
                            roles.Add( role.Id );
                        }
                    }
                }

                RockCache.AddOrUpdate( cacheKey, null, roles, RockDateTime.Now.AddDays( 1 ) );
            }

            return roles;
        }

        private static List<int> GetFosterAdoptedRoles()
        {

            string cacheKey = "Rock.FindRelationships.FosterAndAdoptedRoles";

            List<int> roles = RockCache.Get( cacheKey ) as List<int>;

            if ( roles == null )
            {
                var rockContext = new RockContext();

                roles = new List<int>();

                foreach ( var role in new GroupTypeRoleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) ) )
                {
                    if ( role.Name.Contains( "Foster Child" ) || role.Name.Contains( "Adopted Child" ) )
                    {
                        roles.Add( role.Id );
                    }
                }

                RockCache.AddOrUpdate( cacheKey, null, roles, RockDateTime.Now.AddDays( 1 ) );
            }

            return roles;
        }

        private static Bitmap ImageAdjust( Bitmap originalImage, float brightness = 1.0f, float contrast = 1.0f, float gamma = 1.0f )
        {
            Bitmap adjustedImage = originalImage;

            float adjustedBrightness = brightness - 1.0f;
            // Matrix used to effect the image
            float[][] ptsArray = {
                new float[] { contrast, 0, 0, 0, 0 }, // scale red
                new float[] { 0, contrast, 0, 0, 0 }, // scale green
                new float[] { 0, 0, contrast, 0, 0 }, // scale blue
                new float[] { 0, 0, 0, 1.0f, 0 },     // no change to alpha
                new float[] { adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1 }
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix( new ColorMatrix( ptsArray ), ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
            imageAttributes.SetGamma( gamma, ColorAdjustType.Bitmap );
            Graphics g = Graphics.FromImage( adjustedImage );
            g.DrawImage( originalImage, new Rectangle( 0, 0, adjustedImage.Width, adjustedImage.Height ),
                0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes );

            return adjustedImage;
        }

        public static string GetLastAttendedDate( object input )
        {
            var person = GetPerson( input );
            var rockContext = new RockContext();

            var attendanceSvc = new AttendanceService( rockContext );

            DateTime date = attendanceSvc.Queryable().AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == person.Id )
                .Where( a => a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .Select( a => a.StartDateTime )
                .FirstOrDefault();

            string dateString = "";

            if ( !( date.Equals( DateTime.MinValue ) ) )
            {
                dateString = date.ToShortDateString();
            }
            return dateString;
        }

        public static string GetLastCheckinTime( object input )
        {
            var person = GetPerson( input );
            var rockContext = new RockContext();
            var today = RockDateTime.Today;
            var now = RockDateTime.Now;
            var attendanceSvc = new AttendanceService( rockContext );

            DateTime date = attendanceSvc.Queryable().AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == person.Id )
                .Where( a => a.RSVPDateTime == null )
                .Where( a => a.DidAttend == true )
                .Where( a => ( a.StartDateTime > today ) && ( a.StartDateTime <= now ) )
                .OrderByDescending( a => a.StartDateTime )
                .Select( a => a.StartDateTime )
                .FirstOrDefault();

            string dateString = "Record Not Found";

            if ( !( date.Equals( DateTime.MinValue ) ) )
            {
                dateString = date.ToShortTimeString();
            }
            return dateString;
        }

        public static string GetLastCheckoutTime( object input )
        {
            var person = GetPerson( input );
            var rockContext = new RockContext();
            var today = RockDateTime.Today;
            var now = RockDateTime.Now;
            var attendanceSvc = new AttendanceService( rockContext );

            DateTime? date = attendanceSvc.Queryable().AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == person.Id )
                .Where( a => a.RSVPDateTime == null )
                .Where( a => a.DidAttend == true )
                .Where( a => ( a.StartDateTime > today ) && ( a.StartDateTime <= now ) )
                .OrderByDescending( a => a.StartDateTime )
                .Select( a => a.EndDateTime )
                .FirstOrDefault();

            string dateString = "Checked In";

            if ( date != null && !( date.Equals( DateTime.MinValue ) ) )
            {
                dateString = date.Value.ToShortTimeString();
            }
            return dateString;
        }

        /// <summary>
        /// Gets the last attendance item for a given person in a group provided
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public static Attendance LastAttendanceInGroup( object input, string groupId )
        {
            var person = GetPerson( input );
            int? numericalGroupId = groupId.AsIntegerOrNull();

            if ( person == null && !numericalGroupId.HasValue )
            {
                return new Attendance();
            }

            return new AttendanceService( new RockContext() ).Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group != null &&
                    a.Occurrence.Group.Id == numericalGroupId &&
                    a.PersonAlias.PersonId == person.Id &&
                    a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the first attendance item for a given person in a group provided
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public static Attendance FirstAttendanceInGroup( object input, string groupId )
        {
            var person = GetPerson( input );
            int? numericalGroupId = groupId.AsIntegerOrNull();

            if ( person == null && !numericalGroupId.HasValue )
            {
                return new Attendance();
            }

            return new AttendanceService( new RockContext() ).Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group != null &&
                    a.Occurrence.Group.Id == numericalGroupId &&
                    a.PersonAlias.PersonId == person.Id &&
                    a.DidAttend == true )
                .OrderBy( a => a.StartDateTime )
                .FirstOrDefault();
        }


        public static Boolean FosterAdoptedChildCheck( object input_person )
        {
            var person = GetPerson( input_person );

            bool isFosterOrAdoptedChild = false;

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

            if ( knownRelationshipGroupType != null )
            {
                var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() );
                var inactivePersonRecordDV = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                if ( ownerRole != null )
                {
                    var rockContext = new RockContext();

                    var canCheckInRoleIds = GetFosterAdoptedRoles();
                    var checkInRoles = new GroupTypeRoleService( rockContext ).Queryable().AsNoTracking()
                        .Where( r => canCheckInRoleIds.Contains( r.Id ) )
                        .OrderBy( r => r.Order );

                    var groupMemberService = new GroupMemberService( rockContext );

                    foreach ( var role in checkInRoles )
                    {
                        var checkinKRGroupIds = groupMemberService.Queryable().AsNoTracking()
                            .Where( gm => gm.Group.GroupTypeId == knownRelationshipGroupType.Id )
                            .Where( gm => gm.GroupRoleId == role.Id )
                            .Where( gm => gm.PersonId == person.Id )
                            .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( gm => gm.GroupId );

                        isFosterOrAdoptedChild = groupMemberService.Queryable().AsNoTracking()
                            .Where( gm => checkinKRGroupIds.Contains( gm.GroupId ) )
                            .Where( gm => gm.GroupRoleId == ownerRole.Id )
                            .Where( gm => gm.Person.RecordStatusValueId != inactivePersonRecordDV.Id )
                            .Select( gm => gm.Person )
                            .Any();
                        if ( isFosterOrAdoptedChild )
                        {
                            break;
                        }
                    }
                }
            }

            return isFosterOrAdoptedChild;

        }

        public static int GetCampusId( object input )
        {
            var person = GetPerson( input );
            var rockContext = new RockContext();
            var today = RockDateTime.Today;
            var now = RockDateTime.Now;
            var attendanceSvc = new AttendanceService( rockContext );

            int? campid = attendanceSvc.Queryable().AsNoTracking()
                .Where( a => a.PersonAlias.PersonId == person.Id )
                .Where( a => a.RSVPDateTime == null )
                .Where( a => a.DidAttend == true )
                .Where( a => ( a.StartDateTime > today ) && ( a.StartDateTime <= now ) )
                .OrderByDescending( a => a.StartDateTime )
                .Select( a => a.CampusId )
                .FirstOrDefault();

            if ( campid != null )
            {
                return ( int ) campid;
            }
            return -1;
        }

        public static Campus CampusByCheckinPerson( object input )
        {
            var campusId = GetCampusId( input );

            if ( campusId == -1 )
            {
                return null;
            }

            return new CampusService( new RockContext() ).Queryable().AsNoTracking().Where( c => c.Id == campusId ).FirstOrDefault();
        }

        /// <summary>
        /// Wheres the specified input. Customized to work with collections of AttributeMatrixItems from
        /// an AttributeMatrix to work around the Legacy Lava issue with the Rock Where filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterKey">The filter key.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <returns>The first item matching the filterKey/filterValue</returns>
        public static object MatrixWhere( object input, string filterKey, object filterValue )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                foreach ( object itemObject in input as IEnumerable )
                {
                    var item = itemObject as AttributeMatrixItem;
                    item.LoadAttributes();
                    if ( item.AttributeValues.ContainsKey( filterKey ) && item.AttributeValues[filterKey].ValueFormatted.Equals( filterValue ) )
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public static bool ServingCheckinSetupAutomation( object input, int groupid, string servingTeamName, int parentCheckinGroupId, int childGroupTypeId, int dataViewParentCategoryId, int weekendDTId )
        {
            var rockContext = new RockContext();
            GroupService gs = new GroupService(rockContext);
            GroupSyncService gss = new GroupSyncService(rockContext);
            CategoryService cs = new CategoryService(rockContext);
            DataViewService dvs = new DataViewService(rockContext);
            DataViewFilterService dvfs = new DataViewFilterService(rockContext);

            Group inputGroup = inputGroup = new GroupService(rockContext).Get((int)groupid);
            GroupType inputGroupType = new GroupTypeService(rockContext).Get(inputGroup.GroupTypeId);
            GroupType childGroupType = new GroupTypeService(rockContext).Get(childGroupTypeId);
            string inputGroupCampusGuid = inputGroup.Campus.Guid.ToString();
            Campus inputGroupCampus = new CampusService(rockContext).Get(inputGroup.Campus.Guid);
            string campusShortCode = inputGroupCampus.ShortCode;
            DefinedType weekendServicedt = new DefinedTypeService(rockContext).Get(weekendDTId);
            weekendServicedt.LoadAttributes();

            // Query to figure out group member attribute guid for attributeFilter
            AttributeService attrSvc = new AttributeService(rockContext);
            string groupMemberAttributeGuid = string.Empty;
            Guid? possGmAttrGuid = attrSvc.Queryable().AsNoTracking()
                .Where(a => a.EntityTypeQualifierValue == inputGroupType.Id.ToString())
                .Where(a => a.FieldTypeId == 16)
                .Where(a => a.EntityTypeId == 90)
                .Select(a => a.Guid)
                .First();

            if (possGmAttrGuid == null)
            {
                return false;
            }
            groupMemberAttributeGuid = possGmAttrGuid.ToString();

            // Get Child Group Type Roles
            GroupTypeRole childMemberRole = new GroupTypeRole();
            GroupTypeRole childLeaderRole = new GroupTypeRole();
            foreach (GroupTypeRole gtr in childGroupType.Roles)
            {
                if (gtr.IsLeader)
                {
                    childLeaderRole = gtr;
                }
                else
                {
                    childMemberRole = gtr;
                }
            }


            // Create DataView Categories
            Category parentCategory = new Category();
            parentCategory.Name = campusShortCode + " " + servingTeamName;
            parentCategory.ParentCategoryId = dataViewParentCategoryId;
            parentCategory.EntityTypeId = 34;
            cs.Add(parentCategory);
            rockContext.SaveChanges();

            // Create Parent Group For Serving Team under Campus
            Group campusServingParent = new Group();
            campusServingParent.ParentGroupId = parentCheckinGroupId;
            campusServingParent.GroupTypeId = childGroupTypeId;
            campusServingParent.Campus = inputGroupCampus;
            campusServingParent.Name = campusShortCode + " " + servingTeamName + " VOLUNTEERS";
            gs.Add(campusServingParent);
            rockContext.SaveChanges();

            // Create Variables
            Category groupMemberSubCategory = new Category();
            Category personSubCategory = new Category();

            groupMemberSubCategory.Name = campusShortCode + " Group Member Data Views";
            groupMemberSubCategory.ParentCategoryId = parentCategory.Id;
            groupMemberSubCategory.EntityTypeId = 34;
            cs.Add(groupMemberSubCategory);
            rockContext.SaveChanges();

            personSubCategory.Name = campusShortCode + " Person Data Views";
            personSubCategory.ParentCategoryId = parentCategory.Id;
            personSubCategory.EntityTypeId = 34;
            cs.Add(personSubCategory);
            rockContext.SaveChanges();

            foreach (DefinedValue dv in weekendServicedt.DefinedValues)
            {
                dv.LoadAttributes();
                if (dv.AttributeValues["Campus"].Value.Equals(inputGroupCampusGuid))
                {
                    // Create Group For Each Service Time
                    Group serviceTimeParent = new Group();
                    serviceTimeParent.ParentGroupId = campusServingParent.Id;
                    serviceTimeParent.GroupTypeId = childGroupTypeId;
                    serviceTimeParent.Campus = inputGroupCampus;
                    serviceTimeParent.Name = campusShortCode + " " + dv.AttributeValues["AnalyticsValue"].ToString().ToUpper() + " SERVICE TIME";
                    gs.Add(serviceTimeParent);
                    rockContext.SaveChanges();

                    // Create two data view categories for each service time
                    Category personServiceTimeDVC = new Category();
                    personServiceTimeDVC.Name = campusShortCode + " " + dv.AttributeValues["AnalyticsValue"].ToString().ToUpper() + " Service Time";
                    personServiceTimeDVC.ParentCategoryId = personSubCategory.Id;
                    personServiceTimeDVC.EntityTypeId = 34;
                    cs.Add(personServiceTimeDVC);
                    rockContext.SaveChanges();

                    Category gmServiceTimeDVC = new Category();
                    gmServiceTimeDVC.Name = campusShortCode + " " + dv.AttributeValues["AnalyticsValue"].ToString().ToUpper() + " Service Time";
                    gmServiceTimeDVC.ParentCategoryId = groupMemberSubCategory.Id;
                    gmServiceTimeDVC.EntityTypeId = 34;
                    cs.Add(gmServiceTimeDVC);
                    rockContext.SaveChanges();

                    // Create map to keep track of created person data views -> roles
                    var personDataViews = new Dictionary<GroupTypeRole, DataView>();

                    // Create Two Data Views for Each Role Under Each Service Time
                    foreach (GroupTypeRole gtr in inputGroupType.Roles)
                    {
                        // Create Group Member Data View
                        DataView groupMemberDV = new DataView();
                        groupMemberDV.Name = campusShortCode + " " + dv.AttributeValues["AnalyticsValue"].ToString().ToUpper() + " " + gtr.Name;
                        groupMemberDV.CategoryId = gmServiceTimeDVC.Id;
                        groupMemberDV.EntityTypeId = 90;
                        groupMemberDV.Description = "";
                        dvs.Add(groupMemberDV);
                        rockContext.SaveChanges();

                        DataViewFilter groupMemberDVParentFilter = new DataViewFilter();
                        groupMemberDVParentFilter.ExpressionType = FilterExpressionType.GroupAll;
                        groupMemberDV.DataViewFilter = groupMemberDVParentFilter;
                        dvfs.Add(groupMemberDVParentFilter);

                        DataViewFilter roleFilter = new DataViewFilter();
                        roleFilter.Selection = string.Format("[\"Property_GroupRoleId\",\"1\",\"{0}\"]", gtr.Id);
                        roleFilter.Parent = groupMemberDVParentFilter;
                        roleFilter.DataView = groupMemberDV;
                        roleFilter.EntityTypeId = 121;
                        dvfs.Add(roleFilter);
                        groupMemberDVParentFilter.ChildFilters.Add(roleFilter);
                        rockContext.SaveChanges();

                        DataViewFilter groupFilter = new DataViewFilter();
                        groupFilter.Selection = string.Format("[\"Property_GroupId\",\"1\",\"{0}\"]", inputGroup.Id);
                        groupFilter.Parent = groupMemberDVParentFilter;
                        groupFilter.DataView = groupMemberDV;
                        groupFilter.EntityTypeId = 121;
                        dvfs.Add(groupFilter);
                        groupMemberDVParentFilter.ChildFilters.Add(groupFilter);
                        rockContext.SaveChanges();

                        DataViewFilter attributeFilter = new DataViewFilter();
                        attributeFilter.Selection = string.Format("Attribute_WeekendServiceHours_{0}|8|{1}", groupMemberAttributeGuid.Replace("-", ""), dv.Guid.ToString());
                        attributeFilter.Parent = groupMemberDVParentFilter;
                        attributeFilter.DataView = groupMemberDV;
                        attributeFilter.EntityTypeId = 332;
                        dvfs.Add(attributeFilter);
                        groupMemberDVParentFilter.ChildFilters.Add(attributeFilter);
                        rockContext.SaveChanges();


                        // Create Person Data View
                        DataView personDV = new DataView();
                        personDV.Name = campusShortCode + " " + dv.AttributeValues["AnalyticsValue"].ToString().ToUpper() + " " + gtr.Name;
                        personDV.CategoryId = personServiceTimeDVC.Id;
                        personDV.EntityTypeId = 15;
                        personDV.Description = "";
                        dvs.Add(personDV);
                        rockContext.SaveChanges();

                        DataViewFilter personDVParentFilter = new DataViewFilter();
                        personDVParentFilter.ExpressionType = FilterExpressionType.GroupAll;
                        personDV.DataViewFilter = personDVParentFilter;
                        dvfs.Add(personDVParentFilter);

                        DataViewFilter personFilter = new DataViewFilter();
                        personFilter.Selection = string.Format("{0}|GreaterThanOrEqualTo|1", groupMemberDV.Guid.ToString());
                        personFilter.Parent = personDVParentFilter;
                        personFilter.DataView = personDV;
                        personFilter.EntityTypeId = 341;
                        dvfs.Add(personFilter);
                        personDVParentFilter.ChildFilters.Add(personFilter);
                        rockContext.SaveChanges();

                        personDataViews[gtr] = personDV;
                    }

                    // Create Group for Each Role Under Each Service Time
                    foreach (GroupTypeRole gtr in inputGroupType.Roles)
                    {
                        if (!gtr.IsLeader)
                        {
                            Group roleChildGroup = new Group();
                            roleChildGroup.ParentGroupId = serviceTimeParent.Id;
                            roleChildGroup.GroupTypeId = childGroupTypeId;
                            roleChildGroup.Campus = inputGroupCampus;
                            string roleChildGroupName = dv.AttributeValues["AnalyticsValue"].ToString();
                            roleChildGroup.Name = campusShortCode + " " + roleChildGroupName.Substring(0, roleChildGroupName.Length - 2) + gtr.Name + " Team";
                            gs.Add(roleChildGroup);
                            rockContext.SaveChanges();

                            // Find member dataview
                            DataView memberDV = personDataViews[gtr];

                            // Find leader dataview
                            string searchGTRName0 = gtr.Name + " Coach";
                            DataView coachDv = new DataView();
                            GroupTypeRole coachGtr = new GroupTypeRole();
                            foreach (GroupTypeRole gtr1 in personDataViews.Keys)
                            {
                                if (gtr1.Name.Equals(searchGTRName0))
                                {
                                    coachDv = personDataViews[gtr1];
                                    coachGtr = gtr1;
                                    break;
                                }
                            }

                            // Add sync to created group
                            if (memberDV.Name != null)
                            {
                                GroupSync memberSync = new GroupSync();
                                memberSync.GroupId = roleChildGroup.Id;
                                memberSync.GroupTypeRoleId = childMemberRole.Id;
                                memberSync.SyncDataViewId = memberDV.Id;
                                memberSync.ScheduleIntervalMinutes = 720;
                                gss.Add(memberSync);
                                rockContext.SaveChanges();
                                roleChildGroup.GroupSyncs.Add(memberSync);

                            }
                            else
                            {
                                // This can't be missing...
                                return false;
                            }

                            // Coach DV is now optional
                            if (coachDv.Name != null)
                            {
                                GroupSync coachSync = new GroupSync();
                                coachSync.GroupId = roleChildGroup.Id;
                                coachSync.GroupTypeRoleId = childLeaderRole.Id;
                                coachSync.SyncDataViewId = coachDv.Id;
                                coachSync.ScheduleIntervalMinutes = 720;
                                gss.Add(coachSync);
                                rockContext.SaveChanges();
                                roleChildGroup.GroupSyncs.Add(coachSync);
                            }
                            rockContext.SaveChanges();
                        }
                    }
                }
            }
            return true;
        }

        public static bool ServingCheckinSetupAutomationNoTimes( object input, int groupid, string servingTeamName, int parentCheckinGroupId, int childGroupTypeId, int dataViewParentCategoryId )
        {
            var rockContext = new RockContext();
            GroupService gs = new GroupService( rockContext );
            GroupSyncService gss = new GroupSyncService( rockContext );
            CategoryService cs = new CategoryService( rockContext );
            DataViewService dvs = new DataViewService( rockContext );
            DataViewFilterService dvfs = new DataViewFilterService( rockContext );

            Group inputGroup = inputGroup = new GroupService( rockContext ).Get( ( int ) groupid );
            GroupType inputGroupType = new GroupTypeService( rockContext ).Get( inputGroup.GroupTypeId );
            GroupType childGroupType = new GroupTypeService( rockContext ).Get( childGroupTypeId );
            string inputGroupCampusGuid = inputGroup.Campus.Guid.ToString();
            Campus inputGroupCampus = new CampusService( rockContext ).Get( inputGroup.Campus.Guid );
            string campusShortCode = inputGroupCampus.ShortCode;


            // Get Child Group Type Roles
            GroupTypeRole childMemberRole = new GroupTypeRole();
            GroupTypeRole childLeaderRole = new GroupTypeRole();
            foreach ( GroupTypeRole gtr in childGroupType.Roles )
            {
                if ( gtr.IsLeader )
                {
                    childLeaderRole = gtr;
                }
                else
                {
                    childMemberRole = gtr;
                }
            }


            // Create DataView Categories
            Category parentCategory = new Category();
            parentCategory.Name = campusShortCode + " " + servingTeamName;
            parentCategory.ParentCategoryId = dataViewParentCategoryId;
            parentCategory.EntityTypeId = 34;
            cs.Add( parentCategory );
            rockContext.SaveChanges();

            // Create Parent Group For Serving Team under Campus
            Group campusServingParent = new Group();
            campusServingParent.ParentGroupId = parentCheckinGroupId;
            campusServingParent.GroupTypeId = 479;
            campusServingParent.Campus = inputGroupCampus;
            campusServingParent.Name = campusShortCode + " " + servingTeamName + " VOLUNTEERS";
            gs.Add(campusServingParent);
            rockContext.SaveChanges();

            // Create Variables
            Category weekendGroupMemberSubCategory = new Category();
            Category weekendPersonSubCategory = new Category();
            Category weekdayGroupMemberSubCategory = new Category();
            Category weekdayPersonSubCategory = new Category();
            Category groupMemberSubCategory = new Category();
            Category personSubCategory = new Category();
            Group weekendServingParent = new Group();
            Group weekdayServingParent = new Group();


            groupMemberSubCategory.Name = campusShortCode + " Group Member Data Views";
            groupMemberSubCategory.ParentCategoryId = parentCategory.Id;
            groupMemberSubCategory.EntityTypeId = 34;
            cs.Add( groupMemberSubCategory );
            rockContext.SaveChanges();

            personSubCategory.Name = campusShortCode + " Person Data Views";
            personSubCategory.ParentCategoryId = parentCategory.Id;
            personSubCategory.EntityTypeId = 34;
            cs.Add( personSubCategory );
            rockContext.SaveChanges();



            // Set Dataview Parent Categories Correctly
            Category weekdayGroupMemberSub = new Category();
            Category weekdayPersonSub = new Category();
            Group weekdayParent = new Group();

            weekdayGroupMemberSub = groupMemberSubCategory;
            weekdayPersonSub = personSubCategory;
            weekdayParent = campusServingParent;


                    // Create map to keep track of created person data views -> roles
                    var personDataViews = new Dictionary<GroupTypeRole, DataView>();

                    // Create Two Data Views for Each Role Under Each Service Time
                    foreach ( GroupTypeRole gtr in inputGroupType.Roles )
                    {
                        // Create Group Member Data View
                        DataView groupMemberDV = new DataView();
                groupMemberDV.Name = campusShortCode  + " " + gtr.Name;
                groupMemberDV.CategoryId = groupMemberSubCategory.Id;
                        groupMemberDV.EntityTypeId = 90;
                        groupMemberDV.Description = "";
                        dvs.Add( groupMemberDV );
                        rockContext.SaveChanges();

                        DataViewFilter groupMemberDVParentFilter = new DataViewFilter();
                        groupMemberDVParentFilter.ExpressionType = FilterExpressionType.GroupAll;
                        groupMemberDV.DataViewFilter = groupMemberDVParentFilter;
                        dvfs.Add( groupMemberDVParentFilter );

                        DataViewFilter roleFilter = new DataViewFilter();
                        roleFilter.Selection = string.Format( "[\"Property_GroupRoleId\",\"1\",\"{0}\"]", gtr.Id );
                        roleFilter.Parent = groupMemberDVParentFilter;
                        roleFilter.DataView = groupMemberDV;
                        roleFilter.EntityTypeId = 121;
                        dvfs.Add( roleFilter );
                        groupMemberDVParentFilter.ChildFilters.Add( roleFilter );
                        rockContext.SaveChanges();

                        DataViewFilter groupFilter = new DataViewFilter();
                        groupFilter.Selection = string.Format( "[\"Property_GroupId\",\"1\",\"{0}\"]", inputGroup.Id );
                        groupFilter.Parent = groupMemberDVParentFilter;
                        groupFilter.DataView = groupMemberDV;
                        groupFilter.EntityTypeId = 121;
                        dvfs.Add( groupFilter );
                        groupMemberDVParentFilter.ChildFilters.Add( groupFilter );
                        rockContext.SaveChanges();


                        // Create Person Data View
                        DataView personDV = new DataView();
                personDV.Name = campusShortCode + " " + gtr.Name;
                personDV.CategoryId = personSubCategory.Id;
                        personDV.EntityTypeId = 15;
                        personDV.Description = "";
                        dvs.Add( personDV );
                        rockContext.SaveChanges();

                        DataViewFilter personDVParentFilter = new DataViewFilter();
                        personDVParentFilter.ExpressionType = FilterExpressionType.GroupAll;
                        personDV.DataViewFilter = personDVParentFilter;
                        dvfs.Add( personDVParentFilter );

                        DataViewFilter personFilter = new DataViewFilter();
                        personFilter.Selection = string.Format( "{0}|GreaterThanOrEqualTo|1", groupMemberDV.Guid.ToString() );
                        personFilter.Parent = personDVParentFilter;
                        personFilter.DataView = personDV;
                        personFilter.EntityTypeId = 341;
                        dvfs.Add( personFilter );
                        personDVParentFilter.ChildFilters.Add( personFilter );
                        rockContext.SaveChanges();

                        personDataViews[gtr] = personDV;
                    }

                    // Create Group for Each Role Under Each Service Time
                    foreach ( GroupTypeRole gtr in inputGroupType.Roles )
                    {
                        if ( !gtr.IsLeader )
                        {
                            Group roleChildGroup = new Group();
                    roleChildGroup.ParentGroupId = campusServingParent.Id;
                            roleChildGroup.GroupTypeId = childGroupTypeId;
                            roleChildGroup.Campus = inputGroupCampus;
                    roleChildGroup.Name = campusShortCode + " "  + gtr.Name + " Team";
                            gs.Add( roleChildGroup );
                            rockContext.SaveChanges();

                            // Find member dataview
                            DataView memberDV = personDataViews[gtr];

                            // Find leader dataview
                    string searchGTRName = "Coach";
                            DataView coachDv = new DataView();
                            GroupTypeRole coachGtr = new GroupTypeRole();
                            foreach ( GroupTypeRole gtr1 in personDataViews.Keys )
                            {
                                if ( gtr1.Name.Equals( searchGTRName ) )
                                {
                                    coachDv = personDataViews[gtr1];
                                    coachGtr = gtr1;
                                    break;
                                }
                            }

                            // Add sync to created group
                    if (memberDV.Name != null)
                    {
                            GroupSync memberSync = new GroupSync();
                            memberSync.GroupId = roleChildGroup.Id;
                            memberSync.GroupTypeRoleId = childMemberRole.Id;
                            memberSync.SyncDataViewId = memberDV.Id;
                            memberSync.ScheduleIntervalMinutes = 720;
                            gss.Add( memberSync );
                            rockContext.SaveChanges();
                        roleChildGroup.GroupSyncs.Add(memberSync);

                    }
                    else
                    {
                        // This can't be missing...
                        return false;
                    }

                    // Coach DV is now optional
                    if (coachDv.Name != null)
                    {
                            GroupSync coachSync = new GroupSync();
                            coachSync.GroupId = roleChildGroup.Id;
                            coachSync.GroupTypeRoleId = childLeaderRole.Id;
                            coachSync.SyncDataViewId = coachDv.Id;
                            coachSync.ScheduleIntervalMinutes = 720;
                        //gss.Add(coachSync);
                        rockContext.SaveChanges();
                            gss.Add( coachSync );
                            rockContext.SaveChanges();
                            roleChildGroup.GroupSyncs.Add( coachSync );
                    }
                            rockContext.SaveChanges();
                        }
          
            }
            return true;
        }

        // https://en.wikipedia.org/wiki/YUV
        // This can be used to create a set of visually distinct colors for charts and graphs.
        // The input to the filter is the index;
        // the first parameter is the total number of colors that will be provided (by distinct calls to the filter);
        // The optional third parameter is the luminance value.
        // After looking at the results, I'm not convinced this is the best algorithm. The theory is that this
        // algorithm find N colors equally spaced around the color wheel. The color space chosen is YUV, the theory
        // being that this space is optimized to best map to human perception of color and thereby provides the best
        // environment in which to create visual distinction. In practice, fixing the luminance value seems to severely
        // restrict the diversity of color options.
        // Look for a better algorithm.

        public static string ColorWheel( object input, int segments )
        {
            return ColorWheel( input, segments, 0.5 );
        }

        public static string ColorWheel( object input, int segments, double luminance )
        {
            double index = 0;
            if ( input != null && input is int )
            {
                index = ( int ) input;
            }

            var phi = index / segments * 2 * Math.PI;
            var u = Math.Cos( phi ) * 0.436;
            var v = Math.Sin( phi ) * 0.615;
            var r = ( int ) ( ( luminance + 1.13983 * v ) * 255 );
            var g = ( int ) ( ( luminance - 0.39465 * u - 0.5806 * v ) * 255 );
            var b = ( int ) ( ( luminance + 2.03211 * u ) * 255 );
            r = r < 0 ? 0 : r > 255 ? 255 : r;
            g = g < 0 ? 0 : g > 255 ? 255 : g;
            b = b < 0 ? 0 : b > 255 ? 255 : b;

            return $"{r:X2}{g:X2}{b:X2}";
        }

        // Sunflower? https://stackoverflow.com/questions/28567166/uniformly-distribute-x-points-inside-a-circle#:~:text=function%20sunflower(n,root%0A%20%20%20%20end%0Aend
        // This may still not be ideal but it has a couple of advantages over ColorWheel.
        // It spreads the colors over the full cirle of the
        // UV space so there is more distinction between colors for a given number of total colors.
        // The algorithm also results in adjacent colors (by index) being more significantly displaced. This is especially
        // good with bar charts and the like where adjacent indexes appear side-by-side.
        // One thing I still don't like about it is that the colors spiral out from the center, dark colors first and light
        // colors last. Would be nice to find a way to tweak the algorithm so the indexes bounce back and forth with some
        // kind of even distribution. Think about a reverse-binary-search approach.
        public static string ColorPlate( object input, int colors )
        {
            double index = 1;
            if ( input != null && input is int )
            {
                index = ( int ) input;
            }

            var goldenRatio = ( Math.Sqrt( 5 ) + 1 ) / 2;
            var radius = index > colors ? 1d : Math.Sqrt( index - 0.5d ) / Math.Sqrt( colors - 0.5d );
            var phi = 2 * Math.PI * index / Math.Pow( goldenRatio, 2 );
            
            var u = Math.Cos( phi ) * radius;
            var v = Math.Sin( phi ) * radius;
            var r = ( int ) ( ( radius + 1.13983 * v ) * 255 );
            var g = ( int ) ( ( radius - 0.39465 * u - 0.5806 * v ) * 255 );
            var b = ( int ) ( ( radius + 2.03211 * u ) * 255 );
            r = r < 0 ? 0 : r > 255 ? 255 : r;
            g = g < 0 ? 0 : g > 255 ? 255 : g;
            b = b < 0 ? 0 : b > 255 ? 255 : b;

            return $"{r:X2}{g:X2}{b:X2}";
        }

        public static string[] ColorPalette( object input )
        {
            int colors = 0;
            if ( input != null && input is int )
            {
                colors = ( int ) input;
            }

            string[] result = new string[colors];

            for ( var i = 0; i < colors; i++ )
            {
                result[i] = ColorPlate( i + 1, colors );
            }

            return result;
        }

        #region Schedule Filters

        // Schedule data is very difficult to manipulate in Lava for a variety of reasons. This makes it impractical to have a
        // generic solution to returning this information in a way that a variety of applications can get what they want. Thus
        // we wind up having to do a custom filter for almost every use case where we need to access schedule data from Lava.
        // We started out using {% execute %} clauses to do this. That is effective but requires compilation of the embedded C#
        // on the IIS server each time the Lava is executed. That has a several-second penalty associated with it that is not
        // acceptable for many applications. Hence pre-compiled C# in Lava filters. We also tried using Lava {% cache %} clauses
        // to mitigate the compilation delays in cases where the same result could be expected in most cases (as is the case with
        // first use case below). Unfortunately we ran into problems where the cache key logic seems to be broken and the cache
        // would return content for a different key.

        // Note that the Fluid engine was not available when this was implemented. It is possible that the new Entity tools
        // available in Fluid.Lava would provide a path to this without resorting to a custom filter.

        // And so we begin ...

        // First define a data structure to return the results so we don't have to modem through JSON ...
        public class PreviewOpportunities : DotLiquid.ILiquidizable, ILavaDataDictionarySource
        {
            public string Service { get; set; }
            public string Date { get; set; }
            public string Duration { get; set; }

            ILavaDataDictionary ILavaDataDictionarySource.GetLavaDataDictionary()
            {
                var dictionary = this.ToLiquid( false ) as Dictionary<string, object>;

                return new LavaDataDictionary( dictionary );
            }

            /// <summary>
            /// Creates a DotLiquid compatible dictionary that represents the current entity object. 
            /// </summary>
            /// <returns>DotLiquid compatible dictionary.</returns>
            public object ToLiquid()
            {
                return this.ToLiquid( false );
            }

            /// <summary>
            /// Creates a Lava compatible dictionary that represents the current store model.
            /// </summary>
            /// <param name="debug">if set to <c>true</c> the entire object tree will be parsed immediately.</param>
            /// <returns>
            /// A Lava compatible dictionary.
            /// </returns>
            public object ToLiquid( bool debug )
            {
                var dictionary = new Dictionary<string, object>();

                Type entityType = this.GetType();

                foreach ( var propInfo in entityType.GetProperties() )
                {
                    object propValue = propInfo.GetValue( this, null );

                    if ( propValue is Guid )
                    {
                        propValue = ( ( Guid ) propValue ).ToString();
                    }

                    if ( LavaService.RockLiquidIsEnabled )
                    {
                        if ( debug && propValue is DotLiquid.ILiquidizable )
                        {
                            dictionary.Add( propInfo.Name, ( ( DotLiquid.ILiquidizable ) propValue ).ToLiquid() );
                        }
                        else
                        {
                            dictionary.Add( propInfo.Name, propValue );
                        }
                    }
                    else
                    {
                        if ( debug && propValue is ILavaDataDictionarySource )
                        {
                            dictionary.Add( propInfo.Name, ( ( ILavaDataDictionarySource ) propValue ).GetLavaDataDictionary() );
                        }
                        else
                        {
                            dictionary.Add( propInfo.Name, propValue );
                        }
                    }

                }

                return dictionary;
            }
        }

        // Return preview options for the specified schedules
        public static PreviewOpportunities[] GetPreviews( object input, int mininumDays = 2, int maximumDays = 30, int cacheDelay = 3600 )
        {
            if ( input == null || !( input is string ) )
            {
                return null;
            }

            var inputString = ( string ) input;
            if ( inputString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var ids = ( ( string ) input ).Split( ',' ).Select( id => id.ToIntSafe() );
            var cacheKey = string.Format( "serve-preview-{0}", string.Join( "-", ids ) );
            return ( PreviewOpportunities[] ) RockCache.GetOrAddExisting( cacheKey, null, () =>
            {
                List<PreviewOpportunities> result = new List<PreviewOpportunities>();

                var scheduleService = new ScheduleService( new RockContext() );
                foreach ( var id in ids )
                {
                    var schedule = scheduleService.Get( id );
                    var occurrences = schedule.GetICalOccurrences( DateTime.Today.AddDays( mininumDays ), DateTime.Today.AddDays( maximumDays ) );
                    foreach ( var occurrence in occurrences )
                    {
                        var start = DateTime.SpecifyKind( occurrence.Period.StartTime.Value, DateTimeKind.Local );
                        result.Add( new PreviewOpportunities { Service = schedule.Name, Date = start.ToISO8601DateString(), Duration = schedule.DurationInMinutes.ToString() } );
                    }
                }
                return result.ToArray();
            }, new TimeSpan( 0, 0, cacheDelay ) );
        }

        // Next up is a filter to tell us if a particular schedule (by ScheduleId) is currently active.
        // This has to be done in C# because SQL doesn't know how to process calendar definitions to get schedules of events.
        public static bool? IsScheduleActive( object input )
        {
            if ( input == null || !( input is int ) )
            {
                return null;
            }

            var id = ( int ) input;

            var s = new ScheduleService( new RockContext() ).Get( id );
            if ( s != null )
            {
                return InetCalendarHelper.GetOccurrences( s.iCalendarContent, DateTime.Now ).Any();
            }
            return false;
        }

        #endregion
    }
}