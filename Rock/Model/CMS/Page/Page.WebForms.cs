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

using Rock.Web;

namespace Rock.Model
{
    public partial class Page
    {

        /// <summary>
        /// Gets the name of the fully qualified page.
        /// </summary>
        /// <returns></returns>
        public string GetFullyQualifiedPageName()
        {
            string result = System.Web.HttpUtility.HtmlEncode( this.InternalName );

            result = string.Format( "<a href='{0}'>{1}</a>", new PageReference( this.Id ).BuildUrl(), result );

            Rock.Model.Page parent = this.ParentPage;
            while ( parent != null )
            {
                result = string.Format( "<a href='{0}'>{1}</a> / {2}", new PageReference( parent.Id ).BuildUrl(), System.Web.HttpUtility.HtmlEncode( parent.InternalName ), result );
                parent = parent.ParentPage;
            }

            return result;
        }
    }
}
