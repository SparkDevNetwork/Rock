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

namespace Rock.Model
{
    /// <summary>
    /// Note Extension methods.
    /// </summary>
    public static partial class NoteExtensionMethods
    {
        /// <summary>
        /// Includes the <see cref="NoteType" /> and filters to those where
        /// the specified <paramref name="viewerPersonId"/> either created the note
        /// or is is permitted to view the note.
        /// </summary>
        /// <param name="notes">The notes queryable to apply the filtering to.</param>
        /// <param name="viewerPersonId">The identifier of the Person to evaluate for viewability.</param>
        /// <returns>
        /// The same queryable with an Include for <see cref="NoteType"/>
        /// and filtered to only those notes where the specified person
        /// created the note or the note is approved.
        /// </returns>
        public static IQueryable<Note> AreViewableBy( this IQueryable<Note> notes, int? viewerPersonId )
        {
            // The viewer is the creator of the note and the approval status is not denied.
            // OR the note is not private and is either approved or not required to be approved.
            if ( viewerPersonId.HasValue && viewerPersonId.Value > 0 )
            {
                return notes
                    .Where( n =>
                    ( ( viewerPersonId == n.CreatedByPersonAlias.PersonId && n.ApprovalStatus != NoteApprovalStatus.Denied )
                    || ( !n.IsPrivateNote && ( !n.NoteType.RequiresApprovals || n.ApprovalStatus == NoteApprovalStatus.Approved ) ) ) );
            }
            else
            {
                return notes
                    .Where( n =>
                    ( ( !n.IsPrivateNote && ( !n.NoteType.RequiresApprovals || n.ApprovalStatus == NoteApprovalStatus.Approved ) ) ) );
            }
        }

        /// <summary>
        /// Determines whether the specified <paramref name="viewerPersonId"/> either
        /// created the note or is permitted to view the note based on its private
        /// flag and approval status. This is the in-memory version of
        /// <see cref="AreViewableBy(IQueryable{Note}, int?)"/> and must be kept in
        /// step with it.
        /// </summary>
        /// <param name="note">The note to evaluate.</param>
        /// <param name="viewerPersonId">The identifier of the Person to evaluate for viewability.</param>
        /// <returns><c>true</c> if the note is viewable by the person; otherwise <c>false</c>.</returns>
        internal static bool IsViewableBy( this Note note, int? viewerPersonId )
        {
            // The note is not private and is either approved or not required to be approved.
            // This is checked first so the author is only loaded when needed.
            var requiresApprovals = Rock.Web.Cache.NoteTypeCache.Get( note.NoteTypeId )?.RequiresApprovals ?? false;

            if ( !note.IsPrivateNote && ( !requiresApprovals || note.ApprovalStatus == NoteApprovalStatus.Approved ) )
            {
                return true;
            }

            // The viewer is the creator of the note and the approval status is not denied.
            return viewerPersonId.HasValue
                && viewerPersonId.Value > 0
                && note.ApprovalStatus != NoteApprovalStatus.Denied
                && note.CreatedByPersonAlias != null
                && note.CreatedByPersonAlias.PersonId == viewerPersonId.Value;
        }

        /// <summary>
        /// Includes the <see cref="NoteType" /> and filters to those where
        /// the specified notification hasn't yet been sent,
        /// the note type allows watching,
        /// The last EditedDateTime is greater than <paramref name="since"/>,
        /// and the note is approved or doesn't require approvals.
        /// </summary>
        /// <param name="notes">The notes queryable to apply the filtering to.</param>
        /// <param name="since">The identifier of the Person to evaluate for viewability.</param>
        /// <returns>
        /// The same queryable with an Include for <see cref="NoteType"/>
        /// and filtered to only those notes that match the conditions for an unsent note watch.
        /// </returns>
        public static IQueryable<Note> HaveUnsentWatchNotifications( this IQueryable<Note> notes, DateTime since )
        {
            return notes
                .Where( n =>
                    !n.NotificationsSent
                    && n.NoteType.AllowsWatching
                    && n.EditedDateTime > since
                    && ( !n.NoteType.RequiresApprovals || n.ApprovalStatus == NoteApprovalStatus.Approved ) );
        }
    }
}
