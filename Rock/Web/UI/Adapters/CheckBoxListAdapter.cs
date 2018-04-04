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
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for checkbox list
    /// </summary>
    public class CheckBoxListAdapter : ListControlAdaptor
    {
        /// <summary>
        /// Gets the type of the input tag.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public override string GetInputTagType( ListControl listControl )
        {
            return "checkbox";
        }

        /// <summary>
        /// Gets the repeat columns.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        public override int GetRepeatColumns( ListControl listControl )
        {
            return ( listControl as CheckBoxList )?.RepeatColumns ?? 0;
        }

        /// <summary>
        /// Gets the repeat direction.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        protected override RepeatDirection GetRepeatDirection( ListControl listControl )
        {
            return ( listControl as CheckBoxList )?.RepeatDirection ?? RepeatDirection.Vertical;
        }

        /// <summary>
        /// Gets the name of the input.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="itemIndex">Index of the item.</param>
        /// <returns></returns>
        public override string GetInputName( ListControl listControl, int itemIndex )
        {
            return $"{listControl.UniqueID}${itemIndex}";
        }
    }
}