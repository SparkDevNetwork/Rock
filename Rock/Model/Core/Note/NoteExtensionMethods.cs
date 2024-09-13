using System;
using System.Data.Entity;
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
        public static IQueryable<Note> AreViewableBy( this IQueryable<Note> notes, int viewerPersonId )
        {
            // The viewer is the creator of the note and the approval status is not denied.
            // OR the note is not private and is either approved or not required to be approved.
            return notes
                .Where( n =>
                ( ( viewerPersonId == n.CreatedByPersonAlias.PersonId && n.ApprovalStatus != NoteApprovalStatus.Denied )
                || ( !n.IsPrivateNote && ( !n.NoteType.RequiresApprovals || n.ApprovalStatus == NoteApprovalStatus.Approved ) ) ) );
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
