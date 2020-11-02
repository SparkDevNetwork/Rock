﻿// <copyright>
// Copyright by BEMA Software Services
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
namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Question
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_Question" )]
    [DataContract]
    public class Question : Rock.Data.Model<Question>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>
        /// The resource identifier.
        /// </value>
        [DataMember]
        public int? ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int AttributeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>
        /// The resource.
        /// </value>
        public virtual Resource Resource { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public virtual Rock.Model.Attribute Attribute { get; set; }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration for the Question model
    /// </summary>
    public partial class QuestionConfiguration : EntityTypeConfiguration<Question>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionConfiguration"/> class.
        /// </summary> 
        public QuestionConfiguration()
        {
            this.HasOptional( r => r.Resource ).WithMany().HasForeignKey( r => r.ResourceId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Attribute ).WithMany().HasForeignKey( r => r.AttributeId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "Question" );
        }
    }

    #endregion

}
