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
    public class WorkflowTypePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTypePicker" /> class.
        /// </summary>
        public WorkflowTypePicker()
        {
            this.Items.Clear();
            this.DataTextField = "Name";
            this.DataValueField = "Id";
            this.DataSource = new WorkflowTypeService().Queryable().OrderBy( w => w.Name).ToList();
            this.DataBind();
        }

        /// <summary>
        /// Selects the value as int.
        /// </summary>
        /// <returns></returns>
        public int? SelectedValueAsInt( bool NoneAsNull = true )
        {
            if ( NoneAsNull )
            {
                if ( this.SelectedValue.Equals( Rock.Constants.None.Id.ToString() ) )
                {
                    return null;
                }
            }

            if ( string.IsNullOrWhiteSpace( this.SelectedValue ) )
            {
                return null;
            }
            else
            {
                return int.Parse( this.SelectedValue );
            }
        }

    }
}