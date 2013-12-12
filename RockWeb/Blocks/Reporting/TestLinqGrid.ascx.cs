//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Linq;

using Rock.Model;
using Rock.Web.UI;


namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [Description( "Block to execute a linq command and display the result (if any)." )]
    public partial class TestLinqGrid : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            gReport.GridRebind += gReport_GridRebind;
            RunCommand();
        }

        void gReport_GridRebind( object sender, EventArgs e )
        {
            RunCommand();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void RunCommand()
        {
            gReport.CreatePreviewColumns( typeof( Person ) );

            var service = new PersonService();
            var people = service.Queryable().Where( p => p.LastName == "Turner" );
            var parents = service.Transform( people, new Rock.Reporting.DataTransform.Person.ParentTransform() );

            gReport.DataSource = parents.ToList();
            gReport.DataBind();
        }

        #endregion

    }
}