<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunitySignup.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionOpportunitySignup" %>

<asp:UpdatePanel ID="upnlOpportunityDetail" runat="server">
    <ContentTemplate>
        
        <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Visible="false" />

        <asp:Literal ID="lResponseMessage" runat="server" Visible="false" />
        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

        <asp:Panel ID="pnlSignup" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lIcon" runat="server" /> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:FirstNameTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6" id="pnlHomePhone" runat="server">
                        <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                    </div>
                    <div class="col-md-6" id="pnlMobilePhone" runat="server">
                        <Rock:PhoneNumberBox ID="pnMobile" runat="server" Label="Mobile Phone" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbComments" runat="server" Label="Comments" TextMode="MultiLine" Rows="4" />

                <div class="actions">
                    <asp:LinkButton ID="btnConnect" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Connect" CssClass="btn btn-primary" OnClick="btnConnect_Click" />
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
