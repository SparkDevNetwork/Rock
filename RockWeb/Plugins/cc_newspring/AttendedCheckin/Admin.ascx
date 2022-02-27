<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.AttendedCheckin.Admin" %>

<asp:UpdatePanel ID="pnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
        <asp:HiddenField ID="hfKiosk" runat="server" />
        <asp:HiddenField ID="hfTheme" runat="server" />
        <asp:HiddenField ID="hfCheckinType" runat="server" />
        <asp:HiddenField ID="hfGroupTypes" runat="server" />
        <asp:HiddenField ID="hfLatitude" runat="server" />
        <asp:HiddenField ID="hfLongitude" runat="server" />

        <div style="display: none">
            <asp:LinkButton ID="lbTestPrint" runat="server" />
            <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click" />
            <asp:LinkButton ID="lbCheckGeoLocation" runat="server" OnClick="lbCheckGeoLocation_Click" />
        </div>

        <Rock:ModalAlert ID="maAlert" runat="server" />

        <asp:Panel ID="pnlAdmin" runat="server" DefaultButton="lbOk" CssClass="attended">
            <asp:UpdatePanel ID="pnlHeader" runat="server">
                <ContentTemplate>
                    <div class="row checkin-header">
                        <div class="col-xs-3">
                            <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-large" Label="Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" Visible="false" />
                            <Rock:RockDropDownList ID="ddlTheme" runat="server" CssClass="input-large" Label="Theme" Visible="false" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                        <div class="col-xs-6 text-center">
                            <h1>Admin</h1>
                        </div>
                        <div class=" col-xs-offset-1 col-xs-2 checkin-actions text-right">
                            <Rock:BootstrapButton ID="lbOk" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbOk_Click">
                                <span class="fa fa-arrow-right"></span>
                            </Rock:BootstrapButton>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div class="row checkin-body">
                <div class="col-xs-12 centered">
                    <asp:Label ID="lblHeader" runat="server" Visible="false" CssClass="push-quarter-top"><h4>Checkin Area(s)</h4></asp:Label>
                    <asp:DataList ID="ddlGroupTypes" runat="server" OnItemDataBound="ddlGroupTypes_ItemDataBound" RepeatColumns="3" CssClass="full-width centered">
                        <ItemStyle CssClass="expanded" />
                        <ItemTemplate>
                            <asp:Button ID="btnGroupType" runat="server" data-id='<%# Eval("Id") %>' CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" Text='<%# Eval("Name") %>' OnClientClick="toggleGroupType(this); return false;" />
                        </ItemTemplate>
                    </asp:DataList>
                </div>
            </div>

            <div class="row checkin-footer centered">
                <asp:Label ID="lblInfo" runat="server" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">

    function toggleGroupType(element) {
        var hfGroupTypes = $("input[id$='hfGroupTypes']"); 
        $(element).toggleClass('active').blur();
        var selectedIds = hfGroupTypes.val();
        var groupTypeId = element.getAttribute('data-id');
        if (selectedIds.indexOf(groupTypeId) >= 0) { // already selected, remove id
            var selectedIdRegex = new RegExp(groupTypeId + ',', "g");
            hfGroupTypes.val(selectedIds.replace(selectedIdRegex, ''));
        } else { // newly selected, add id
            hfGroupTypes.val(groupTypeId + ',' + selectedIds);
        }
    };

    var setKeyboardEvents = function () {
        $(document).unbind('keydown').keydown(function (e) {
            if (e.keyCode === 73 && e.ctrlKey) {
                // Ctrl + Shift + I
                e.stopPropagation();
                var postbackArg = $('a[id$="lbTestPrint"]').attr('id');
                window.location = "javascript:__doPostBack('" + postbackArg + "', '')";
                return false;
            }
        });
    };

    $(document).ready(function () {
        setKeyboardEvents();
    });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setKeyboardEvents);
</script>
