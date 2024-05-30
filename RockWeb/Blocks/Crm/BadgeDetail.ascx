<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BadgeDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.BadgeDetail" %>

<asp:UpdatePanel ID="upBadge" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-icons"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation"  />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Badge, Rock" PropertyName="Name" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Badge, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Entity Type" Required="false" IncludeGlobalOption="true" EnhanceForLongLists="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:BadgeComponentPicker ID="compBadgeType" runat="server" ContainerType="Rock.Badge.BadgeContainer, Rock" Label="Badge Type" Required="true" OnSelectedIndexChanged="compBadgeType_SelectedIndexChanged" AutoPostBack="true"/>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="rtbQualifierColumn" runat="server" Label="Qualifier Column" Required="false" fi />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="rtbQualifierValue" runat="server" Label="Qualifier Value" Required="false" />
                    </div>

                    <div class="col-md-6">
                        <div class="attributes">
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
