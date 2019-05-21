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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Contains a list of previous LastNames that this person has had
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonPreviousName" )]
    [DataContract]
    public class PersonPreviousName : Model<PersonPreviousName>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [Previewable]
        [Index( "IDX_LastName", IsUnique = false )]
        public string LastName { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return LastName;
        }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        private History.HistoryChangeList HistoryChanges { get; set; }

        /// <summary>
        /// Gets or sets the history person identifier.
        /// </summary>
        /// <value>
        /// The history person identifier.
        /// </value>
        [NotMapped]
        private int? HistoryPersonId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;

            HistoryPersonId = PersonAlias != null ? PersonAlias.PersonId : (int?)null;
            if ( !HistoryPersonId.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Get( PersonAliasId );
                if ( personAlias != null )
                {
                    HistoryPersonId = personAlias.PersonId;
                }
            }

            if ( HistoryPersonId.HasValue )
            {
                HistoryChanges = new History.HistoryChangeList();

                switch ( entry.State )
                {
                    case EntityState.Added:
                        {
                            History.EvaluateChange( HistoryChanges, "Previous Name", string.Empty, LastName );
                            break;
                        }

                    case EntityState.Modified:
                        {
                            History.EvaluateChange( HistoryChanges, "Previous Name", entry.OriginalValues["LastName"].ToStringSafe(), LastName );
                            break;
                        }

                    case EntityState.Deleted:
                        {
                            History.EvaluateChange( HistoryChanges, "Previous Name", entry.OriginalValues["LastName"].ToStringSafe(), string.Empty );
                            return;
                        }
                }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( HistoryChanges != null && HistoryChanges.Any() && HistoryPersonId.HasValue )
            {
                HistoryService.SaveChanges( (RockContext)dbContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), HistoryPersonId.Value, HistoryChanges, true, this.ModifiedByPersonAliasId );
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Person Configuration class.
    /// </summary>
    public partial class PersonPreviousNameConfiguration : EntityTypeConfiguration<PersonPreviousName>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDuplicateConfiguration"/> class.
        /// </summary>
        public PersonPreviousNameConfiguration()
        {
            this.HasRequired( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
