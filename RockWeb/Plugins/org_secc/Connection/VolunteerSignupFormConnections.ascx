<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerSignupFormConnections.ascx.cs" Inherits="org.secc.Connection.VolunteerSignupFormConnections" %>
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

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4" id="divHome" runat="server">
                        <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                    </div>
                    <div class="col-md-4" id="divMobile" runat="server">
                        <Rock:PhoneNumberBox ID="pnMobile" runat="server" Label="Mobile Phone" />
                    </div>
                    <div class="col-md-4" id="divBirthdate" runat="server">
                        <Rock:BirthdayPicker ID="bpBirthdate" runat="server" Label="Birthday" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" Required="true" />
                        <Rock:RockLiteral ID="ltCampus" runat="server" Label="Campus" Visible="false" CssClass="literal" />
                    </div>
                </div>

                <div class="row">
                    <asp:Repeater runat="server" id="rptGroupRoleAttributes">
                        <ItemTemplate>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="ltRole" runat="server" Label="Serving Role" Visible="false" CssClass="literal" />
                                <asp:HiddenField runat="server" ID="hdnGroupRoleTypeId" />
                                <asp:HiddenField runat="server" ID="hdnGroupId" />
                    
                                <asp:PlaceHolder runat="server" ID="phAttributes" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <Rock:RockTextBox ID="tbComments" runat="server" Label="Comments"  TextMode="MultiLine" Rows="4" />

                <div class="actions">
                    <asp:LinkButton ID="btnConnectandAddAnother" Visible="false" runat="server" AccessKey="n" Text="Connect and Add Another" CssClass="btn btn-default" OnClick="btnConnect_Click" />
                    <asp:LinkButton ID="btnConnect" runat="server" AccessKey="m" Text="Connect" CssClass="btn btn-primary" OnClick="btnConnect_Click" />
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
