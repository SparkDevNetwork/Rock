// <copyright>
// Copyright by BEMA Information Technologies
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
using com.bemaservices.WorkflowExtensions.Field.Types;
using Rock.Attribute;
using Rock.Field.Types;
using Rock.Model;

namespace com.bemaservices.WorkflowExtensions.Attribute
{
    public class ScheduleBuilderFieldAttribute : FieldAttribute
    {
        public ScheduleBuilderFieldAttribute( string name = "Schedule Builder", string description = "", bool required = true, string category = "", int order = 0, string key = null )
            : base( name, description, required, null, category, order, key, typeof( ScheduleBuilderFieldType ).FullName )
        {
        }
    }
}
