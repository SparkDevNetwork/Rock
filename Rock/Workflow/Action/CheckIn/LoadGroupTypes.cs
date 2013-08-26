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

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Loads the group types allowed for each person in a family
    /// </summary>
    [Description("Loads the group types allowed for each person in a family")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Load Group Types" )]
    [BooleanField( "Filter Age", "By default all group types are loaded for members of the selected family.  Select this option to only load grouptypes for people who qualify by age.", false )]
    [BooleanField( "Filter Grade", "By default all group types are loaded for members of the selected family.  Select this option to only load grouptypes for people who qualify by grade.", false )]
    public class LoadGroupTypes : CheckInActionComponent
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
                bool filterAge = false;
                bool.TryParse( GetAttributeValue( action, "FilterAge" ), out filterAge );
                bool filterGrade = false;                
                bool.TryParse( GetAttributeValue( action, "FilterGrade" ), out filterGrade );

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People )
                    {
                        foreach ( var kioskGroupType in checkInState.Kiosk.FilteredGroupTypes( checkInState.ConfiguredGroupTypes ) )
                        {
                            if ( kioskGroupType.KioskGroups.SelectMany( g => g.KioskLocations ).Any( l => l.Location.IsActive ) )
                            {
                                char[] delimiter = { ',' };
                                bool personIsQualified = true;

                                if ( filterAge || filterGrade )
                                {
                                    double? personAge = person.Person.AgePrecise;
                                    string ageAttribute = kioskGroupType.GroupType.GetAttributeValue( "AgeRange" ) ?? string.Empty;
                                    int? personGrade = person.Person.Grade;                                    
                                    string gradeAttribute = kioskGroupType.GroupType.GetAttributeValue( "GradeRange" ) ?? string.Empty;

                                    if ( personIsQualified && !string.IsNullOrWhiteSpace( ageAttribute ) && personAge.HasValue )
                                    {
                                        List<double?> ageRange = ageAttribute.Split( delimiter, StringSplitOptions.None )
                                            .Select( s => ExtensionMethods.ParseNullable<double?>( s ) ).ToList();
                                        personIsQualified = ( personAge >= ageRange.First() && personAge <= ageRange.Last() );
                                    }

                                    if ( personIsQualified && !string.IsNullOrWhiteSpace( gradeAttribute ) && personGrade.HasValue )
                                    {
                                        List<int?> gradeRange = ageAttribute.Split( delimiter, StringSplitOptions.None )
                                            .Select( s => ExtensionMethods.ParseNullable<int?>( s ) ).ToList();
                                        personIsQualified = ( personGrade >= gradeRange.First() && personGrade <= gradeRange.Last() );
                                    }                                   
                                }
                                
                                if ( personIsQualified && !person.GroupTypes.Any( g => g.GroupType.Id == kioskGroupType.GroupType.Id ) )
                                {
                                    var checkinGroupType = new CheckInGroupType();
                                    checkinGroupType.GroupType = kioskGroupType.GroupType.Clone( false );
                                    checkinGroupType.GroupType.CopyAttributesFrom( kioskGroupType.GroupType );
                                    person.GroupTypes.Add( checkinGroupType );
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