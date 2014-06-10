<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.PersonDetail" %>

<asp:UpdatePanel ID="upResidencyPersonDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfPersonId" runat="server" />

            <div id="pnlEditDetails" runat="server" class="well">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <Rock:PersonPicker ID="ppPerson" runat="server" Label="Select Resident" Required="true" />

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
            
            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Resident
                </legend>
                <div class="well">
                    <div class="row-fluid">
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>

            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
