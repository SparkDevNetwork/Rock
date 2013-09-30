<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.TagDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <Rock:HighlightLabel ID="hlEntityType" runat="server" LabelType="Type" />
            </div>
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

            <div id="pnlEditDetails" runat="server">

                <fieldset>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                            <Rock:RockRadioButtonList ID="rblScope" runat="server" Label="Scope" RepeatDirection="Horizontal"
                                AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                                <asp:ListItem Value="Public" Text="Public" Selected="True" />
                                <asp:ListItem Value="Private" Text="Private" />
                            </Rock:RockRadioButtonList>
                            <Rock:PersonPicker ID="ppOwner" runat="server" Label="Owner" />
                        </div>
                        <div class="span6">
                            <Rock:RockDropDownList id="ddlEntityType" runat="server" Label="Entity Type" />
                            <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                            <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                       </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </fieldset>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn" OnClick="btnDelete_Click" />
                </div>

            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
