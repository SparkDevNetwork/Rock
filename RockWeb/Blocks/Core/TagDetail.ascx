<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.TagDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-tag"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlEntityType" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:HiddenField ID="hfId" runat="server" />

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlEditDetails" runat="server">

                    <fieldset>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" ValidateRequestMode="Disabled" />
                                <asp:RegularExpressionValidator ID="revTagName" runat="server" ControlToValidate ="tbName" Display="None" ErrorMessage="Invalid characters have been entered for the tag name. Angle brackets, percent, and ampersand are not allowed." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlAdvanced" runat="server" CssClass="row">
                            <div class="col-md-6">
                                <Rock:CategoryPicker ID="cpCategory" runat="server" Required="false" Label="Category" EntityTypeName="Rock.Model.Tag" />
                                <Rock:RockRadioButtonList ID="rblScope" runat="server" Label="Scope" RepeatDirection="Horizontal"
                                    AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                                    <asp:ListItem Value="Organization" Text="Organizational" Selected="True" />
                                    <asp:ListItem Value="Personal" Text="Personal" />
                                </Rock:RockRadioButtonList>
                                <Rock:PersonPicker ID="ppOwner" runat="server" Label="Owner" />
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Tag, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlEntityType" runat="server" Label="Entity Type" EnhanceForLongLists="true" />
                                <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                                <Rock:ColorPicker ID="cpBackground" runat="server" Label="Background Color" Help="The background color to use when displaying tag." />
                                <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                            </div>
                        </asp:Panel>

                        <Rock:NotificationBox ID="nbEditError" runat="server" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </fieldset>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <p class="description">
                        <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockLiteral ID="lScope" runat="server" Label="Scope" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockLiteral ID="lOwner" runat="server" Label="Owner" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />

                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" Title="Secure Group" />
                        </span>
                    </div>

                </fieldset>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
