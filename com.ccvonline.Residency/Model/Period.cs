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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using com.ccvonline.Residency.Data;

namespace com.ccvonline.Residency.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_Residency_Period" )]
    [DataContract]
    public partial class Period : NamedModel<Period>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        [Column(TypeName="Date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the residency tracks.
        /// </summary>
        /// <value>
        /// The residency tracks.
        /// </value>
        public virtual List<Track> Tracks { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class PeriodConfiguration : EntityTypeConfiguration<Period>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodConfiguration"/> class.
        /// </summary>
        public PeriodConfiguration()
        {
        }
    }
}
