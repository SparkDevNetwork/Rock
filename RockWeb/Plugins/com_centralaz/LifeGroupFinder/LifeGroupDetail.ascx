<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LifeGroupDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.LifeGroupDetail" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <script type="text/javascript">
            function jumpToControl() {
                var $item = $("[id$='tbFirstName']");
                if ($item.length > 0) {
                    $('html, body').animate({
                        scrollTop: $item.offset().top
                    }, 1000);
                }
            };
        </script>
        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" CssClass="lifegroupdetail-noticebox" NotificationBoxType="Danger" />
        <asp:Panel ID="pnlView" runat="server" Visible="true">

            <div class="row lifegroupdetail-groupname">
                <h2>
                    <asp:Literal ID="lGroupName" runat="server" />
                </h2>
            </div>

            <div class="row">
                <div class="col-md-6 lifegroupdetail-backdiv pull-left">
                    <asp:LinkButton ID="lbGoBack" runat="server" Text="Back" OnClick="lbGoBack_Click" />
                </div>
                <asp:Panel ID="pnlLogin" runat="server" CssClass="col-md-6 lifegroupdetail-loginbutton text-right">
                    <asp:LinkButton ID='lbLogin' runat='server' Text='Sign in' OnClick='lbLogin_Click' CausesValidation="false" />
                    to autocomplete forms. 
                </asp:Panel>
            </div>

            <asp:Literal ID="lOutput" runat="server" />
            <asp:Literal ID="lDebug" Visible="false" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlSignup" runat="server" Visible="true">
            <div class="lifegroupdetail-validationdiv">
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following" CssClass="alert alert-danger" />
            </div>
            <div class="lifegroupdetail-errordiv">
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
            </div>

            <div class="form-horizontal">
                <div class="form-group">
                    <label for="tbFirstName" class="col-md-2 control-label">First Name</label>
                    <div class="col-md-10">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Placeholder="First Name" Required="true" RequiredErrorMessage="First Name is required"></Rock:RockTextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label for="tbLastName" class="col-md-2 control-label">Last Name</label>
                    <div class="col-md-10">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Placeholder="Last Name" Required="true" RequiredErrorMessage="Last Name is required"></Rock:RockTextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label id="lblHome" runat="server" for="pnHome" class="col-md-2 control-label">Home Phone</label>
                    <div class="col-md-10">
                        <Rock:PhoneNumberBox ID="pnHome" runat="server" Placeholder="Home Phone" Required="true" RequiredErrorMessage="Phone Number is required"/>
                    </div>
                </div>
                <div class="form-group">
                    <label for="tbLastName" class="col-md-2 control-label">Email</label>
                    <div class="col-md-10">
                        <Rock:EmailBox ID="tbEmail" runat="server" Placeholder="Email" Required="true" RequiredErrorMessage="Email is required"></Rock:EmailBox>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lSecondSignup" runat="server" Text="Will some one else be attending with you, such as a spouse, friend, or neighbor?" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbSecondSignup" runat="server" Text="Yes" AutoPostBack="true" OnCheckedChanged="cbSecondSignup_CheckedChanged" />
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlSecondSignup" runat="server" Visible="false">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label for="tbSecondFirstName" class="col-md-2 control-label">First Name</label>
                        <div class="col-md-10">
                            <Rock:RockTextBox ID="tbSecondFirstName" runat="server" Placeholder="First Name" Required="true" RequiredErrorMessage="Additional person's First Name is required"></Rock:RockTextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="tbSecondLastName" class="col-md-2 control-label">Last Name</label>
                        <div class="col-md-10">
                            <Rock:RockTextBox ID="tbSecondLastName" runat="server" Placeholder="Last Name" Required="true" RequiredErrorMessage="Additional person's Last Name is required"></Rock:RockTextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="pnSecondHome" class="col-md-2 control-label">Home Phone</label>
                        <div class="col-md-10">
                            <Rock:PhoneNumberBox ID="pnSecondHome" runat="server" Placeholder="Home Phone" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="tbSecondEmail" class="col-md-2 control-label">Email</label>
                        <div class="col-md-10">
                            <Rock:EmailBox ID="tbSecondEmail" runat="server" Placeholder="Email" Required="true" RequiredErrorMessage="Additional person's Email is required"></Rock:EmailBox>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <div class="pull-right">
                <div class="lifegroupdetail-actiondiv actions">
                    <asp:LinkButton ID="btnRegister" runat="server" Text="Sign up!" CssClass="btn btn-primary" OnClick="btnRegister_Click" />
                    <asp:LinkButton ID="btnEmail" runat="server" Text="Send Email" CssClass="btn btn-primary" OnClick="btnEmail_Click" Visible="false" />
                </div>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <center>
            <h3>Success!</h3>
            <hr />
            <h3>
                Thank you for submitting your info!
                <br />
                You should receive a confirmation email shortly.
            </h3>
            </center>
            <div class="col-md-6 col-md-offset-3">
                <asp:Panel runat="server">

                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h3 class="panel-title padding-all-sm">Your information</h3>
                        </div>
                        <div class="panel-body">
                        <asp:Literal ID="lResultFirstName" runat="server"></asp:Literal>
                        <asp:Literal ID="lResultLastName" runat="server"></asp:Literal><br />
                        <asp:Literal ID="lResultEmail" runat="server"></asp:Literal><br />
                        <asp:Literal ID="lResultHome" runat="server"></asp:Literal>
                        </div>
                    </div>

                </asp:Panel>
                <br />

                <asp:Panel ID="pnlSecondResult" runat="server" Visible="false">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h3 class="panel-title padding-all-sm">Others information</h3>
                        </div>
                        <div class="panel-body">
                        <asp:Literal ID="lSecondResultFirstName" runat="server"></asp:Literal>
                        <asp:Literal ID="lSecondResultLastName" runat="server"></asp:Literal><br />
                        <asp:Literal ID="lSecondResultEmail" runat="server"></asp:Literal><br />
                        <asp:Literal ID="lSecondResultHome" runat="server"></asp:Literal>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEmailSent" runat="server" Visible="false">
            <h1>Your request for information has been sent.
            </h1>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
