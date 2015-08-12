<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityReportForm.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityReportForm" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Accountability Report</h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <div class="row">
                    <div class="col-md-10">
                        <b>Report for week ending on date:</b>
                    </div>
                    <div class="col-md-2">
                        <Rock:RockDropDownList ID="ddlSubmitForDate" runat="server"></Rock:RockDropDownList>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-2 col-md-offset-10">
                        Comments (300 chars max)
                    </div>
                </div>

                <div class="row">
                    <asp:PlaceHolder ID="phQuestions" runat="server" />
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbComments" Label="Comments (4000 chars max)" SourceTypeName="com.centralaz.Accountability.Model.ResponseSet, com.centralaz.Accountability" PropertyName="Comments" TextMode="MultiLine" Rows="5" runat="server" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <asp:LinkButton ID="lbSubmit" class="btn btn-primary" Text="Submit" runat="server" OnClick="lbSubmit_OnClick" />
                        <asp:LinkButton ID="lbCancel" class="btn btn-link" Text="Cancel" runat="server" OnClick="lbCancel_OnClick" />
                    </div>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
