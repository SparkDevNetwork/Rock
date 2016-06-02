// <copyright>
// Copyright by Central Christian Church
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using com.centralaz.DpsMatch.Data;
using Rock;
using Rock.Data;
using Rock.Model;
namespace com.centralaz.DpsMatch.Model
{
    /// <summary>
    /// A Match
    /// </summary>
    [Table( "_com_centralaz_DpsMatch_Match" )]
    [DataContract]
    public class Match : Rock.Data.Model<Match>
    {

        #region Entity Properties

        /// <summary>
        /// The PersonAlias of the potential Person Match
        /// </summary>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// The OffenderId of the potential Offender Match
        /// </summary>
        [DataMember]
        public int OffenderId { get; set; }

        /// <summary>
        /// The likelyhood of the match
        /// </summary>
        [DataMember]
        public int MatchPercentage { get; set; }

        /// <summary>
        /// Whether the potential match is confirmed as not a match
        /// </summary>
        [DataMember]
        public bool? IsMatch { get; set; }

        /// <summary>
        /// The date the potential match was confirmed as true or false
        /// </summary>
        [DataMember]
        public DateTime? VerifiedDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// The personalias of the potential match
        /// </summary>
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// The sexual offender of the potential match
        /// </summary>
        public virtual Offender Offender { get; set; }

        #endregion
    }

    #region Entity Configuration


    public partial class MatchConfiguration : EntityTypeConfiguration<Match>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchConfiguration"/> class.
        /// </summary>
        public MatchConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Offender ).WithMany().HasForeignKey( r => r.OffenderId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
