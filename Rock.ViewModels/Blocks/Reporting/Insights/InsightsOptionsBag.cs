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

namespace Rock.ViewModels.Blocks.Reporting.Insights
{
    /// <summary>
    /// Options bag for the Insights block.
    /// </summary>
    public class InsightsOptionsBag
    {
        /// <summary>
        /// Passes the IsDemographicsShown block attribute to the front end.
        /// </summary>
        public bool IsDemographicsShown { get; set; }

        /// <summary>
        /// Passes the IsConnectionStatusShown block attribute to the front end.
        /// </summary>
        public bool IsConnectionStatusShown { get; set; }

        /// <summary>
        /// Passes the IsAgeShown block attribute to the front end.
        /// </summary>
        public bool IsAgeShown { get; set; }

        /// <summary>
        /// Passes the IsMaritalStatusShown block attribute to the front end.
        /// </summary>
        public bool IsMaritalStatusShown { get; set; }

        /// <summary>
        /// Passes the IsGenderShown block attribute to the front end.
        /// </summary>
        public bool IsGenderShown { get; set; }

        /// <summary>
        /// Passes the IsEthnicityShown block attribute to the front end.
        /// </summary>
        public bool IsEthnicityShown { get; set; }

        /// <summary>
        /// Passes the IsRaceShown block attribute to the front end.
        /// </summary>
        public bool IsRaceShown { get; set; }

        /// <summary>
        /// Passes the IsInformationStatisticsShown block attribute to the front end.
        /// </summary>
        public bool IsInformationStatisticsShown { get; set; }

        /// <summary>
        /// Passes the IsInformationCompletenessShown block attribute to the front end.
        /// </summary>
        public bool IsInformationCompletenessShown { get; set; }

        /// <summary>
        /// Passes the IsPercentageOfIndividualsWithAssessmentsShown block attribute to the front end.
        /// </summary>
        public bool IsPercentageOfIndividualsWithAssessmentsShown { get; set; }

        /// <summary>
        /// Passes the IsRecordStatusesShown block attribute to the front end.
        /// </summary>
        public bool IsRecordStatusesShown { get; set; }
    }
}
