//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DetailPage]
    public partial class WorkflowTypeTreeView : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string[] eventArgs = ( Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries );
            if ( eventArgs.Length == 2 )
            {
                if ( eventArgs[0].Equals("categoryId", StringComparison.OrdinalIgnoreCase) )
                {
                    categoryItem_Click( eventArgs[1] );
                }
                else if ( eventArgs[0].Equals("workflowTypeId", StringComparison.OrdinalIgnoreCase) )
                {
                    workflowTypeItem_Click( eventArgs[1] );
                }
            }
        }

        /// <summary>
        /// Categories the item_ click.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        protected void categoryItem_Click( string categoryId )
        {
            NavigateToDetailPage( "categoryId", int.Parse( categoryId ) );
        }

        /// <summary>
        /// Workflows the type item_ click.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type id.</param>
        protected void workflowTypeItem_Click( string workflowTypeId )
        {
            NavigateToDetailPage( "workflowTypeId", int.Parse( workflowTypeId ) );
        }
    }
}