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
    /// A Question
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_Question" )]
    [DataContract]
    public class Question : Rock.Data.Model<Question>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [DataMember]
        public int? ResourceId { get; set; }

        [DataMember]
        public int? LocationId { get; set; }

        [DataMember]
        public int Order { get; set; }      

        [DataMember]
        public string QuestionText { get; set; }

        [DataMember]
        public int AnswerFieldTypeId { get; set; }

        #endregion

        #region Virtual Properties

        public virtual Resource Resource { get; set; }

        public virtual Location Location { get; set; }

        public virtual FieldType AnswerFieldType { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class QuestionConfiguration : EntityTypeConfiguration<Question>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionConfiguration"/> class.
        /// </summary>
        public QuestionConfiguration()
        {
            this.HasOptional( r => r.Resource ).WithMany().HasForeignKey( r => r.ResourceId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.AnswerFieldType ).WithMany().HasForeignKey( r => r.AnswerFieldTypeId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "Question" );
        }
    }

    #endregion

}
