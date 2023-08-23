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
using Rock.Data;
using Rock.Lava;
using Rock.Security;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;

namespace Rock.Model
{
    /// <summary>
    /// Note Logic
    /// </summary>
    public partial class Note
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the created by person photo URL.
        /// </summary>
        /// <value>
        /// The created by person photo URL.
        /// </value>
        [LavaVisible]
        public virtual string CreatedByPersonPhotoUrl
        {
            get
            {
                return Person.GetPersonPhotoUrl( this.CreatedByPersonAlias.Person );
            }
        }

        /// <summary>
        /// Gets the id to use in the note's anchor tag
        /// </summary>
        /// <value>
        /// The note anchor identifier.
        /// </value>
        [LavaVisible]
        public virtual string NoteAnchorId => $"NoteRef-{this.Guid.ToString( "N" )}";

        /// <summary>
        /// Gets the name of the person that last edited the note text. Use this instead of ModifiedByPersonName to determine the last person to edit the note text
        /// </summary>
        /// <value>
        /// The edited by person alias.
        /// </value>
        [LavaVisible]
        public virtual string EditedByPersonName
        {
            get
            {
                var editedByPerson = EditedByPersonAlias?.Person ?? CreatedByPersonAlias?.Person;
                return editedByPerson?.FullName;
            }
        }

        /// <summary>
        /// Gets the name of the entity (If it is a Note on a Person, it would be the person's name, etc)
        /// </summary>
        /// <value>
        /// The name of the entity.
        /// </value>
        [LavaVisible]
        public virtual string EntityName
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var noteTypeEntityTypeId = NoteTypeCache.Get( this.NoteTypeId )?.EntityTypeId;
                    if ( noteTypeEntityTypeId.HasValue && this.EntityId.HasValue )
                    {
                        var entity = new EntityTypeService( rockContext ).GetEntity( this.NoteType.EntityTypeId, this.EntityId.Value );
                        return entity?.ToString();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the approval URL.
        /// </summary>
        /// <value>
        /// The approval URL.
        /// </value>
        [LavaVisible]
        [Obsolete( "This property is no longer used and will be removed in the future." )]
        [RockObsolete( "1.16" )]
        public virtual string ApprovalUrl => string.Empty;

        #endregion Virtual Properties

        #region Security Overrides

        /// <summary>
        /// Gets the parent security authority of this Note. Where security is inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                var noteType = NoteTypeCache.Get( this.NoteTypeId );
                return noteType ?? base.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether the specified action is authorized on this note.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            /*
                SUMMARY OF NOTE SECURITY LOGIC

                Private      - Private notes are ONLY viewable by the creator. No one else no matter what their permissions are can see them.

                View         - You can view a note if you have View security to the note itself

                Edit         - Edit access gives you rights to add a new note
                               AND edit notes that you have authored
                               IMPORTANT - Edit does not give you rights to edit notes that someone else has authored

                Administrate - Allows you to edit notes, even those you did not create.
            */

            if ( this.IsPrivateNote )
            {
                if ( Id == 0 )
                {
                    // If we have not been created yet, use the default security
                    // from the parent authority since we don't have a created
                    // by person yet.
                    return base.IsAuthorized( action, person );
                }

                // If this is a private note, the creator has FULL access to it. Everybody else has NO access (including admins)
                if ( this.CreatedByPersonAlias?.PersonId == person?.Id )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ( action.Equals( Rock.Security.Authorization.VIEW, StringComparison.OrdinalIgnoreCase ) )
            {
                return base.IsAuthorized( Authorization.VIEW, person );
            }
            else if ( action.Equals( Rock.Security.Authorization.EDIT, StringComparison.OrdinalIgnoreCase ) )
            {
                // If this note was created by the logged person, they should be able to EDIT their own note,
                // otherwise EDIT (and DELETE) of other people's notes require ADMINISTRATE
                if ( CreatedByPersonAlias?.PersonId == person?.Id )
                {
                    return true;
                }
                else if ( Id == 0 )
                {
                    // If this is a new note being created, use the default
                    // EDIT permission which checks the NoteType.
                    return base.IsAuthorized( Authorization.EDIT, person );
                }
                else 
                {
                    return base.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, person );
                }
            }
            else
            {
                // If this note was created by the logged person, they should be able to do any action.
                if ( CreatedByPersonAlias?.PersonId == person?.Id )
                {
                    return true;
                }

                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// Determines whether the specified action is private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override bool IsPrivate( string action, Person person )
        {
            if ( CreatedByPersonAlias != null && person != null &&
                CreatedByPersonAlias.PersonId == person.Id &&
                IsPrivateNote )
            {
                return true;
            }

            return base.IsPrivate( action, person );
        }

        #endregion Security Overrides

        #region Public Methods

        /// <summary>
        /// Updates the caption of this note to be a value that matches the
        /// state of <see cref="IsPrivateNote"/>. This will only update the
        /// caption if it does not already have a custom value.
        /// </summary>
        public void UpdateCaption()
        {
            // Mark the note caption as either private or not unless it
            // already has an unknown value.
            var personalNoteCaption = "You - Personal Note";
            if ( Caption.IsNullOrWhiteSpace() && IsPrivateNote )
            {
                // Note is private so mark the caption as such.
                Caption = personalNoteCaption;
            }
            else if ( Caption == personalNoteCaption && !IsPrivateNote )
            {
                // Note was private but is no longer.
                Caption = string.Empty;
            }
            else if ( Caption == null )
            {
                Caption = string.Empty;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Text;
        }

        #endregion Public Methods
    }
}
