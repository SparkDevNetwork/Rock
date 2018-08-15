<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinAutoStart.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Checkin.CheckinAutoStart" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>
    
    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <asp:HiddenField ID="hfLatitude" runat="server" />
    <asp:HiddenField ID="hfLongitude" runat="server" />
    <asp:HiddenField ID="hfTheme" runat="server" />
    <asp:HiddenField ID="hfKiosk" runat="server" />

    <span style="display:none">
        <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
    </span>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>Check-in Options</h1>
    </div>
    
    <asp:Panel runat="server" CssClass="checkin-body" ID="pnlManualConfig">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                
                <Rock:RockDropDownList ID="ddlTheme" runat="server" CssClass="input-xlarge" Label="Theme" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" AutoPostBack="true" />
                <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" Label="Kiosk Device" DataTextField="Name" DataValueField="Id" />


            </div>
        </div>

    </asp:Panel>
    

    <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" runat="server" ID="lbManual" visible="true" OnClick="lbManual_Click" Text="Manual Config" />
            <asp:LinkButton CssClass="btn btn-primary pull-right" ID="lbOk" runat="server" OnClick="lbOk_Click" Text="Ok" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
