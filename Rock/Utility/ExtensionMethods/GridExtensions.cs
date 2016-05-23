// <copyright>
// Copyright by the Spark Development Network
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
using System.Web.UI.WebControls;

namespace Rock
{
    public static partial class ExtensionMethods
    {
        #region Grid Extensions

        /// <summary>
        /// Gets the grid column that matches the header text.
        /// </summary>
        /// <param name="dataControlFieldCollection">The data control field collection.</param>
        /// <see href="http://stackoverflow.com/a/22005731/1853867" />
        /// <param name="headerText">The header text.</param>
        /// <returns></returns>
        public static DataControlField GetColumnByHeaderText( this DataControlFieldCollection dataControlFieldCollection, string headerText )
        {
            foreach ( DataControlField column in dataControlFieldCollection )
            {
                if ( column.HeaderText == headerText )
                {
                    return column;
                }
            }

            return null;
        }

        #endregion
    }
}