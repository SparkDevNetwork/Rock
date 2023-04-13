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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a snippet type
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "SnippetType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "FD4C72DE-6B5D-4EB5-9438-385E2E15AF05" )]
    public class SnippetType : Model<SnippetType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [DataMember]
        public string HelpText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is personal allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is personal allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPersonalAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shared allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shared allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSharedAllowed { get; set; }

        #endregion Entity Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="string" /> that represents this snippet type.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this snippet type.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Snippet">Snippets</see> which are associated with the SnippetType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Snippet">Snippets</see> which are associated with the SnippetType.
        /// </value>
        [DataMember]
        public virtual ICollection<Snippet> Snippets { get; set; } = new Collection<Snippet>();

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class SnippetTypeConfiguration : EntityTypeConfiguration<SnippetType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetTypeConfiguration"/> class.
        /// </summary>
        public SnippetTypeConfiguration()
        {
        }
    }

    #endregion Entity Configuration
}
