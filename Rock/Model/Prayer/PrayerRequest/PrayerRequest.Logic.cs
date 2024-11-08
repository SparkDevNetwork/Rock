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

namespace Rock.Model
{
    public partial class PrayerRequest
    {
        /// <summary>
        /// Gets  full name of the person for who the prayer request is about.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this prayer request is about.
        /// </value>
        public virtual string FullName
        {
            get
            {
                return string.Format( "{0} {1}", FirstName, LastName );
            }
        }

        /// <summary>
        /// Gets the full name of the person who this prayer request is about in Last Name, First Name format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this prayer request is about in last name first name format.
        /// </value>
        public virtual string FullNameReversed
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, FirstName );
            }
        }

        /// <summary>
        /// Gets the name of the prayer request. The format for this is the EnteredDate - FullName. This is required to implement ICategorized
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of prayer request. 
        /// </value>
        public virtual string Name
        {
            get
            {
                return string.Format( "{0} - {1:MM/dd/yy}", FullName, EnteredDateTime );
            }
        }

        /// <summary>
        /// The <see cref="Rock.Web.SystemSettings" /> configuration object for prayer request AI completions.
        /// Identified by the <seealso cref="SystemKey.SystemSetting.PRAYER_REQUEST_AI_COMPLETIONS"/>.
        /// </summary>
        public class PrayerRequestAICompletions
        {
            /// <summary>
            /// The Lava template to be used for generating the text formatting AI completion request.
            /// </summary>
            public string PrayerRequestFormatterTemplate { get; set; }

            /// <summary>
            /// The Lava template to be used for generating the analysis completion request
            /// for a prayer request (e.g. Sentiment Classification, auto-cateogrization etc.).
            /// </summary>
            public string PrayerRequestAnalyzerTemplate { get; set; }
        }
    }
}
