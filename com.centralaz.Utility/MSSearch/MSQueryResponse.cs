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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.centralaz.Utility.MSSearch
{
    /// <summary>
    /// The MOSS search query response
    /// </summary>
    public class MSQueryResponse
    {
        private int _startAt = 0;
        private int _count = 0;
        private int _totalAvailable = 0;
        private bool _success = false;
        private string _xmlQueryRequest = string.Empty;
        private string _xmlQueryResponse = string.Empty;
        private string _xmlRegistrationResponse = string.Empty;
        private DataTable _dataTableQueryRelevantResults = null;
        private DataTable _dataTableQuerySpecialTermResults = null;
        private DataTable _dataTableQueryHighConfidenceResults = null;

        /// <summary>
        /// Specifies which result is the initial result in the response
        /// </summary>
        public int StartAt
        {
            get { return _startAt; }
            set { _startAt = value; }
        }

        /// <summary>
        /// Contains number of results included in the response
        /// </summary>
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        /// <summary>
        /// Contains the total number of relevant results returned in the response
        /// </summary>
        public int TotalAvailable
        {
            get { return _totalAvailable; }
            set { _totalAvailable = value; }
        }

        /// <summary>
        /// Indicates whether the search was successfull
        /// </summary>
        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }

        /// <summary>
        /// The XML document returned from the registration
        /// </summary>
        public string XmlRegistrationResponse
        {
            get { return _xmlRegistrationResponse; }
            set { _xmlRegistrationResponse = value; }
        }

        /// <summary>
        /// The XML document injected into the search web service
        /// </summary>
        public string XmlQueryRequest
        {
            get { return _xmlQueryRequest; }
            set { _xmlQueryRequest = value; }
        }

        /// <summary>
        /// The response from the search web service
        /// </summary>
        public string XmlQueryResponse
        {
            get { return _xmlQueryResponse; }
            set { _xmlQueryResponse = value; }
        }

        /// <summary>
        /// A data table of the response from the search web service for high confidence results
        /// </summary>
        public DataTable DataTableQueryHighConfidenceResults
        {
            get { return _dataTableQueryHighConfidenceResults; }
            set { _dataTableQueryHighConfidenceResults = value; }
        }

        /// <summary>
        /// A data table of the response from the search web service for special term results
        /// </summary>
        public DataTable DataTableQuerySpecialTermResults
        {
            get { return _dataTableQuerySpecialTermResults; }
            set { _dataTableQuerySpecialTermResults = value; }
        }

        /// <summary>
        /// A data table of the response from the search web service for relevant results
        /// </summary>
        public DataTable DataTableQueryRelevantResults
        {
            get { return _dataTableQueryRelevantResults; }
            set { _dataTableQueryRelevantResults = value; }
        }
    }
}
