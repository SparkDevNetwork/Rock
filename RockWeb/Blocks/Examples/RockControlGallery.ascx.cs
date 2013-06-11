using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RockControlGallery : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                bddlExample.Items.Clear();
                bddlExample.Items.Add( new ListItem( "Pickles", "44" ) );
                bddlExample.Items.Add( new ListItem( "Onions", "88" ) );
                bddlExample.Items.Add( new ListItem( "Ketchup", "150" ) );
                bddlExample.Items.Add( new ListItem( "Mustard", "654" ) );
                bddlExample.SelectedValue = "44";
                
                ddlDataExample.Items.AddRange(bddlExample.Items.OfType<ListItem>().ToArray());

                labeledCheckBoxList.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );
                labeledRadioButtonList.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowAttributeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowAttributeEditor_Click( object sender, EventArgs e )
        {
            aeExampleDiv.Visible = !aeExampleDiv.Visible;
        }

        /// <summary>
        /// Handles the CancelClick event of the aeExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void aeExample_CancelClick( object sender, EventArgs e )
        {
            aeExampleDiv.Visible = false;
        }

        /// <summary>
        /// Handles the SaveClick event of the aeExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void aeExample_SaveClick( object sender, EventArgs e )
        {
            aeExampleDiv.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the binaryFileTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void binaryFileTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            binaryFilePicker.BinaryFileTypeId = binaryFileTypePicker.SelectedValueAsInt();
        }

        /// <summary>
        /// Handles the Click event of the btnToggleLabels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnToggleLabels_Click( object sender, EventArgs e )
        {
            foreach ( var control in pnlDetails.Controls )
            {
                if ( control is ILabeledControl )
                {
                    ILabeledControl labeledControl = control as ILabeledControl;
                    if ( string.IsNullOrWhiteSpace( labeledControl.LabelText ) )
                    {
                        labeledControl.LabelText = string.Format( "Rock:{0}", labeledControl.GetType().Name );
                    }
                    else
                    {
                        labeledControl.LabelText = string.Empty;
                    }
                }
                else if ( control is HtmlGenericControl )
                {
                    HtmlGenericControl hg = ( control as HtmlGenericControl );
                    if ( hg.TagName.Equals( "h4", StringComparison.OrdinalIgnoreCase ) )
                    {
                        hg.Visible = !hg.Visible;
                    }
                }
                
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the monthYearPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void monthYearPicker_TextChanged( object sender, EventArgs e )
        {
            var date = monthYearPicker.SelectedDate;
        }
        
        /// <summary>
        /// Handles the TextChanged event of the monthDayPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void monthDayPicker_TextChanged( object sender, EventArgs e )
        {
            var date = monthDayPicker.SelectedDate;
        }
}
}