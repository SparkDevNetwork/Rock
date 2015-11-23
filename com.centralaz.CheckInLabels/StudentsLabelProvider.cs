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
    internal class StudentsLabelProvider : IPrintLabel
    {
        private RockContext rockContext = new RockContext();
        private StudentsLabelSet label;

        public StudentsLabelProvider()
        {

        }

        /// <summary>
        /// IPrintLabel implementation to print out name tags.
        /// </summary>
        void IPrintLabel.Print( CheckInLabel checkInLabel, CheckInPerson person, CheckInState checkInState, CheckInGroupType groupType )
        {
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
                    label.PrintLabel( printerAddress );
                }
            }

        }

        /// <summary>
        /// Intialize a person's label set with the information from the given person, occurrence(s),
        /// and attendance record.
        /// </summary>
        private void InitLabel( CheckInLabel checkInLabel, CheckInPerson attendee, CheckInState checkInState, CheckInGroupType groupType )
        {
            label = new StudentsLabelSet
            {
                FirstName = attendee.Person.NickName.Trim() != string.Empty ? attendee.Person.NickName : attendee.Person.FirstName,
                LastName = attendee.Person.LastName,
                FullName = string.Format( "{0} {1}", attendee.Person.NickName, attendee.Person.LastName ),
                FirstTime = attendee.FirstTime,
                LogoImageFile = checkInLabel.MergeFields["CentralAZ.LogoImageFile"],
            };
        }
    }
}
