//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// By default any BoundField will participate in the RowSelected event of the grid.  A custom BoundField can implement
    /// this interface to prevent the RowSelected event from being fired when this field (column) is clicked on
    /// </summary>
    public interface INotRowSelectedField
    {
    }
}
