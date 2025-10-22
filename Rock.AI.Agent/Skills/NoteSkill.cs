using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides functionality to create and manage notes within the AI agent.
    /// </summary>
    /// <remarks>
    /// This skill allows for operations such as adding, updating, deleting, and listing notes. It also provides tools to look up note types and retrieve specific notes.
    /// </remarks>
    [AgentSkillGuid( "216E5428-DE1A-4458-A22C-22812955264A" )]
    [EntityTypeGuid( "76DD142A-FB37-4B9E-A1F0-305A5B675B76" )]
    [Description( "This skill provides functionality to manage notes." )]
    internal sealed partial class NoteSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger instance for logging messages.
        /// </summary>
        private readonly ILogger<NoteSkill> _logger;

        /// <summary>
        /// The factory for creating RockContext instances.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteSkill"/> class.
        /// </summary>
        /// <param name="rockContextFactory">Factory to create rock contexts.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public NoteSkill( IRockContextFactory rockContextFactory, ILogger<NoteSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Shared Helpers

        /// <summary>
        /// Retrieves the result object for a specific note.
        /// </summary>
        /// <param name="note">The note object.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>A <see cref="NoteResult"/> object containing the note details.</returns>
        private static NoteResult GetNoteResult( Rock.Model.Note note, RockContext rockContext )
        {
            if ( note == null )
            {
                return null;
            }

            // This may not be populated since we could have just saved, so eager
            // load it if necessary.
            var noteType = NoteTypeCache.Get( note.NoteTypeId );
            string entityName = null;
            string entityTypeName = null;

            // Fetch the entity type and name
            if ( note.EntityId.HasValue && noteType.EntityTypeId.HasValue )
            {
                var entity = new EntityTypeService( rockContext ).GetEntity( noteType.EntityTypeId.Value, note.EntityId.Value );

                if ( entity != null )
                {
                    entityName = entity?.ToString();
                }
            }

            if ( noteType.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( noteType.EntityTypeId.Value );
                entityTypeName = entityType.FriendlyName;
            }

            return new NoteResult
            {
                Author = note.CreatedByPersonAlias != null
                    ? new PersonResult
                    {
                        Id = note.CreatedByPersonAlias.Person.Id,
                        NickName = note.CreatedByPersonAlias.Person.NickName,
                        LastName = note.CreatedByPersonAlias.Person.LastName
                    }
                    : null,
                NoteType = new NoteTypeResult
                {
                    EntityType = new KeyNameResult
                    {
                        Id = noteType.EntityTypeId,
                        Name = entityTypeName
                    },
                    Id = noteType.Id,
                    Name = noteType.Name
                },
                EntityName = entityName,
                Caption = note.Caption,
                IsAlert = note.IsAlert ?? false,
                Id = note.Id,
                IsPinned = note.IsPinned,
                IsPrivate = note.IsPrivateNote,
                Text = note.Text,
            };
        }

        #endregion
    }
}
