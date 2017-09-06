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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// This can be used to keep all the bookmark saved by <see cref="Rock.Model.Person"/>
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "PersonBookmark" )]
    [DataContract]
    public class PersonBookmark : Model<PersonBookmark>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the friendly Name of the bookmark. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the bookmark.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of the bookmark. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the bookmark.
        /// </value>
        [Required]
        [MaxLength( 2083 )]
        [DataMember( IsRequired = true )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this Person Bookmark belongs to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that the Person Bookmark belongs to. 
        /// If the Person Bookmark does not belong to a category, this value will be null.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this Person Bookmark belongs to.
        /// </summary>
        /// <value>
        /// Teh <see cref="Rock.Model.Category"/> that this Person Bookmark belongs to.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( ( PersonAlias != null && person != null &&
                PersonAlias.PersonId == person.Id ) || PersonAlias == null )
            {
                return true;
            }

            return false;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class PersonBookmarkConfiguration : EntityTypeConfiguration<PersonBookmark>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBookmarkConfiguration"/> class.
        /// </summary>
        public PersonBookmarkConfiguration()
        {
            this.HasOptional( b => b.PersonAlias ).WithMany().HasForeignKey( b => b.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( b => b.Category ).WithMany().HasForeignKey( b => b.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
