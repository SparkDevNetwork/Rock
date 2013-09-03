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

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Selects a group from those available if one hasn't been previously selected
    /// </summary>
    [Description( "Selects the grouptype for each person based on their best fit." )]
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
                        // if the person already has something selected, don't change it                        
                        char[] delimiter = { ',' };                        
                        if ( person.GroupTypes.Any() )
                        {
                            CheckInGroupType groupType;
                            if ( person.GroupTypes.Count > 1 )
                            {
                                var gradeFilter = person.GroupTypes.Where( g => g.GroupType.Attributes.ContainsKey( "GradeRange" ) )
                                    .Select( g => new
                                        {  
                                            GroupType = g, 
                                            GradeRange = g.GroupType.GetAttributeValue( "GradeRange" ).Split( delimiter, StringSplitOptions.None )
                                        } ).ToList();
                                groupType = gradeFilter.Where( g => ExtensionMethods.ParseNullable<int?>( g.GradeRange.First() ) <= person.Person.Grade
                                        && ExtensionMethods.ParseNullable<int?>( g.GradeRange.Last() ) >= person.Person.Grade )
                                        //.Aggregate( 
                                        .Select( g => g.GroupType ).FirstOrDefault();

                                var ageFilter = person.GroupTypes.Where( g => g.GroupType.Attributes.ContainsKey( "AgeRange" ) )
                                    .Select( g => new
                                        {  
                                            GroupType = g, 
                                            AgeRange = g.GroupType.GetAttributeValue( "AgeRange" ).Split( delimiter, StringSplitOptions.None )
                                        } ).ToList();
                                groupType = ageFilter.Where( g => ExtensionMethods.ParseNullable<int?>( g.AgeRange.First() ) <= person.Person.Age
                                        && ExtensionMethods.ParseNullable<int?>( g.AgeRange.Last() ) >= person.Person.Age )
                                        //.Aggregate( 
                                        .Select( g => g.GroupType ).FirstOrDefault();

                            }
                            else 
                            {   // only one option is left
                                groupType = person.GroupTypes.FirstOrDefault();
                            }
                            

                            if ( groupType != null && groupType.Groups.Count > 0 )
                            {
                                groupType.Selected = true;
                                var group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
                                if ( group == null )
                                {   
                                    // find the closest group by Age or Grade attribute
                                    var gradeGroups = groupType.Groups.Where( g => g.Group.Attributes.ContainsKey( "GradeRange" ) ).Select( g => new
                                        {  
                                            Group = g, 
                                            GradeRange = g.Group.GetAttributeValue( "GradeRange" ).Split( delimiter, StringSplitOptions.None )
                                        } ).ToList();
                                    var groupMatchGrade = gradeGroups.Where( g => ExtensionMethods.ParseNullable<int?>( g.GradeRange.First() ) <= person.Person.Grade
                                            && ExtensionMethods.ParseNullable<int?>( g.GradeRange.Last() ) >= person.Person.Grade )
                                            //.Aggregate( 
                                            .Select( g => g.Group ).FirstOrDefault();

                                    var ageGroups = groupType.Groups.Where( g => g.Group.Attributes.ContainsKey( "AgeRange" ) ).Select( g => new
                                        {
                                            Group = g,
                                            AgeRange = g.Group.GetAttributeValue( "AgeRange" ).Split( delimiter, StringSplitOptions.None )
                                        } ).ToList();
                                    var groupMatchAge = ageGroups.Where( g => ExtensionMethods.ParseNullable<int?>( g.AgeRange.First() ) <= person.Person.Age
                                            && ExtensionMethods.ParseNullable<int?>( g.AgeRange.Last() ) >= person.Person.Age )
                                            //.Aggregate(
                                            .Select( g => g.Group ).FirstOrDefault();

                                    group = groupMatchGrade ?? groupMatchAge;                                     
                                }
                                

                                if ( group != null && group.Locations.Count > 0 )
                                {
                                    group.Selected = true;
                                    var location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
                                    if ( location == null )
                                    {
                                        // this works when a group is only meeting at one location
                                        
                                        var groupLocations = new GroupLocationService
                                        //location = group.Locations.Where( l => group.Group.GroupLocations.Any( gl => gl.LocationId == l.Location.Id ) ).FirstOrDefault();                                        
                                    }

                                    if ( location != null && location.Schedules.Count > 0 )
                                    {
                                        location.Selected = true;
                                        var schedule = location.Schedules.Where( s => s.Selected ).FirstOrDefault();
                                        if ( schedule == null )
                                        {
                                            schedule = location.Schedules.FirstOrDefault();
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