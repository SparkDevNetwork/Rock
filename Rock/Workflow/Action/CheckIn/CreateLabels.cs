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
    /// Saves the selected check-in data as attendance
    /// </summary>
    [Description("Saves the selected check-in data as attendance")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Save Attendance" )]
    public class CreateLabels : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( action, out errorMessages );
            if ( checkInState != null )
            {
                using ( var uow = new Rock.Data.UnitOfWorkScope() )
                {
                    foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                    {
                        foreach ( var person in family.People.Where( p => p.Selected ) )
                        {
                            person.Labels = new List<CheckInLabel>();

                            CheckInLabel label = new CheckInLabel();
                            label.PrinterAddress = "255.255.255.255";
                            label.LabelFile = @"\\ccvfs1\30 Day Drop\Jon Edmiston\labeltest1.vpl";
                            label.MergeFields = new Dictionary<string, string>();
                            label.MergeFields.Add( "CF0", person.SecurityCode );
                            label.MergeFields.Add( "CF1", person.Person.FirstName );
                            label.MergeFields.Add( "CF2", person.Person.LastName );
                            person.Labels.Add( label );

                            label = new CheckInLabel();
                            label.PrinterAddress = "255.255.255.255";
                            label.LabelFile = @"\\ccvfs1\30 Day Drop\Jon Edmiston\labeltest2.vpl";
                            label.MergeFields = new Dictionary<string, string>();
                            label.MergeFields.Add( "CF0", person.SecurityCode );
                            label.MergeFields.Add( "CF1", person.Person.FirstName );
                            label.MergeFields.Add( "CF2", person.Person.LastName );
                            person.Labels.Add( label );

                            label = new CheckInLabel();
                            label.PrinterAddress = "255.255.255.255";
                            label.LabelFile = @"\\ccvfs1\30 Day Drop\Jon Edmiston\labeltest3.vpl";
                            label.MergeFields = new Dictionary<string, string>();
                            label.MergeFields.Add( "CF0", person.SecurityCode );
                            label.MergeFields.Add( "CF1", person.Person.FirstName );
                            label.MergeFields.Add( "CF2", person.Person.LastName );
                            person.Labels.Add( label );

                            foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                            {
                                foreach ( var location in groupType.Locations.Where( l => l.Selected ) )
                                {
                                    foreach ( var group in location.Groups.Where( g => g.Selected ) )
                                    {
                                        foreach ( var schedule in group.Schedules.Where( s => s.Selected ) )
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                SetCheckInState( action, checkInState );
                return true;

            }

            return false;
        }
    }
}