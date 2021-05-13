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
    [LinkedPage( "Multi-Person First Page (Family Check-in)",
        Key = AttributeKey.MultiPersonFirstPage,
        Description = "The first page for each person during family check-in.",
        IsRequired = false,
        Order = 5 )]

    [LinkedPage( "Multi-Person Last Page  (Family Check-in)",
        Key = AttributeKey.MultiPersonLastPage,
        Description = "The last page for each person during family check-in.",
        IsRequired = false,
        Order = 6 )]

    [LinkedPage( "Multi-Person Done Page (Family Check-in)",
        Key = AttributeKey.MultiPersonDonePage,
        Description = "The page to navigate to once all people have checked in during family check-in.",
        IsRequired = false,
        Order = 7 )]

    public abstract class CheckInBlockMultiPerson : CheckInBlock
    {
        /// <summary>
        /// The attribute keys that can be used by all blocks that inherit this one. In the child class create a new private AttributeKey sub class and point those const to this class.
        /// e.g.
        /// private new static class AttributeKey
        /// {
        ///     public const string Caption = "Caption";
        ///     public const string NextPage = CheckinBlock.AttributeKey.NextPage;
        ///     public const string MultiPersonFirstPage = CheckInBlockMultiPerson.AttributeKey.MultiPersonFirstPage;
        /// }
        /// </summary>
        protected new static class AttributeKey
        {
            /// <summary>
            /// The first page for each person during family check-in.
            /// </summary>
            public const string MultiPersonFirstPage = "MultiPersonFirstPage";

            /// <summary>
            /// The last page for each person during family check-in.
            /// </summary>
            public const string MultiPersonLastPage = "MultiPersonLastPage";

            /// <summary>
            /// The page to navigate to once all people have checked in during family check-in.
            /// </summary>
            public const string MultiPersonDonePage = "MultiPersonDonePage";
        }

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