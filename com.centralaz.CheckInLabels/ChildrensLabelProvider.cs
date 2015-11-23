/**********************************************************************
* Description:	Class that handles printing check-in labels for children
*               who check in to a class.
* Created By:   Jason Offutt @ Central Christian Church of the East Valley
* Date Created:	2009-01-06
*
* $Workfile: CccevPrintLabel.cs $
* $Revision: 19 $ 
* $Header: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CheckIn/Entity/CccevPrintLabel.cs   19   2013-11-13 11:24:35-07:00   nicka $
* 
* $Log: /trunk/Arena.Custom.Cccev/Arena.Custom.Cccev.CheckIn/Entity/CccevPrintLabel.cs $
*  
*  Revision: 19   Date: 2013-11-13 18:24:35Z   User: nicka 
*  Added new EpiPen flag/symbol that appears on attendance labels and nametags 
*  if the "Cccev.EpiPenReleaseAttributeID" org setting is tied to an attribute 
*  of type Document and a document is attached to the person. 
*  
*  Revision: 18   Date: 2013-04-29 22:37:35Z   User: nicka 
*  If both the Cccev.DisplayRoomNameOnNameTag and 
*  Cccev.UseTagNameInsteadOfRoomName organization settings are true, it will 
*  print the linked tag name (if any) in place of the room name on the 
*  name-tag label. 
*  
*  Revision: 17   Date: 2012-11-26 23:30:25Z   User: nicka 
*  Only try to print to kiosk (instead of location) if the kiosk has a printer 
*  (line 97). 
*  
*  Revision: 16   Date: 2010-01-19 23:17:48Z   User: JasonO 
*  
*  Revision: 15   Date: 2009-10-29 16:35:34Z   User: JasonO 
*  Adding enhanced support for at kiosk/at location printing via 
*  OccurrenceTypeReports. 
*  
*  Revision: 14   Date: 2009-10-27 17:16:46Z   User: JasonO 
*  
*  Revision: 13   Date: 2009-10-08 17:18:18Z   User: JasonO 
*  Merging/updating to make changes for 1.2 release. 
*  
*  Revision: 12   Date: 2009-09-16 15:32:00Z   User: JasonO 
*  
*  Revision: 11   Date: 2009-09-15 23:38:17Z   User: JasonO 
*  Implementing R# recommendations. 
*  
*  Revision: 10   Date: 2009-06-18 22:46:04Z   User: nicka 
*  
*  Revision: 9   Date: 2009-06-18 22:45:33Z   User: nicka 
*  DanielH|HDC patch 
*  
*  Revision: 8   Date: 2009-06-18 17:43:42Z   User: nicka 
*  Changes to handle new IPrintLabel that requires kiosk as discussed here: 
*  http://checkinwizard.codeplex.com/Thread/View.aspx?ThreadId=57675 
*  
*  Revision: 7   Date: 2009-03-05 15:36:26Z   User: nicka 
*  use nick name only if available else use first name 
*  
*  Revision: 6   Date: 2009-03-05 02:01:53Z   User: nicka 
*  Change label to use Nick name instead of First name 
*  
*  Revision: 5   Date: 2009-01-27 13:43:20Z   User: nicka 
*  change health notes to show until 1st grade 
*  
*  Revision: 4   Date: 2009-01-07 02:41:12Z   User: nicka 
*  Added logic to change Age Group based on age and or grade. Also only show 
*  Health Notes details if person under 2 years old. 
*  
*  Revision: 3   Date: 2009-01-07 00:21:01Z   User: JasonO 
*  
*  Revision: 2   Date: 2009-01-06 22:14:13Z   User: JasonO 
*  Sprint completion! 
*  
*  Revision: 1   Date: 2009-01-06 17:35:18Z   User: JasonO 
**********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.centralaz.CheckInLabels
{
    internal class ChildrensLabelProvider : IPrintLabel
    {
        private RockContext rockContext = new RockContext();
        private ChildrenLabelSet label;

        public ChildrensLabelProvider()
        {
        }

        /// <summary>
        /// IPrintLabel implementation to print out name tags.
        /// </summary>
        void IPrintLabel.Print( CheckInLabel checkInLabel, CheckInPerson person, CheckInState checkInState, CheckInGroupType groupType )
        {
            //location = new Location( occurrences.First().LocationID );
            //InitLabel( person, checkInState );

            //OccurrenceTypeReportCollection reports = new OccurrenceTypeReportCollection( occurrences.First().OccurrenceTypeID );
            //var report = reports.OfType<OccurrenceTypeReport>().FirstOrDefault();

            //if ( ( report != null && report.UseDefaultPrinter && kiosk.Printer != null ) ||
            //    location.Printer.PrinterName.Equals( "[Kiosk]", StringComparison.CurrentCultureIgnoreCase ) )
            //{
            //    label.PrintAllLabels( kiosk.Printer.PrinterName );
            //}
            //else
            //{
            //    label.PrintAllLabels( location.Printer.PrinterName );
            //}

            IEnumerable<CheckInLabel> printFromServer = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server );
            if ( printFromServer.Any() )
            {
                string printerAddress = string.Empty;

                foreach ( var label in printFromServer )
                {
                    var labelCache = KioskLabel.Read( label.FileGuid );
                    if ( labelCache != null )
                    {
                        if ( !string.IsNullOrWhiteSpace( label.PrinterAddress ) )
                        {
                            printerAddress = label.PrinterAddress;
                            break;
                        }
                    }
                }

                if ( !string.IsNullOrWhiteSpace( printerAddress ) )
                {
                    InitLabel( checkInLabel, person, checkInState, groupType );
                    label.PrintAllLabels( printerAddress );
                }
            }

        }

        /// <summary>
        /// Intialize a person's label set with the information from the given person, occurrence(s),
        /// and attendance record.
        /// </summary>
        private void InitLabel( CheckInLabel checkInLabel, CheckInPerson attendee, CheckInState checkInState, CheckInGroupType groupType )
        {
            CheckInLocation firstLocation = null;
            label = new ChildrenLabelSet
            {
                FirstName = attendee.Person.NickName.Trim() != string.Empty ? attendee.Person.NickName : attendee.Person.FirstName,
                LastName = attendee.Person.LastName,
                FullName = string.Format( "{0} {1}", attendee.Person.NickName, attendee.Person.LastName ),
                BirthdayDate = attendee.Person.BirthDate ?? DateTime.MinValue,
                SecurityToken = attendee.SecurityCode,
                CheckInDate = RockDateTime.Now
            };

            label.AttendanceLabelTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.AttendanceLabelTitle" ) ? checkInLabel.MergeFields["CentralAZ.AttendanceLabelTitle"] : string.Empty;
            label.BirthdayImageFile = checkInLabel.MergeFields.ContainsKey( "CentralAZ.BirthdayImageFile" ) ? checkInLabel.MergeFields["CentralAZ.BirthdayImageFile"] : string.Empty;
            label.Footer = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ClaimCardFooter" ) ? checkInLabel.MergeFields["CentralAZ.ClaimCardFooter"] : string.Empty;
            label.ClaimCardTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ClaimCardTitle" ) ? checkInLabel.MergeFields["CentralAZ.ClaimCardTitle"] : string.Empty;
            label.HealthNotesTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.HealthNotesTitle" ) ? checkInLabel.MergeFields["CentralAZ.HealthNotesTitle"] : string.Empty;
            label.LogoImageFile = checkInLabel.MergeFields.ContainsKey( "CentralAZ.LogoImageFile" ) ? checkInLabel.MergeFields["CentralAZ.LogoImageFile"] : string.Empty;
            label.ParentsInitialsTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ParentsInitialsTitle" ) ? checkInLabel.MergeFields["CentralAZ.ParentsInitialsTitle"] : string.Empty;
            label.ServicesTitle = checkInLabel.MergeFields.ContainsKey( "CentralAZ.ServicesLabel" ) ? checkInLabel.MergeFields["CentralAZ.ServicesLabel"] : string.Empty;

            // Get start times from any selected schedules...
            // This section is only needed because we have a weird "Transfer: " chunk
            // on the label that lists all the services the person is checked into.
            StringBuilder services = new StringBuilder();
            foreach ( var group in groupType.Groups.Where( g => g.Selected ) )
            {
                foreach ( var location in group.Locations.Where( l => l.Selected ).OrderBy( e => e.Schedules.Min( s => s.StartTime ) ) )
                {
                    // Put the first location's name on the label
                    if ( firstLocation == null )
                    {
                        firstLocation = location;
                        label.RoomName = firstLocation.Location.Name;
                    }

                    foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
                    {
                        if ( services.Length > 0 )
                        {
                            services.Append( ", " );
                        }
                        services.Append( schedule.StartTime.Value.ToShortTimeString() );
                    }
                }
            }

            label.Services = services.ToString();
            SetAgeGroup( attendee.Person );
            SetLabelFlags( attendee.Person );
        }

        /// <summary>
        /// This method will set the label's flags and health notes for the given person.
        /// </summary>
        /// <param name="attendee"></param>
        private void SetLabelFlags( Person attendee )
        {
            attendee.LoadAttributes();

            var selfCheckOut = attendee.GetAttributeValue( "CentralAZ.SelfCheckOut" ).AsBoolean();
            var legalNotes = attendee.GetAttributeValue( "LegalNotes" );
            var healthNotes = attendee.GetAttributeValue( "CentralAZ.HealthNotes" );
            var epiPenRelease = attendee.GetAttributeValue( "CentralAZ.EpiPenRelease" ).AsBoolean();

            label.EpiPenFlag = epiPenRelease;
            label.SelfCheckOutFlag = attendee.GetAttributeValue( "CentralAZ.SelfCheckOut" ).AsBoolean();
            label.LegalNoteFlag = ( !string.IsNullOrWhiteSpace( legalNotes ) );

            if ( !string.IsNullOrWhiteSpace( healthNotes ) )
            {
                label.HealthNoteFlag = true;
                // This was removed after speaking with Laurie (NA 1/26/2009)
                // Don't print health notes if child greater than 1st grade.
                //if ( !( attendee.GraduationDate > DateTime.Parse( "1/1/1900" )
                //    && Person.CalculateGradeLevel( attendee.GraduationDate, organization.GradePromotionDate ) >= 1 ) )
                //{
                label.HealthNotes = healthNotes;
                //}
            }
            else
            {
                label.HealthNoteFlag = false;
            }
        }

        /// <summary>
        /// This sets the label's "Age Group" value to a specific text (as per Children's
        /// ministries) based on the person's age and or grade.
        /// Basically:
        ///		* if the person is under the age of 2, display age in months;
        ///     * if they are in grade school, display the grade
        ///		* otherwise display age in years.
        /// </summary>
        /// <param name="person">The person whose label is being created.</param>
        private void SetAgeGroup( Person person )
        {
            if ( person.Age.HasValue && person.Age < 2 )
            {
                label.AgeGroup = String.Format( "{0} months", GetAgeInMonths( person.BirthDate.Value ) );
            }
            else
            {
                label.AgeGroup = ( !string.IsNullOrEmpty( person.GradeFormatted ) ) ? person.GradeFormatted : String.Format( "{0} year olds", person.Age );
            }
        }

        /// <summary>
        /// Calculates the age in whole months for the given birthday.
        /// </summary>
        /// <param name="birthday">A birthdate</param>
        /// <returns>number of whole months</returns>
        private static int GetAgeInMonths( DateTime birthday )
        {
            return GetDeltaInMonths( birthday, RockDateTime.Now );
        }

        /// <summary>
        /// An algorthim to determine the number of whole months
        /// between two dates (by Greg Golden).
        /// </summary>
        /// <param name="then">A date.</param>
        /// <param name="now">Another date.</param>
        /// <returns>number of whole months between the two dates</returns>
        private static int GetDeltaInMonths( DateTime then, DateTime now )
        {
            int thenMonthsSince1900 = 12 * ( then.Year - 1900 ) + then.Month;
            int nowMonthsSince1900 = 12 * ( now.Year - 1900 ) + now.Month;

            int monthsOld = nowMonthsSince1900 - thenMonthsSince1900;

            if ( now.Day < then.Day )
            {
                monthsOld--;
            }

            return monthsOld;
        }
    }
}
