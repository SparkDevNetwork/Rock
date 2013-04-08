//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.DataFilters
{
    /// <summary>
    /// A filter for selecting another data filter as a filter
    /// </summary>
    public abstract class OtherDataViewFilter<T> : DataFilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Existing Data View"; }
        }

        /// <summary>
        /// Gets the name of the filtered entity type.
        /// </summary>
        /// <value>
        /// The name of the filtered entity type.
        /// </value>
        public override string FilteredEntityTypeName
        {
            get { return typeof( T ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string ClientFormatSelection
        {
            get
            {
                return "'Included in ' + " +
                    "'\\'' + $('select:first', $content).find(':selected').text() + '\\' ' + " +
                    "'Data View'";
            }
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            string s = "Another Data View";

            int dvId = int.MinValue;
            if ( int.TryParse( selection, out dvId ) )
            {
                var dataView = new DataViewService().Get( dvId );
                if ( dataView != null )
                {
                    return string.Format( "Included in '{0}' Data View", dataView.Name );
                }
            }

            return s;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( FilterField filterControl )
        {
            int entityTypeId = EntityTypeCache.Read( typeof( T ) ).Id;

            DropDownList ddlDataViews = new DropDownList();
            ddlDataViews.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( ddlDataViews );

            RockPage page = filterControl.Page as RockPage;
            if ( page != null )
            {
                foreach ( var dataView in new DataViewService().GetByEntityTypeId( entityTypeId ) )
                {
                    if ( dataView.IsAuthorized( "View", page.CurrentPerson ) &&
                        dataView.DataViewFilter.IsAuthorized( "View", page.CurrentPerson ) )
                    {
                        ddlDataViews.Items.Add( new ListItem( dataView.Name, dataView.Id.ToString() ) );
                    }
                }
            }

            return new Control[1] { ddlDataViews };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Label
            writer.AddAttribute( "class", "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( "Data View" );
            writer.RenderEndTag();

            // Controls
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[0].RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
        {
            return ( (DropDownList)controls[0] ).SelectedValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
        {
            ( (DropDownList)controls[0] ).SelectedValue = selection;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( object serviceInstance, Expression parameterExpression, string selection )
        {
            int dvId = int.MinValue;
            if ( int.TryParse( selection, out dvId ) )
            {
                var dataView = new DataViewService().Get( dvId );
                if ( dataView != null && dataView.DataViewFilter != null )
                {
                    // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
                    if ( !ThisViewInFilter( dataView.Id, dataView.DataViewFilter ) )
                    {
                        // TODO: Should probably verify security again on the selected dataview and it's filters,
                        // as that could be a moving target.
                        return dataView.GetExpression( serviceInstance, (ParameterExpression)parameterExpression );
                    }
                }
            }
            return null;
        }

        public bool ThisViewInFilter( int dataViewId, Rock.Model.DataViewFilter filter )
        {
            if ( filter.EntityTypeId == EntityTypeCache.Read( this.GetType() ).Id )
            {
                int filterDataViewId = int.MinValue;
                if (int.TryParse(filter.Selection, out filterDataViewId))
                {
                    if (filterDataViewId == dataViewId)
                    {
                        return true;
                    }
                }
            }

            foreach ( var childFilter in filter.ChildFilters )
            {
                if ( ThisViewInFilter( dataViewId, childFilter ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}