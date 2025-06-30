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
#if WEBFORMS
using System.Web.UI.WebControls;

namespace Rock
{
    public static partial class ExtensionMethods
    {
        #region Grid Extensions

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
                    if ( grid is Rock.Web.UI.Controls.Grid )
                    {
                        return ( grid as Rock.Web.UI.Controls.Grid ).GetColumnIndex( col );
                    }
                    else
                    {
                        return grid.Columns.IndexOf( col );
                    }

                }
            }

            return null;
        }

        #endregion
    }
}
#endif
