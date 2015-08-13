<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BaptismAddBaptism.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Baptism.BaptismAddBaptism" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lPanelTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbErrorWarning" runat="server" NotificationBoxType="Danger" />
                <Rock:DateTimePicker ID="dtpBaptismDate" runat="server" Label="Date" />
                <Rock:PersonPicker ID="ppBaptizee" runat="server" Label="Person being baptized" />
                <Rock:PersonPicker ID="ppBaptizer1" runat="server" Label="Primary Baptizer" />
                <Rock:PersonPicker ID="ppBaptizer2" runat="server" Label="Secondary Baptizer (Optional)" />
                <Rock:PersonPicker ID="ppApprover" runat="server" Label="Approved By" />
                <Rock:RockCheckBox ID="cbIsConfirmed" runat="server" Label="Confirmed" />

                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_OnClick" />
                <asp:LinkButton ID="btnDelete" runat="server" Text="<i class='fa fa-trash-o'></i> Delete" CssClass="btn btn-link" OnClick="btnDelete_OnClick" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_OnClick" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
