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
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppOwner" runat="server" Label="Owner" />
                            <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                            <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList id="ddlEntityType" runat="server" Label="Entity Type" />
                            <Rock:RockRadioButtonList ID="rblScope" runat="server" Label="Scope" RepeatDirection="Horizontal"
                                AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                                <asp:ListItem Value="Public" Text="Public" Selected="True" />
                                <asp:ListItem Value="Private" Text="Private" />
                            </Rock:RockRadioButtonList>
                       </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </fieldset>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:Literal ID="lblMainDetails" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                </div>

            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
