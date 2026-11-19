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

using System;

using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// Helper methods for loading outreach contacts and touchpoints that
    /// are owned by the current person.
    /// </summary>
    internal static class OutreachContactHelper
    {
        /// <summary>
        /// Gets the contact identified by the key if it is owned by the
        /// current person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="key">The contact identifier key, guid or integer identifier.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="allowIntegerIdentifier">If set to <c>true</c> then integer identifiers are allowed.</param>
        /// <returns>The <see cref="Contact"/> or <c>null</c> if it was not found or is not owned by the current person.</returns>
        public static Contact GetOwnedContact( RockContext rockContext, string key, Person currentPerson, bool allowIntegerIdentifier )
        {
            if ( currentPerson == null || key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var contact = new ContactService( rockContext ).Get( key, allowIntegerIdentifier );

            return IsOwnedBy( rockContext, contact, currentPerson ) ? contact : null;
        }

        /// <summary>
        /// Gets the contact identified by the integer identifier if it is
        /// owned by the current person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>The <see cref="Contact"/> or <c>null</c> if it was not found or is not owned by the current person.</returns>
        public static Contact GetOwnedContact( RockContext rockContext, int contactId, Person currentPerson )
        {
            if ( currentPerson == null )
            {
                return null;
            }

            var contact = new ContactService( rockContext ).Get( contactId );

            return IsOwnedBy( rockContext, contact, currentPerson ) ? contact : null;
        }

        /// <summary>
        /// Gets the touchpoint identified by the key if its contact is owned
        /// by the current person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="key">The touchpoint identifier key, guid or integer identifier.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="allowIntegerIdentifier">If set to <c>true</c> then integer identifiers are allowed.</param>
        /// <returns>The <see cref="ContactTouchpoint"/> or <c>null</c> if it was not found or its contact is not owned by the current person.</returns>
        public static ContactTouchpoint GetOwnedTouchpoint( RockContext rockContext, string key, Person currentPerson, bool allowIntegerIdentifier )
        {
            if ( currentPerson == null || key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var touchpoint = new ContactTouchpointService( rockContext ).Get( key, allowIntegerIdentifier );

            if ( touchpoint == null || GetOwnedContact( rockContext, touchpoint.ContactId, currentPerson ) == null )
            {
                return null;
            }

            return touchpoint;
        }

        /// <summary>
        /// Determines whether the contact is owned by the person. The owner
        /// is compared by person so that merged people still match.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contact">The contact.</param>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if the contact is owned by the person; otherwise, <c>false</c>.</returns>
        private static bool IsOwnedBy( RockContext rockContext, Contact contact, Person person )
        {
            if ( contact == null )
            {
                return false;
            }

            return new PersonAliasService( rockContext ).GetPersonId( contact.OwnerPersonAliasId ) == person.Id;
        }

        /// <summary>
        /// Gets the binary file identified by the guid if it was uploaded by
        /// the person, so it can be used as a contact photo.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="photoGuid">The binary file unique identifier.</param>
        /// <param name="person">The person.</param>
        /// <returns>The <see cref="BinaryFile"/> or <c>null</c> if it was not found or was not uploaded by the person.</returns>
        public static BinaryFile GetUploadedContactPhoto( RockContext rockContext, Guid photoGuid, Person person )
        {
            /*
                10/18/2026 - MSE

                Contact photos come from the client as a binary file guid. Without
                this check any signed-in person could attach any file in the system
                to their own contact, and the Contact Profile block would later
                delete it when the photo is changed. Mobile uploads are saved as
                permanent files, so IsTemporary cannot be used to tell a new upload
                apart from an existing file. Only files the person uploaded are
                accepted.

                Reason: Prevent attaching and deleting other people's files.
            */
            if ( person == null || photoGuid == Guid.Empty )
            {
                return null;
            }

            var photo = new BinaryFileService( rockContext ).Get( photoGuid );

            return IsCreatedBy( rockContext, photo, person ) ? photo : null;
        }

        /// <summary>
        /// Determines whether the binary file was uploaded by the person. The
        /// creator is compared by person so that merged people still match.
        /// Files with no recorded creator are not accepted.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="binaryFile">The binary file.</param>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if the binary file was uploaded by the person; otherwise, <c>false</c>.</returns>
        public static bool IsCreatedBy( RockContext rockContext, BinaryFile binaryFile, Person person )
        {
            if ( binaryFile?.CreatedByPersonAliasId == null || person == null )
            {
                return false;
            }

            return new PersonAliasService( rockContext ).GetPersonId( binaryFile.CreatedByPersonAliasId.Value ) == person.Id;
        }
    }
}
