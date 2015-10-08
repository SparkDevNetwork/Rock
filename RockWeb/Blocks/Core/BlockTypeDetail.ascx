<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockTypeDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <asp:HiddenField ID="hfBlockTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-square"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />


                <fieldset>
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.BlockType, Rock" PropertyName="Name" Label="Name" />
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.BlockType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    <Rock:DataTextBox ID="tbPath" runat="server" SourceTypeName="Rock.Model.BlockType, Rock" PropertyName="Path" CssClass="input-xlarge" />
                    <Rock:RockLiteral ID="lblStatus" runat="server" Label="Status" />
                    <Rock:RockLiteral ID="lPages" runat="server" Label="Pages that use this block type" />
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

