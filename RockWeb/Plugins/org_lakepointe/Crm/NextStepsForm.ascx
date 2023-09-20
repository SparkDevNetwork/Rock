<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NextStepsForm.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Crm.NextStepsForm" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPersonId" runat="server" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-warning" HeaderText="Please correct the following" ValidationGroup="vgSchedule" />



        <asp:Panel ID="pnlSchedule" CssClass="panel panel-block" runat="server" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lScheduleBaptismHeader" runat="server"></asp:Literal></h1>
            </div>
            <div class="panel-body">
                <asp:Panel ID="pnlIntro" runat="server" CssClass="row">
                    <div class="col-xs-12">                 
                        <asp:Literal ID="lIntroText" runat="server" />
                    </div>
                </asp:Panel>
                <div class="row">
                    <div class="col-sm-6">
                        <h3>Person Information</h3>
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Which Lakepointe campus do you attend?" Required="true" RequiredErrorMessage="Please select a campus" ValidationGroup="vgSchedule" AutoPostBack="true" OnSelectedIndexChanged="ddlCampus_SelectedIndexChanged" />
                    </div>
                </div>
                    <asp:Panel ID="pnlPersonSelection" CssClass="panel panel-block" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:PersonPicker ID="personpicker" runat="server" Label="Select Person" Required="true" RequiredErrorMessage="Please select a person" />
                            </div>
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="pnlPersonInformation" CssClass="panel panel-block" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbNickName" runat="server" Label="First Name" Required="true" RequiredErrorMessage="First Name is required" ValidationGroup="vgSchedule" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" RequiredErrorMessage="Last Name is required" ValidationGroup="vgSchedule" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:DatePicker ID="dpBirthdate" runat="server" Label="Birthdate" AllowFutureDateSelection="false" Required="true" RequiredErrorMessage="Birthdate is required" ValidationGroup="vgSchedule" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockRadioButtonList ID="rblGender" runat="server" Label="Gender" Required="true" RequiredErrorMessage="Gender is required" RepeatDirection="Horizontal" RepeatColumns="2" ValidationGroup="vgSchedule">
                                <asp:ListItem Value="1" Text="Male" />
                                <asp:ListItem Value="2" Text="Female" />
                            </Rock:RockRadioButtonList>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" RequiredErrorMessage="Email is required" ValidationGroup="vgSchedule" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:PhoneNumberBox ID="tbMobilePhone" runat="server" Label="Mobile Phone" Required="true" RequiredErrorMessage="Phone Number is required" ValidationGroup="vgSchedule" />
                        </div>
                        <div class="col-sm-3">
                            <Rock:Toggle ID="tglMayWeTextYou" runat="server" Label="May We Text You" OffText="No" OnText="Yes"  ToolTip="Message Frequency Varies. Carrier message and data rates may apply."
                                Required="true" RequiredErrorMessage="May we text you is required." Checked="true" ValidationGroup="vgSchedule" />
                        </div>

                    </div>
                    <asp:Panel ID="pnlAddress" runat="server" CssClass="row" Visible="true">
                        <div class="col-sm-12">
                            <Rock:AddressControl ID="acHomeAddress" runat="server" Label="Home Address" Required="false" RequiredErrorMessage="Home Address is required" ValidationGroup="vgSchedule" />

                        </div>
                    </asp:Panel>
                </asp:Panel>


                <div class="row">
                    <div class="col-sm-6">
                        <h3>Testimony</h3>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-6">
                        <Rock:RockCheckBox ID="rckTestimonyTaken" Checked="false" Label="Testimony Taken" runat="server" Text="I received this indivudial's testimony." Visible="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:RockTextBox ID="tbTestionyNotes" runat="server" TextMode="MultiLine" Rows="4" Label="Testimony Notes" />
                    </div>
                </div>
                <br />


                <div class="row">
                    <div class="col-sm-6">
                        <h3>Schedule Next Step Class</h3>
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlNextStepsClassDate" runat="server" Label="Preferred Next Step Class Date" />
                    </div>
                </div>

                <br />

                <div class="row">
                    <div class="col-sm-6">
                        <h3>Schedule Baptism</h3>
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlBaptismDate" runat="server" Label="Requested Baptism Date" AutoPostBack="true" OnSelectedIndexChanged="ddlBaptismDate_SelectedIndexChanged"  ValidationGroup="vgSchedule"/>
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlBaptismTime" runat="server" Label="Requested Service Time" RequiredErrorMessage="Please select a service time" ValidationGroup="vgSchedule" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:DatePicker ID="dpAcceptChrist" runat="server" Label="Approximately when did you accept Christ?" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:RockTextBox ID="tbComments" runat="server" TextMode="MultiLine" Rows="4" Label="Is there anything that we need to know?" />
                    </div>
                </div>

                <div class="actions">
                    <Rock:BootstrapButton ID="lbSave" runat="server" Text="Submit" CssClass="btn btn-primary" DataLoadingText="Saving..." OnClick="lbSave_Click" ValidationGroup="vgSchedule" />
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlConfirm" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lConfirmationHeader" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal ID="lConfirmMessage" runat="server" />
                    </div>
                </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
