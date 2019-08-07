// <copyright>
// Copyright by the Spark Development Network
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
using System.Linq;

using Rock.Attribute;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in
    /// </summary>
    [LinkedPage( "Multi-Person First Page (Family Check-in)", "The first page for each person during family check-in.", false, "", "", 5, "MultiPersonFirstPage" )]
    [LinkedPage( "Multi-Person Last Page  (Family Check-in)", "The last page for each person during family check-in.", false, "", "", 6, "MultiPersonLastPage" )]
    [LinkedPage( "Multi-Person Done Page (Family Check-in)", "The page to navigate to once all people have checked in during family check-in.", false, "", "", 7, "MultiPersonDonePage" )]
    public abstract class CheckInBlockMultiPerson : CheckInBlock
    {
        /// <summary>
        /// Processes result of current person not having any option
        /// </summary>
        protected virtual void ProcessNoOption()
        {
            if ( KioskCurrentlyActive && CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                {
                    var queryParams = CheckForOverride();

                    var schedule = person.CurrentSchedule;
                    if ( schedule != null )
                    {
                        schedule.Selected = false;
                    }

                    if ( !person.SelectedSchedules.Any() )
                    {
                        person.Selected = false;
                    }

                    var nextPerson = CurrentCheckInState.CheckIn.CurrentPerson;
                    if ( nextPerson != null && 
                        !string.IsNullOrWhiteSpace( GetAttributeValue( "MultiPersonFirstPage" ) ) )
                    {
                        var nextBlock = GetCheckInBlock( "MultiPersonFirstPage" );
                        if ( nextBlock != null && nextBlock.RequiresSelection( false ) )
                        {
                            NavigateToLinkedPage( "MultiPersonFirstPage", queryParams );
                        }
                    }
                    else
                    {
                        if ( CurrentCheckInState.CheckIn.CurrentFamily.GetPeople( true ).Any() &&
                            !string.IsNullOrWhiteSpace( GetAttributeValue( "MultiPersonDonePage" ) ) )
                        {
                            NavigateToLinkedPage( "MultiPersonDonePage", queryParams );
                        }
                        else
                        {
                            CancelCheckin();
                        }
                    }
                }
                else
                {
                    CancelCheckin();
                }
            }
            else
            {
                CancelCheckin();
            }
        }

    }
}