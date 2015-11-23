using System;
using System.Collections.Generic;
using System.Reflection;

using Rock;
using Rock.CheckIn;
//using Rock.Model;
using Rock.Web.Cache;

/// <summary>
/// A generic print label provider for use with the check-in system.
/// </summary>
namespace com.centralaz.CheckInLabels
{
    public interface IPrintLabel
    {
        //void Print(FamilyMember person, IEnumerable<Occurrence> occurrences, OccurrenceAttendance attendance, ComputerSystem kiosk);
        void Print( CheckInLabel checkInLabel, CheckInPerson person, CheckInState checkInState, CheckInGroupType checkInGroupType );
    }

    public static class PrintLabelHelper
    {
        public static IPrintLabel GetPrintLabelClass( string assemblyName, string assemblyClass )
        {
            Assembly assembly = Assembly.Load( assemblyName );

            if ( assembly == null )
            {
                return null;
            }

            Type type = assembly.GetType( assemblyClass ) ?? assembly.GetType( assemblyName + "." + assemblyClass );

            if ( type == null )
            {
                throw new Exception( string.Format( "Could not find '{0}' class in '{1}' assembly.", assemblyClass, assemblyName ) );
            }

            return (IPrintLabel)Activator.CreateInstance( type );
        }
    }
}
