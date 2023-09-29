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
using System;
using System.Linq;

using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.ViewModels.Rest.Controls;

namespace Rock
{
    /// <summary>
    /// Extension methods related to <see cref="MediaPlayerOptions"/>.
    /// </summary>
    internal static class PersonBasicEditorExtensions
    {
        /// <summary>
        /// Updates the options from a <see cref="MediaElement"/>. If one is
        /// not found then no changes are made.
        /// </summary>
        /// <param name="options">The options to be updated.</param>
        /// <param name="mediaElementId">The media element identifier.</param>
        /// <param name="mediaElementGuid">The media element unique identifier.</param>
        /// <param name="autoResumeInDays">The number of days back to look for an existing watch map to auto-resume from. Pass -1 to mean forever or 0 to disable.</param>
        /// <param name="combinePlayStatisticsInDays">The number of days back to look for an existing interaction to be updated. Pass -1 to mean forever or 0 to disable.</param>
        /// <param name="currentPerson">The person to use when searching for existing interactions.</param>
        /// <param name="personAliasId">If <paramref name="currentPerson"/> is <c>null</c> then this value will be used to optionally find an existing interaction.</param>
        internal static void updatePersonFromBag( this PersonBasicEditorBag bag, Person person )
        {
            bag.IfValidProperty( nameof( bag.PersonGender ), () => person.Gender = bag.PersonGender );
            //person.gender = bag.gender
        }

        internal static PersonBasicEditorBag GetPersonBasicEditorBagFromPerson( this Person person)
        {
            //
            return new PersonBasicEditorBag { };
        }
    }
}
