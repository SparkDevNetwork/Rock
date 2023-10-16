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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data model for the EntitySearch feature that uses dynamic LINQ statements
    /// to provide search capabilities for any entity type.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "EntitySearch" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ENTITY_SEARCH )]
    public partial class EntitySearch : Model<EntitySearch>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the search query.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that represents the name.
        /// </value>
        [Required( ErrorMessage = "Name is required" )]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/> that
        /// will be targeted by this search. This property is required.
        /// </summary>
        /// <value>An <see cref="int"/> representing the Id</value>
        [Required]
        [DataMember( IsRequired = true )]
        [EnableAttributeQualification]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the  key of this search. This is used to identify
        /// this search item through the API and Lava. This value must be
        /// unique for a given <see cref="EntityTypeId"/>. This property
        /// is required.
        /// </summary>
        /// <value>A <see cref="string"/> that represents the key.</value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the text that describes the purpose of this search.
        /// </summary>
        /// <value>A <see cref="string"/> that describes the search.</value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this search is active.
        /// </summary>
        /// <value><c>true</c> if this search is active; otherwise, <c>false</c>.</value>
        [DataMember( IsRequired = true )]
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the expression that will be used to filter the query.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Where()</c> expression.</value>
        [DataMember]
        public string WhereExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to group the results.
        /// This is processed after <see cref="WhereExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>GroupBy()</c> expression.</value>
        [DataMember]
        public string GroupByExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to define the structure
        /// of the resulting items. This is processed after <see cref="GroupByExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Select()</c> expression.</value>
        [DataMember]
        public string SelectExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to order the results.
        /// This is processed after <see cref="SelectExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>OrderBy()</c> expression.</value>
        [DataMember]
        public string OrderByExpression { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results per query. More data can
        /// be retrieved by subsequent queries that skip the first n items.
        /// </summary>
        /// <value>An optional <see cref="int"/> containing the maximum number of results per query.</value>
        [DataMember]
        public int? MaximumResultsPerQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this search will entity
        /// enforce entity security. Entity security has a pretty heafty
        /// performance hit and should only be used when it is actually needed.
        /// </summary>
        /// <value><c>true</c> if this search will enforce entity security; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsEntitySecurityEnforced { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the property paths to be included by Entity Framework.
        /// This is only valid when <see cref="IsEntitySecurityEnforced"/> is <c>true</c>.
        /// </para>
        /// <para>
        /// Example: <c>GroupType,Members.Person</c>
        /// </para>
        /// </summary>
        /// <value>The property paths to include as a comma seperated list.</value>
        [MaxLength( 200 )]
        [DataMember]
        public string IncludePaths { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether search query will allow
        /// custom refinement options in the form of an additional user query.
        /// </summary>
        /// <value><c>true</c> if this query allows refinement; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsRefinementAllowed { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Entity Type that will be queried by this search.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that will be queried by this search.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override bool IsValid
        {
            get
            {
                if ( !base.IsValid )
                {
                    return false;
                }

                if ( Regex.IsMatch( Key, "[^a-zA-Z0-9-]" ) )
                {
                    var validationResult = new ValidationResult( $"{nameof( Key )} may only contain letters, numbers and underscore.", new[] { nameof( Key ) } );
                    ValidationResults.Add( validationResult );

                    return false;
                }

                return true;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Entity Search Configuration class.
    /// </summary>
    public partial class EntitySearchConfiguration : EntityTypeConfiguration<EntitySearch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySearchConfiguration"/> class.
        /// </summary>
        public EntitySearchConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
