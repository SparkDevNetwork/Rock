using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Data;
using System.Runtime.Serialization;
using System.Reflection;

namespace com.reallifeministries.GroupMatching
{
    [DataContract]
    public class GroupMatcher : Rock.Lava.ILiquidizable
    {
        private double metersInMile = 1609.344;
        public int numMatches = 3;

        [DataMember]
        public List<DayOfWeek> daysOfWeek;
        [DataMember]
        public Person person;
        [DataMember]
        public Location personLocation;
        [DataMember]
        public GroupType groupType;

        [LavaIgnore]
        public GroupMatcher(Person pers, GroupType gt, List<DayOfWeek> days)
        {
            person = pers;
            personLocation = pers.GetHomeLocation();
            daysOfWeek = days;
            groupType = gt;
        }
        
        [LavaIgnore]
        public List<GroupMatch> GetMatches()
        {
            var matches = new List<GroupMatch>();
            var ctx = new RockContext();
            
               matches = (
                    from gl in ctx.GroupLocations
                    let distance = gl.Location.GeoPoint.Distance(personLocation.GeoPoint)
                    let memberCount = gl.Group.Members.Where(m => 
                        m.GroupMemberStatus == GroupMemberStatus.Active
                        ).Select(m => m.PersonId).Distinct().Count()
                    where gl.Group.Schedule.WeeklyDayOfWeek != null
                    where  daysOfWeek.Contains( (DayOfWeek)gl.Group.Schedule.WeeklyDayOfWeek )
                    where gl.Group.GroupTypeId == groupType.Id
                    where gl.Location.GeoPoint != null
                    where gl.Group.IsActive
                    orderby distance
                    select new GroupMatch {
                        Group = gl.Group,
                        Distance = distance / metersInMile,
                        MemberCount = memberCount,
                        Location = gl.Location,
                        Schedule = gl.Group.Schedule
                    }
                   ).Take(numMatches).ToList();
              
            return matches;
        }

        #region iLiquidizable

        [LavaIgnore]
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debuging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaIgnore]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string>();

                foreach (var propInfo in GetType().GetProperties())
                {
                    if (propInfo != null && LiquidizableProperty( propInfo ))
                    {
                        availableKeys.Add( propInfo.Name );
                    }
                }

                return availableKeys;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaIgnore]
        public virtual object this[object key]
        {
            get
            {
                var propInfo = GetType().GetProperty( key.ToString() );
                if (propInfo != null && LiquidizableProperty( propInfo ))
                {
                    try
                    {
                        object propValue = propInfo.GetValue( this, null );
                        if (propValue is Guid)
                        {
                            return ((Guid)propValue).ToString();
                        }
                        else
                        {
                            return propValue;
                        }
                    }
                    catch { }
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            var propInfo = GetType().GetProperty( key.ToString() );
            if (propInfo != null && LiquidizableProperty( propInfo ))
            {
                return true;
            }

            return false;
        }

        private bool LiquidizableProperty( PropertyInfo propInfo )
        {
            // If property has a [LavaIgnore] attribute return false
            if (propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() > 0)
            {
                return false;
            }

            // If property has a [DataMember] attribute return true
            if (propInfo.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0)
            {
                return true;
            }

            // If property has a [LavaInclude] attribute return true
            if (propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIncludeAttribute ) ).Count() > 0)
            {
                return true;
            }

            // otherwise return false
            return false;

        }
        #endregion
    }
}
