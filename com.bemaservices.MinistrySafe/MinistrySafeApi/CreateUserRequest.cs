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
using Newtonsoft.Json;

namespace com.bemaservices.MinistrySafe.MinistrySafeApi
{
    /// <summary>
    /// JSON return structure for the create candidate API call's response.
    /// </summary>
    internal class CreateUserRequest
    {
        public string email { get; set; }
        public string employee_id { get; set; }
        public string external_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int? score { get; set; }
        public object complete_date { get; set; }
        public string direct_login_url { get; set; }
        public string user_type { get; set; }
    }
}