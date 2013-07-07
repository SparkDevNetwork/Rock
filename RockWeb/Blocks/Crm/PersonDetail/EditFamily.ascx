<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditFamily" %>

<asp:UpdatePanel ID="upEditFamily" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

        <asp:ValidationSummary ID="valSummaryTop" runat="server"  
            HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

        <div class="row-fluid">
            <div class="span4 form-horizontal">
                <fieldset>
                    <Rock:LabeledTextBox ID="tbFamilyName" runat="server" LabelText="Family Name" Required="true" CssClass="input-meduim" />
                </fieldset>
            </div>
            <div class="span4 form-horizontal">
                <fieldset>
                    <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                </fieldset>
            </div>
             <div class="span4 form-horizontal">
                <fieldset>
                    <Rock:LabeledDropDownList ID="ddlRecordStatus" runat="server" LabelText="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                    <Rock:LabeledDropDownList ID="ddlReason" runat="server" LabelText="Reason" Visible="false"></Rock:LabeledDropDownList>
                </fieldset>

            </div>
       </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
