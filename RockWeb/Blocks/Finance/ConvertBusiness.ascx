<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConvertBusiness.ascx.cs" Inherits="RockWeb.Blocks.Finance.ConvertBusiness" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building"></i> Convert Business</h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Validation" />

                <Rock:PersonPicker ID="ppSource" runat="server" IncludeBusinesses="true" Label="Person or Business" Help="Select the person or business that you want to convert." OnSelectPerson="ppSource_SelectPerson" />

                <asp:Panel ID="pnlToPerson" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsToPerson" runat="server" CssClass="alert alert-validation" ValidationGroup="ConvertToPerson" />

                    <Rock:NotificationBox ID="nbToPerson" runat="server" NotificationBoxType="Info">This business will be converted to a person using the following details.</Rock:NotificationBox>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbPersonFirstName" runat="server" Label="First Name" Required="true" ValidationGroup="ConvertToPerson" />
                            <Rock:RockTextBox ID="tbPersonLastName" runat="server" Label="Last Name" Required="true" ValidationGroup="ConvertToPerson" />
                            <Rock:DefinedValuePicker ID="dvpPersonConnectionStatus" runat="server" Label="Connection Status" Required="true" ValidationGroup="ConvertToPerson" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DefinedValuePicker ID="dvpMaritalStatus" runat="server" Label="Marital Status" Required="false" ValidationGroup="ConvertToPerson" />
                            <Rock:RockRadioButtonList ID="rblGender" runat="server" Label="Gender" RepeatDirection="Horizontal" Required="true" ValidationGroup="ConvertToPerson" />
                        </div>
                    </div>
                    
                    <div class="actions">
                        <asp:LinkButton ID="lbPersonSave" runat="server" Text="Convert" CssClass="btn btn-primary" ValidationGroup="ConvertToPerson" OnClick="lbToPersonSave_Click" />
                        <asp:LinkButton ID="lbPersonCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbPersonCancel_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlToBusiness" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsToBusiness" runat="server" CssClass="alert alert-validation" ValidationGroup="ConvertToBusiness" />

                    <Rock:NotificationBox ID="nbToBusiness" runat="server" NotificationBoxType="Info">This person will be converted to a Business with the following name.</Rock:NotificationBox>

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbBusinessName" runat="server" Label="Name" Required="true" ValidationGroup="ConvertToBusiness" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbBusinessSave" runat="server" Text="Convert" CssClass="btn btn-primary" ValidationGroup="ConvertToBusiness" OnClick="lbToBusinessSave_Click" />
                        <asp:LinkButton ID="lbBusinessCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbBusinessCancel_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>