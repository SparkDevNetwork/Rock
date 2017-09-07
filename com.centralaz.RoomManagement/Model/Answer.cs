// <copyright>
// Copyright by the Central Christian Church
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

using Rock.Model;
namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Answer
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_Answer" )]
    [DataContract]
    public class Answer : Rock.Data.Model<Answer>, Rock.Data.IRockEntity
    {

        #region Entity Properties        

        [DataMember]
        public int QuestionId { get; set; }

        [DataMember]
        public int ReservationId { get; set; }

        [DataMember]
        public string Value { get; set; }

        #endregion

        #region Virtual Properties

        public virtual Question Question { get; set; }

        public virtual Reservation Reservation { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class AnswerConfiguration : EntityTypeConfiguration<Answer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerConfiguration"/> class.
        /// </summary>
        public AnswerConfiguration()
        {
            this.HasRequired( r => r.Question ).WithMany().HasForeignKey( r => r.QuestionId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Reservation ).WithMany().HasForeignKey( r => r.ReservationId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "Answer" );
        }
    }

    #endregion

}
