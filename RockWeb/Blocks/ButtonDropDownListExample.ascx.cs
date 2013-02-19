using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.UI;

/// <summary>
/// 
/// </summary>
public partial class ButtonDropDownListExample : RockBlock
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
            ddlNoPostBack.Items.Clear();
            ddlNoPostBack.Items.Add( new ListItem( "Sample 1", "7" ) );
            ddlNoPostBack.Items.Add( new ListItem( "Sample 10", "11" ) );
            ddlNoPostBack.Items.Add( new ListItem( "Sample 150", "13" ) );
            ddlNoPostBack.Items.Add( new ListItem( "Sample 2500", "17" ) );
            ddlNoPostBack.SelectedValue = "13";

            ddlWithPostBack.Items.Clear();
            ddlWithPostBack.Items.Add( new ListItem( "Pickles", "44" ) );
            ddlWithPostBack.Items.Add( new ListItem( "Onions", "88" ) );
            ddlWithPostBack.Items.Add( new ListItem( "Ketchup", "150" ) );
            ddlWithPostBack.Items.Add( new ListItem( "Mustard", "654" ) );
            ddlWithPostBack.SelectedValue = "44";
        }
    }
    
    /// <summary>
    /// Handles the SelectionChanged event of the ddlWithPostBack control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void ddlWithPostBack_SelectionChanged( object sender, EventArgs e )
    {
        lblSelectedItem2.Text = ddlWithPostBack.SelectedItem.Text + " - " + ddlWithPostBack.SelectedItem.Value; ;
    }
    
    /// <summary>
    /// Handles the Click event of the bbGetNoPostBackText control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void bbGetNoPostBackText_Click( object sender, EventArgs e )
    {
        lblSelectedItem1.Text = ddlNoPostBack.SelectedItem.Text + " - " + ddlNoPostBack.SelectedItem.Value; ;
    }
}