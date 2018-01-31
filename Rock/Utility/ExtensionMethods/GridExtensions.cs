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

namespace Rock
{
    public static partial class ExtensionMethods
    {
        #region Grid Extensions

        /// <summary>
        /// Gets the first grid column that matches the header text.
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

        /// <summary>
        /// Gets the first grid column that matches the header text.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="headerText">The header text.</param>
        /// <returns></returns>
        public static DataControlField GetColumnByHeaderText( this GridView grid, string headerText )
        {
            return grid.Columns.GetColumnByHeaderText( headerText );
        }

        /// <summary>
        /// Gets the index of the grid's first column that contains the specified type.  This is 
        /// very useful when you're looking for for the column that has the DeleteField or EditField.
        /// Use like so:
        /// int? index  = gFields.GetColumnIndexByFieldType( typeof( DeleteField ) );
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static int? GetColumnIndexByFieldType( this GridView grid, System.Type type )
        {
            foreach ( DataControlField col in grid.Columns )
            {
                if ( col.GetType() == type )
                {
                    return grid.Columns.IndexOf( col );
                }
            }

            return null;
        }

        #endregion
    }
}