//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RockControlGallery : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gExample.DataKeyNames = new string[] { "id" };
            gExample.GridRebind += gExample_GridRebind;
        }



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

                ddlDataExample.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );

                labeledCheckBoxList.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );
                labeledRadioButtonList.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );

                BindGrid();
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

        /// <summary>
        /// Handles the GridRebind event of the gExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gExample_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        private class ExampleDataItem
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            /// <value>
            /// The id.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the color of the defined value.
            /// </summary>
            /// <value>
            /// The color of the defined value.
            /// </value>
            public string DefinedValueColor { get; set; }

            /// <summary>
            /// Gets or sets the name of the defined value type.
            /// </summary>
            /// <value>
            /// The name of the defined value type.
            /// </value>
            public string DefinedValueTypeName { get; set; }

            /// <summary>
            /// Gets or sets some date time.
            /// </summary>
            /// <value>
            /// Some date time.
            /// </value>
            public DateTime SomeDateTime { get; set; }

            /// <summary>
            /// Gets or sets some boolean.
            /// </summary>
            /// <value>
            /// Some boolean.
            /// </value>
            public Boolean SomeBoolean { get; set; }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            Random random = new Random();

            List<ExampleDataItem> dataList = new List<ExampleDataItem>();
            dataList.Add( new ExampleDataItem { Id = 1, DefinedValueColor = "green", DefinedValueTypeName = "Pickles", SomeDateTime = DateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );
            dataList.Add( new ExampleDataItem { Id = 2, DefinedValueColor = "#ff0000", DefinedValueTypeName = "Ketchup", SomeDateTime = DateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );
            dataList.Add( new ExampleDataItem { Id = 3, DefinedValueColor = "white", DefinedValueTypeName = "Onions", SomeDateTime = DateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );
            dataList.Add( new ExampleDataItem { Id = 4, DefinedValueColor = "rgb(255,255,0)", DefinedValueTypeName = "Mustard", SomeDateTime = DateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );

            SortProperty sortProperty = gExample.SortProperty ?? new SortProperty( new GridViewSortEventArgs( "DefinedValueTypeName", SortDirection.Ascending ) );

            gExample.DataSource = dataList.AsQueryable().Sort(sortProperty).ToList();
            gExample.DataBind();
        }


    }
}