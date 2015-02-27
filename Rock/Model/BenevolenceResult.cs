// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a result of a benevolence request that a person has submitted.
    /// </summary>
    [Table( "BenevolenceResult" )]
    [DataContract]
    public partial class BenevolenceResult : Model<BenevolenceResult>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the Benevolence Request the result is a result of.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the Id of the Benevolence Request the result is a result of.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int BenevolenceRequestId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value <see cref="Rock.Model.DefinedValue"/> representing the type of Benevolence Result.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the type of Benevolence Result.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [DefinedValue( SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE )]
        public int ResultTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the amount of benevolence
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the amount of benevolence.
        /// </value>
        [DataMember]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the text of the result details.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the details of the result.
        /// </value>
        [DataMember]
        public string ResultSummary { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Benevolence Request.
        /// </summary>
        /// <value>
        /// The Benevolence Request.
        /// </value>
        [DataMember]
        public virtual BenevolenceRequest BenevolenceRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the type of Benevolence Result.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the type of Benevolence Result.
        /// </value>
        [DataMember]
        public virtual DefinedValue ResultTypeValue { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// BenevolenceResult Configuration class.
    /// </summary>
    public partial class BenevolenceResultConfiguration : EntityTypeConfiguration<BenevolenceResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenevolenceResultConfiguration"/> class.
        /// </summary>
        public BenevolenceResultConfiguration()
        {
            this.HasRequired( p => p.BenevolenceRequest ).WithMany( p => p.BenevolenceResults ).HasForeignKey( p => p.BenevolenceRequestId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ResultTypeValue ).WithMany().HasForeignKey( p => p.ResultTypeValueId ).WillCascadeOnDelete( false );

        }
    }

    #endregion
}