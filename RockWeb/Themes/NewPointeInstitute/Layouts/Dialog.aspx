<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.DialogPage" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="Rock" %>

<!DOCTYPE html>

<script runat="server">
    
    /// <summary>
    /// An optional subtitle
    /// </summary>
    /// <value>
    /// The sub title.
    /// </value>
    public override string SubTitle
    {
        get
        {
            return lSubTitle.Text.TrimStart( "<small>".ToCharArray() ).TrimEnd( "</small>".ToCharArray() );
        }
        set
        {
            lSubTitle.Text = string.IsNullOrWhiteSpace( value ) ? "" : "<small>" + value + "</small>";
        }
    }

    /// <summary>
    /// Gets or sets the close message.
    /// </summary>
    /// <value>
    /// The close message.
    /// </value>    
    public override string CloseMessage
    {
        get
        {
            return hfCloseMessage.Value;
        }
        set
        {
            hfCloseMessage.Value = value;
        }
    }

    /// <summary>
    /// Handles the Click event of the btnSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnSave_Click( object sender, EventArgs e )
    {
        base.FireSave( sender, e );
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        lTitle.Text = Request.QueryString["t"] ?? "Title";

        btnSave.Text = Request.QueryString["pb"] ?? "Save";
        btnSave.Visible = btnSave.Text.Trim() != string.Empty;

        btnCancel.Text = Request.QueryString["sb"] ?? "Cancel";
        btnCancel.Visible = btnCancel.Text.Trim() != string.Empty;
        if ( !btnSave.Visible )
        {
            btnCancel.AddCssClass( "btn-primary" );
        }
    }    
    
</script>

<html class="no-js">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=10" />
    <title></title>

    <script src="<%# ResolveRockUrl("~/Scripts/modernizr.js", true) %>"></script>
    <script src="<%# ResolveRockUrl("~/Scripts/jquery-1.10.2.min.js", true) %>"></script>

    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/bootstrap.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/theme.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css", true) %>" />

    <style>
        html, body {
            height: auto;
            width: 100%;
            min-width: 100%;
            margin: 0 0 0 0;
            padding: 0 0 0 0;
            vertical-align: top;
        }
    </style>

</head>

<body id="dialog" class="rock-modal">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="sManager" runat="server" />
        <asp:UpdatePanel ID="updatePanelDialog" runat="server">
            <ContentTemplate>
                <div class="modal-content">
                    <Rock:HiddenFieldWithClass ID="hfCloseMessage" runat="server" CssClass="modal-close-message" />
                    <div class="modal-header">
                        <a id="closeLink" href="#" class="close" onclick="window.parent.Rock.controls.modal.close($(this).closest('.modal-content').find('.modal-close-message').first().val());">&times;</a>
                        <h3 class="modal-title">
                            <asp:Literal ID="lTitle" runat="server"></asp:Literal></h3>
                        <asp:Literal ID="lSubTitle" runat="server"></asp:Literal>
                    </div>

                    <div class="modal-body">

                        <!-- Ajax Error -->
                        <div class="alert alert-danger ajax-error" style="display:none">
                            <p><strong>Error</strong></p>
                            <span class="ajax-error-message"></span>
                        </div>

                        <Rock:Zone Name="Main" runat="server" />

                    </div>

                    <div class="modal-footer">
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClientClick="window.parent.Rock.controls.modal.close($(this).closest('.modal-content').find('.modal-close-message').first().val());" CausesValidation="false" />
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click " />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>


</html>
<script>
    Sys.Application.add_load(function () {
        Rock.controls.modal.updateSize();
    });
</script>
