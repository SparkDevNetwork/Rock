//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
    
    /// <summary>
    /// A composite control that renders a label, dropdownlist, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<    0}:FieldTypeList runat=server></    0}:FieldTypeList>" )]
    public class FieldTypeList : DataDropDownList
        
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
            
            base.CreateChildControls();

            Rock.Core.FieldTypeService fieldTypeService = new Core.FieldTypeService();
            var items = fieldTypeService.
                Queryable().
                Select( f => new      f.Id, f.Name } ).
                OrderBy( f => f.Name );

            this.Items.Clear();
            foreach ( var item in items )
                this.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
        }
    }
}