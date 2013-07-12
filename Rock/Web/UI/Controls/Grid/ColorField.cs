using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ColorField runat=server></{0}:ColorField>" )]
    public class ColorField : BoundField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorField"/> class.
        /// </summary>
        public ColorField()
            : base()
        {
            this.HeaderStyle.CssClass = "color-field";
            this.ItemStyle.CssClass = "color-field";
        }

        /// <summary>
        /// Gets or sets the tool tip data field.
        /// </summary>
        /// <value>
        /// The tool tip data field.
        /// </value>
        public string ToolTipDataField
        {
            get
            {
                return ViewState["ToolTipDataField"] as string;
            }

            set
            {
                ViewState["ToolTipDataField"] = value;
            }
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            // we only want to set the background color style, no text
            return string.Empty;
        }

        /// <summary>
        /// Initializes the specified <see cref="T:System.Web.UI.WebControls.TableCell" /> object to the specified row state.
        /// </summary>
        /// <param name="cell">The <see cref="T:System.Web.UI.WebControls.TableCell" /> to initialize.</param>
        /// <param name="rowState">One of the <see cref="T:System.Web.UI.WebControls.DataControlRowState" /> values.</param>
        protected override void InitializeDataCell( DataControlFieldCell cell, DataControlRowState rowState )
        {
            base.InitializeDataCell( cell, rowState );
            cell.DataBinding += cell_DataBinding;
        }

        /// <summary>
        /// Handles the DataBinding event of the cell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cell_DataBinding( object sender, EventArgs e )
        {
            if ( sender is TableCell )
            {
                TableCell cell = sender as TableCell;
                GridViewRow row = ( sender as TableCell ).Parent as GridViewRow;

                if ( row.DataItem != null )
                {
                    string dataValue = row.DataItem.GetPropertyValue( this.DataField ) as string;
                    string toolTipValue = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( this.ToolTipDataField ) )
                    {
                        toolTipValue = row.DataItem.GetPropertyValue( this.ToolTipDataField ) as string;
                    }

                    cell.ToolTip = toolTipValue;
                    cell.Style[HtmlTextWriterStyle.BackgroundColor] = dataValue;
                }
            }
        }
    }
}
