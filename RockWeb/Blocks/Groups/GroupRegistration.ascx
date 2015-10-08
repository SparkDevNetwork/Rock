<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRegistration.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupRegistration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" NotificationBoxType="Danger"/>

        <asp:Panel ID="pnlView" runat="server">

            <asp:Literal ID="lLavaOverview" runat="server" />
            <asp:Literal ID="lLavaOutputDebug" runat="server" />

            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div class="row">
                <asp:Panel id="pnlCol1" runat="server">
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" ></Rock:RockTextBox>
                    <asp:Panel ID="pnlHomePhone" runat="server" CssClass="row">
                        <div class="col-sm-7">
                            <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                        </div>
                        <div class="col-sm-5">
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlCellPhone" runat="server" CssClass="row">
                        <div class="col-sm-7">
                            <Rock:PhoneNumberBox ID="pnCell" runat="server" Label="Cell Phone" />
                        </div>
                        <div class="col-sm-5">
                            <Rock:RockCheckBox ID="cbSms"  runat="server" Label="&nbsp;" Text="Enable SMS" />
                        </div>
                    </asp:Panel>
                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" ></Rock:EmailBox>
                    <Rock:AddressControl ID="acAddress" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlCol2" runat="server" CssClass="col-md-6">
                    <Rock:RockTextBox ID="tbSpouseFirstName" runat="server" Label="Spouse First Name" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="tbSpouseLastName" runat="server" Label="Spouse Last Name" ></Rock:RockTextBox>
                    <div class="row">
                        <div class="col-sm-7">
                            <Rock:PhoneNumberBox ID="pnSpouseCell" runat="server" Label="Spouse Cell Phone" />
                        </div>
                        <div class="col-sm-5">
                            <Rock:RockCheckBox ID="cbSpouseSms"  runat="server" Label="&nbsp;" Text="Enable SMS" />
                        </div>
                    </div>
                    <Rock:EmailBox ID="tbSpouseEmail" runat="server" Label="Email" />
                </asp:Panel>

            </div>

            <div class="actions">
                <asp:LinkButton ID="btnRegister" runat="server" CssClass="btn btn-primary" OnClick="btnRegister_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <asp:Literal ID="lResult" runat="server" />
            <asp:Literal ID="lResultDebug" runat="server" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
