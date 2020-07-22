<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Admin" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <asp:PlaceHolder ID="phGeoCodeScript" runat="server" />
    <asp:HiddenField ID="hfGeoError" runat="server" />
    <asp:HiddenField ID="hfLatitude" runat="server" />
    <asp:HiddenField ID="hfLongitude" runat="server" />

    <span style="display:none">
        <asp:LinkButton ID="lbCheckGeoLocation" runat="server" OnClick="lbCheckGeoLocation_Click"></asp:LinkButton>
    </span>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>Check-in Options</h1>
    </div>

    <asp:Panel runat="server" CssClass="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">
                <Rock:NotificationBox ID="nbGeoMessage" runat="server" NotificationBoxType="Danger" />

                <asp:Panel ID="pnlManualConfig" runat="server" Visible="false">
                    <Rock:RockDropDownList ID="ddlTheme" runat="server" CssClass="input-xlarge" Label="Theme" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" AutoPostBack="true" />
                    <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" Label="Kiosk Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" />
                    <Rock:RockDropDownList ID="ddlCheckinType" runat="server" CssClass="input-xlarge" Label="Check-in Configuration" OnSelectedIndexChanged="ddlCheckinType_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id"/>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList ID="cblPrimaryGroupTypes" runat="server" Label="Check-in Area(s)" DataTextField="Name" DataValueField="Id" ></Rock:RockCheckBoxList>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList ID="cblAlternateGroupTypes" runat="server" Label="Additional Area(s)" DataTextField="Name" DataValueField="Id" ></Rock:RockCheckBoxList>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>

    </asp:Panel>


    <div class="checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbOk" runat="server" OnClick="lbOk_Click" Text="OK" Visible="false" />
            <a class="btn btn-default" runat="server" ID="lbRetry" visible="false" href="javascript:window.location.href=window.location.href" >Retry</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
