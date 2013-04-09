//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [Description( "Block to execute a linq command and display the result (if any)." )]
    public partial class LinqGrid : RockBlock
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

            IQueryable<int> Ids = service.Queryable().Where( p => p.LastName == "Turner" ).Select( p => p.Id );

            var txfrm = service.Queryable()
                .Where( p => p.Members.Where( a => a.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                    .Any( a => a.Group.Members.Any( c => c.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) ) &&
                         Ids.Contains( c.Person.Id ) ) ) );
                    
            gReport.DataSource = txfrm.ToList();
            gReport.DataBind();
        }

        #endregion

    }
}