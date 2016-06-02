// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
