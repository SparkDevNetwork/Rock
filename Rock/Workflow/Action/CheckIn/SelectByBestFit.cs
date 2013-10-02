//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Assigns a grouptype, group, location and schedule from those available if one hasn't been previously selected
    /// </summary>
    [Description( "Selects the grouptype, group, location and schedule for each person based on their best fit." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Select By Best Fit" )]
    public class SelectByBestFit : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    foreach ( var person in family.People.Where( f => f.Selected ) )
                    {   
                        char[] delimiter = { ',' };                        
                        if ( person.GroupTypes.Any() )
                        {
                            CheckInGroupType groupType;
                            if ( person.GroupTypes.Count > 1 )
                            {
                                var gradeFilter = person.GroupTypes.Where( gt => gt.GroupType.Attributes.ContainsKey( "GradeRange" ) ).Select( g =>
                                        new
                                        {
                                            GroupType = g,
                                            GradeRange = g.GroupType.GetAttributeValue( "GradeRange" ).Split( delimiter, StringSplitOptions.None )
                                                .Select( av => av.AsType<int?>() )
                                        } ).ToList();

                                // use an aggregate function to find the grouptype with the closest grade
                                // #ToDO: Test the upper value of grade and age ranges
                                var groupTypeMatchGrade = gradeFilter.Aggregate( ( x, y ) => Math.Abs( Convert.ToDouble( x.GradeRange.First() - person.Person.Grade ) )
                                        < Math.Abs( Convert.ToDouble( y.GradeRange.First() - person.Person.Grade ) ) ? x : y )
                                            .GroupType;

                                var ageFilter = person.GroupTypes.Where( g => g.GroupType.Attributes.ContainsKey( "AgeRange" ) ).Select( g =>
                                        new
                                        {
                                            GroupType = g,
                                            AgeRange = g.GroupType.GetAttributeValue( "AgeRange" ).Split( delimiter, StringSplitOptions.None )
                                                .Select( av => av.AsType<double?>() )
                                        } ).ToList();

                                // use an aggregate function to find the grouptype with the closest age
                                var groupTypeMatchAge = ageFilter.Aggregate( ( x, y ) => Math.Abs( Convert.ToDouble( x.AgeRange.First() - person.Person.Age ) )
                                        < Math.Abs( Convert.ToDouble( y.AgeRange.First() - person.Person.Age ) ) ? x : y )
                                            .GroupType;

                                groupType = groupTypeMatchGrade ?? groupTypeMatchAge;
                            }
                            else 
                            {   // only one option is left
                                groupType = person.GroupTypes.FirstOrDefault();
                            }                            

                            if ( groupType != null && groupType.Groups.Count > 0 )
                            {
                                groupType.PreSelected = true;
                                groupType.Selected = true;
                                var group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
                                if ( group == null )
                                {
                                    //  check groups by grade
                                    var gradeGroups = groupType.Groups.Where( g => g.Group.Attributes.ContainsKey( "GradeRange" ) ).Select( g => 
                                        new {  
                                            Group = g, 
                                            GradeRange = g.Group.GetAttributeValue( "GradeRange" ).Split( delimiter, StringSplitOptions.None )
                                                .Select( av => av.AsType<int?>() )
                                        } ).ToList();
                                    
                                    var groupMatchGrade = gradeGroups.Aggregate( ( x, y ) => Math.Abs( Convert.ToDouble( x.GradeRange.First() - person.Person.Grade ) )
                                            < Math.Abs( Convert.ToDouble( y.GradeRange.First() - person.Person.Grade ) ) ? x : y )
                                                .Group;

                                    // check groups by age
                                    var ageGroups = groupType.Groups.Where( g => g.Group.Attributes.ContainsKey( "AgeRange" ) ).Select( g => 
                                        new {
                                            Group = g,
                                            AgeRange = g.Group.GetAttributeValue( "AgeRange" ).Split( delimiter, StringSplitOptions.None )
                                                .Select( av => av.AsType<double?>() )
                                        } ).ToList();
                                                                                                                                                
                                    var groupMatchAge = ageGroups.Aggregate( ( x, y ) => Math.Abs( Convert.ToDouble( x.AgeRange.First() - person.Person.Age ) )
                                            < Math.Abs( Convert.ToDouble( y.AgeRange.First() - person.Person.Age ) ) ? x : y )
                                                .Group;

                                    group = groupMatchGrade ?? groupMatchAge;
                                }                                

                                if ( group != null && group.Locations.Count > 0 )
                                {
                                    group.PreSelected = true;
                                    group.Selected = true;
                                    var location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
                                    if ( location == null )
                                    {
                                        // this works when a group is only meeting at one location per campus
                                        int primaryGroupLocationId = new GroupLocationService().Queryable().Where( gl => gl.GroupId == group.Group.Id )
                                            .Select( gl => gl.LocationId ).ToList().FirstOrDefault();
                                        location = group.Locations.Where( l => l.Location.Id == primaryGroupLocationId ).FirstOrDefault();
                                    }

                                    if ( location != null && location.Schedules.Count > 0 )
                                    {
                                        location.PreSelected = true;
                                        location.Selected = true;
                                        var schedule = location.Schedules.Where( s => s.Selected ).FirstOrDefault();
                                        if ( schedule == null )
                                        {
                                            schedule = location.Schedules.FirstOrDefault();
                                            schedule.PreSelected = true;
                                            schedule.Selected = true;
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}
