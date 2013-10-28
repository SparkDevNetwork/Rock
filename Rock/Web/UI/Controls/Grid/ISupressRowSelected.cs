//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// By default all columns in the grid will fire the OnRowSelected event when a user clicks on a cell in that column. A Grid Field can implement
    /// this interface to prevent the OnRowSelected event from being fired when this field (column) is clicked
    /// </summary>
    public interface INotRowSelectedField
    {
    }
}
