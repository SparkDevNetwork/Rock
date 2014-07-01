<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusDetail" %>

<asp:UpdatePanel ID="upCampusDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            
            <asp:HiddenField ID="hfCampusId" runat="server" />

            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbCampusName" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                    </div>
                </div>

                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbCampusCode" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="ShortCode" />
                        <Rock:PersonPicker ID="ppCampusLeader" runat="server" Label="Campus Leader" />
                        <Rock:KeyValueList ID="kvlServiceTimes" runat="server" label="Service Times" KeyPrompt="Day" ValuePrompt="Time" Help="A list of days and times that this campus has services." />
                    </div>
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Url" />
                        <Rock:DataTextBox ID="tbPhoneNumber" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="PhoneNumber" />
                        <Rock:RockTextBox ID="tbStreet" runat="server" Label="Address"></Rock:RockTextBox>
                        <div class="row">
                            <div class="col-md-7">
                                <Rock:RockTextBox ID="tbCity" runat="server" Label="City" />
                            </div>
                            <div class="col-md-2">
                                <Rock:StateDropDownList ID="ddlState" runat="server" UseAbbreviation="true" Label="State" />
                            </div>
                            <div class="col-md-3">
                                <Rock:RockTextBox  ID="tbZip" runat="server" Label="Zip" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <div class="attributes">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                    </div>
                </div>

            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
