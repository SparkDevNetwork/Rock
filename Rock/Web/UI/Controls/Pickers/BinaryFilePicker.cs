//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class BinaryFilePicker : DropDownList
    {
        /// <summary>
        /// Gets or sets the binary file type id.
        /// </summary>
        /// <value>
        /// The binary file type id.
        /// </value>
        public int? BinaryFileTypeId
        {
            get 
            { 
                return ViewState["BinaryFileTypeId"] as int?; 
            }
            set
            {
                ViewState["BinaryFileTypeId"] = value;

                this.Items.Clear();
                this.DataTextField = "FileName";
                this.DataValueField = "Id";
                this.DataSource = new BinaryFileService()
                    .Queryable()
                    .Where( f => f.BinaryFileTypeId == value && !f.IsTemporary )
                    .OrderBy( f => f.FileName )
                    .Select( f => new { f.FileName, f.Id } )
                    .ToList();
                this.DataBind();
            }
        }
    }
}