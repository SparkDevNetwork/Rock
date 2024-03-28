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
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsSourceZipCode table for reporting on zip codes.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceZipCode" )]
    [DataContract]
    [HideFromReporting]
    [IncludeForModelMap]
    public partial class AnalyticsSourceZipCode
    {
        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>
        /// The zip code.
        /// </value>
        [DataMember]
        [Key, DatabaseGenerated( DatabaseGeneratedOption.None )]
        [MaxLength( 50 )]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the geographic parameter around the a ZipCode's GeoPoint. This can also be used to define a large area
        /// like a neighborhood.
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object representing the parameter of a location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography GeoFence { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember]
        [MaxLength( 2 )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the square miles.
        /// </summary>
        /// <value>
        /// The square miles.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal SquareMiles { get; set; }

        /// <summary>
        /// Gets or sets the households total.
        /// </summary>
        /// <value>
        /// The households total.
        /// </value>
        [DataMember]
        public int? HouseholdsTotal { get; set; }

        /// <summary>
        /// Gets or sets the families total.
        /// </summary>
        /// <value>
        /// The families total.
        /// </value>
        [DataMember]
        public int? FamiliesTotal { get; set; }

        /// <summary>
        /// Gets or sets the families under 10k percent.
        /// </summary>
        /// <value>
        /// The families under 10k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal FamiliesUnder10kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 10k to 14k percent.
        /// </summary>
        /// <value>
        /// The families 10k to 14k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families10kTo14kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 10k to 14k percent.
        /// </summary>
        /// <value>
        /// The families 10k to 14k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families15kTo24kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 25k to 34k percent.
        /// </summary>
        /// <value>
        /// The families 25k to 34k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families25kTo34kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 35k to 49k percent.
        /// </summary>
        /// <value>
        /// The families 35k to 49k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families35kTo49kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 50k to 74k percent.
        /// </summary>
        /// <value>
        /// The families 50k to 74k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families50kTo74kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 75k to 99k percent.
        /// </summary>
        /// <value>
        /// The families 75k to 99k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families75kTo99kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 100k to 149k percent.
        /// </summary>
        /// <value>
        /// The families 100k to 149k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families100kTo149kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families 150k to 199k percent.
        /// </summary>
        /// <value>
        /// The families 150k to 199k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal Families150kTo199kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families over 200k percent.
        /// </summary>
        /// <value>
        /// The families over 200k percent.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal FamiliesMore200kPercent { get; set; }

        /// <summary>
        /// Gets or sets the families median income.
        /// </summary>
        /// <value>
        /// The families median income.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal FamiliesMedianIncome { get; set; }

        /// <summary>
        /// Gets or sets the families median income margin of error.
        /// </summary>
        /// <value>
        /// The families median income margin of error.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal FamiliesMedianIncomeMarginOfError { get; set; }

        /// <summary>
        /// Gets or sets the families mean income.
        /// </summary>
        /// <value>
        /// The families mean income.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal FamiliesMeanIncome { get; set; }

        /// <summary>
        /// Gets or sets the families mean income margin of error.
        /// </summary>
        /// <value>
        /// The families mean income margin of error.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal FamiliesMeanIncomeMarginOfError { get; set; }

        /// <summary>
        /// Gets or sets the married couples total.
        /// </summary>
        /// <value>
        /// The married couples total.
        /// </value>
        [DataMember]
        public int? MarriedCouplesTotal { get; set; }

        /// <summary>
        /// Gets or sets the non family households total.
        /// </summary>
        /// <value>
        /// The non family households total.
        /// </value>
        [DataMember]
        public int? NonFamilyHouseholdsTotal { get; set; }

        /// <summary>
        /// Gets or sets the last update.
        /// </summary>
        /// <value>
        /// The last update.
        /// </value>
        [DataMember]
        public int? LastUpdate { get; set; }
    }
}
