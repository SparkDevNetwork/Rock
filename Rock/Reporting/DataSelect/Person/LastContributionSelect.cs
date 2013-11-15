using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using Rock.Model;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Selects Last Contribution Fields for a Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Last Contribution Fields" )]
    public class LastContributionSelect : DataSelectComponent<Rock.Model.Person>
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get
            {
                return "Last Contribution";
            }
        }

        /// <summary>
        /// Gets the name of the entity type.
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string EntityTypeName
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the data column values.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override List<object> GetDataColumnValues( Rock.Model.Person person )
        {
            FinancialTransactionService financialTransactionService = new FinancialTransactionService();
            var lastContribution = financialTransactionService.Queryable().Where( a => a.AuthorizedPersonId == person.Id ).OrderByDescending( a => a.TransactionDateTime ).Take( 1 ).FirstOrDefault();

            List<object> result = new List<object>();

            if ( lastContribution != null )
            {
                result.Add( lastContribution.Amount );
                result.Add( lastContribution.TransactionDateTime );
            }

            return result;
        }

        #region Controls methods

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( Type entityType, System.Web.UI.Control parentControl )
        {
            // TODO: Add Account Picker
            
            return base.CreateChildControls( entityType, parentControl );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            // TODO: 
            return base.FormatSelection( selection );
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the widget is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The client format script.
        ///   </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            // TODO: 
            return base.GetClientFormatSelection( entityType );
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="parentControl"></param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            // TODO: 
            base.RenderControls( entityType, parentControl, writer, controls );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, System.Web.UI.Control[] controls, string selection )
        {
            // TODO: 
            base.SetSelection( entityType, controls, selection );
        }

        #endregion

        /// <summary>
        /// Gets the data columns.
        /// </summary>
        /// <value>
        /// The data columns.
        /// </value>
        public override List<System.Data.DataColumn> DataColumns
        {
            get
            {
                List<DataColumn> result = new List<DataColumn>();
                result.Add( new DataColumn( "LastContributionAmount", typeof( decimal ) ) );
                result.Add( new DataColumn( "LastContributionDateTime", typeof( DateTime ) ) );
                return result;
            }
        }
    }
}
