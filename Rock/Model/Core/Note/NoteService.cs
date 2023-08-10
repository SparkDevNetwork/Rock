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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using AngleSharp.Dom;

using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Note"/> entity objects.
    /// </summary>
    public partial class NoteService : Service<Note>
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Note">Notes</see> for the specified <see cref="Rock.Model.NoteType"/> and entity.
        /// </summary>
        /// <param name="noteTypeId">A <see cref="System.Int32" /> representing the Id of the <see cref="Rock.Model.NoteType"/>.</param>
        /// <param name="entityId">TA <see cref="System.Int32"/> representing the Id of the entity that the note belongs to.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Note">Notes</see> for the specified <see cref="Rock.Model.NoteType"/> and entity. </returns>
        public IQueryable<Note> Get( int noteTypeId, int entityId )
        {
            return Queryable( "CreatedByPersonAlias.Person" )
                .Where( n =>
                    n.NoteTypeId == noteTypeId &&
                    n.EntityId == entityId )
                .OrderByDescending( n => n.IsAlert )
                .ThenByDescending( n => n.CreatedDateTime );
        }

        /// <summary>
        /// Returns all of the <see cref="Rock.Model.Note">Notes</see> for the specified note type.
        /// </summary>
        /// <param name="noteTypeId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.NoteType"/>.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Note">Notes</see> by <see cref="Rock.Model.NoteType"/>.</returns>
        public IQueryable<Note> GetByNoteTypeId( int noteTypeId )
        {
            return Queryable( "CreatedByPersonAlias.Person" )
                .Where( n =>
                    n.NoteTypeId == noteTypeId )
                .OrderByDescending( n => n.IsAlert )
                .ThenByDescending( n => n.CreatedDateTime );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Note">Notes</see> that are descendants (replies) of a specified note.
        /// </summary>
        /// <param name="parentNoteId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Note"/> to retrieve descendants for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Note">Notes</see> that are descendants of referenced note.</returns>
        public IEnumerable<Note> GetAllDescendents( int parentNoteId )
        {
            return this.ExecuteQuery(
                @"
                with CTE as (
                select * from [Note] where [ParentNoteId]={0}
                union all
                select [a].* from [Note] [a]
                inner join CTE pcte on pcte.Id = [a].[ParentNoteId]
                )
                select * from CTE
                ", parentNoteId );
        }

        /// <summary>
        /// Gets the reply depth of the specified note identifier. If this note
        /// is not a reply then <c>0</c> will be returned. If it is the first level of
        /// replies then <c>1</c> will be returned, and so on.
        /// </summary>
        /// <param name="noteId">The note whose depth is to be determined.</param>
        /// <returns>The reply depth of the note.</returns>
        public int GetReplyDepth( int noteId )
        {
            return Context.Database
                .SqlQuery<int>( @"
WITH [NoteCte] ([Id], [ParentNoteId], [Depth])
AS
(
    SELECT [Id], [ParentNoteId], 0 AS [Depth]
    FROM [Note]
    WHERE [Id] = @NoteId
 
    UNION ALL
 
    SELECT [P].[Id], [P].[ParentNoteId], Depth + 1
    FROM [Note] AS [P]
    INNER JOIN [NoteCte] AS [C] ON [C].[ParentNoteId] = [P].[Id]
)
SELECT MAX([Depth])
FROM [NoteCte];
", new SqlParameter( "NoteId", noteId ) )
                .FirstOrDefault();
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="deleteChildNotes">if set to <c>true</c> [delete child notes].</param>
        /// <returns></returns>
        public bool Delete( Note item, bool deleteChildNotes )
        {
            if ( deleteChildNotes )
            {
                var childNotes = GetAllDescendents( item.Id );
                if ( childNotes.Any() )
                {
                    base.DeleteRange( childNotes );
                }
            }

            return base.Delete( item );
        }

        /// <summary>
        /// Determines whether this instance [can delete child notes] the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can delete child notes] the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDeleteChildNotes( Note item, Person currentPerson, out string errorMessage )
        {
            errorMessage = string.Empty;

            var childNotes = this.GetAllDescendents( item.Id );
            if ( childNotes.Any() )
            {
                foreach ( var childNote in childNotes )
                {
                    if ( !childNote.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) )
                    {
                        errorMessage = "This note contains one or more note replies that cannot be deleted.";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the note can have any replies made to it.
        /// </summary>
        /// <param name="note">The note will be replied to.</param>
        /// <param name="errorMessage">On <c>false </c>return will contain the error message.</param>
        /// <returns><c>true</c> if the note can have replies; otherwise <c>false</c>.</returns>
        public bool CanReplyToNote( Note note, out string errorMessage )
        {
            var noteType = NoteTypeCache.Get( note.NoteTypeId );

            if ( !noteType.AllowsReplies )
            {
                errorMessage = "Note type does not allow replies.";

                return false;
            }

            if ( noteType.MaxReplyDepth.HasValue )
            {
                var depth = GetReplyDepth( note.Id );

                if ( depth >= noteType.MaxReplyDepth.Value )
                {
                    errorMessage = "Note type does not allow replies this deep.";

                    return false;
                }
            }

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Gets any mention identifier keys found in the note content.
        /// </summary>
        /// <param name="content">The content to be searched.</param>
        /// <returns>A list of <see cref="PersonAlias"/> <see cref="IEntity.IdKey"/> values.</returns>
        public static List<string> GetMentionIdKeysFromContent( string content )
        {
            if ( string.IsNullOrEmpty( content ) )
            {
                return new List<string>();
            }

            var mentions = new List<string>();

            var parseOptions = new AngleSharp.Html.Parser.HtmlParserOptions();
            var parser = new AngleSharp.Html.Parser.HtmlParser( parseOptions );
            var dom = parser.ParseDocument( string.Empty );
            var nodes = parser.ParseFragment( content, dom.Body );

            foreach ( var node in nodes )
            {
                if ( !( node is IElement element ) )
                {
                    continue;
                }

                var isMention = element.TagName.ToLower() == "span"
                    && element.HasAttribute( "class" )
                    && element.HasAttribute( "data-identifier" )
                    && element.GetAttribute( "class" ).ToLower() == "mention";

                if ( isMention )
                {
                    mentions.Add( element.GetAttribute( "data-identifier" ) );
                }
            }

            return mentions;
        }

        /// <summary>
        /// Gets the person identifiers that are mentioned in <paramref name="content"/>
        /// but were not mentioned in <paramref name="oldContent"/>.
        /// </summary>
        /// <param name="content">The new content to be searched.</param>
        /// <param name="oldContent">The old content to be searched.</param>
        /// <returns>A list of <see cref="Person"/> <see cref="IEntity.Id"/> values.</returns>
        public List<int> GetNewPersonIdsMentionedInContent( string content, string oldContent )
        {
            var oldMentionIdKeys = GetMentionIdKeysFromContent( oldContent );
            var newMentionIdKeys = GetMentionIdKeysFromContent( content );

            // Remove any id keys we already knew about.
            newMentionIdKeys = newMentionIdKeys.Except( oldMentionIdKeys ).ToList();

            // If we don't have anything new then we are done.
            if ( !newMentionIdKeys.Any() )
            {
                return new List<int>();
            }

            // At this point we have some possibly new mentions. Because these
            // are PersonAlias IdKey values, we might have a new mention for
            // a person that was already mentioned with a different PersonAlias.

            // Get the person alias identifiers from the mention keys.
            var oldMentionPersonAliasIds = oldMentionIdKeys
                .Select( k => IdHasher.Instance.GetId( k ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var newMentionPersonAliasIds = newMentionIdKeys
                .Select( k => IdHasher.Instance.GetId( k ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            // Get all the unique person alias ids between the old and new content.
            var allMentionPersonAliasIds = oldMentionPersonAliasIds
                .Union( newMentionPersonAliasIds )
                .Distinct()
                .ToList();

            // Get all the person identifiers for any of these aliases.
            var personIds = new PersonAliasService( ( RockContext ) Context )
                .Queryable()
                .Where( pa => allMentionPersonAliasIds.Contains( pa.Id ) )
                .Select( pa => new
                {
                    PersonAliasId = pa.Id,
                    pa.PersonId
                } )
                .ToList();

            // Get the person identifiers that were mentioned in the old content.
            var oldMentionPersonIds = personIds
                .Where( p => oldMentionPersonAliasIds.Contains( p.PersonAliasId ) )
                .Select( p => p.PersonId )
                .ToList();

            // Get the person identifiers that were mentioned in the new content.
            var newMentionPersonIds = personIds
                .Where( p => newMentionPersonAliasIds.Contains( p.PersonAliasId ) )
                .Select( p => p.PersonId )
                .ToList();

            // Get the new person identifiers that were not previously mentioned.
            return newMentionPersonIds
                .Except( oldMentionPersonIds )
                .ToList();
        }
    }
}
