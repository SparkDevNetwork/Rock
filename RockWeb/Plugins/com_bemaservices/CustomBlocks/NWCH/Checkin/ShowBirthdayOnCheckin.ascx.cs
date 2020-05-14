// <copyright>
// Copyright by BEMA Information Technologies
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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Show Birthday on Checkin" )]
    [Category( "BEMA Services > Check-in" )]
    [Description( "Shows the birthday on checkin." )]
    public partial class ShowBirthdayOnCheckin : CheckInBlockMultiPerson
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        /// <param name="person">The person.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            var family = CurrentCheckInState.CheckIn.CurrentFamily;

            if ( person != null )
            {
                if ( person.Person.Age < 18 )
                {
                    lTitleAge.Text = string.Format( "<h1>{0} ({1} years old)</h1>", person.Person.FullName, person.Person.Age );
                }
                else
                {
                    lTitleAge.Text = string.Format( "<h1>{0}</h1>", person.Person.FullName );
                }
            }
            else if ( family != null )
            {
                lTitleAge.Text = string.Format( "<div id='family-id'>" );
                foreach ( var eachPerson in family.People )
                {
                    var monthDeci = 0.0;
                    int months = 0;
                    if ( eachPerson.Person.AgePrecise.HasValue && eachPerson.Person.Age.HasValue )
                    {
                        monthDeci = ( double ) ( eachPerson.Person.AgePrecise - eachPerson.Person.Age ) * 12;
                        months = ( int ) Math.Floor( monthDeci );
                    }
                    if ( eachPerson.Person.GradeFormatted.Contains( "Grade" ) )
                    {
                        lTitleAge.Text += string.Format( @"
						<div id='temp-with-age' style='display:none;'>{0}<br>({1})</div>",
                        eachPerson.Person.FullName, eachPerson.Person.GradeFormatted );
                    }
                    else if ( eachPerson.Person.Age < 18 )
                    {
                        lTitleAge.Text += string.Format( @"
						<div id='temp-with-age' style='display:none;'>{0}<br>({1} years {2} months)</div>",
                        eachPerson.Person.FullName, eachPerson.Person.Age, months );
                    }
                    else
                    {
                        lTitleAge.Text += string.Format( @"
						<div id='temp-with-age' style='display:none;'>{0}</div>",
                        eachPerson.Person.FullName );
                    }
                }
                lTitleAge.Text += string.Format( "</div>" );
            }
        }
    }

}
