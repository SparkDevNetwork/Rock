﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonBadgeDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonBadgeDetail" %>

<asp:UpdatePanel ID="upPersonBadge" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation"  />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.PersonBadge, Rock" PropertyName="Name" Required="true" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.PersonBadge, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:ComponentPicker ID="compBadgeType" runat="server" ContainerType="Rock.PersonProfile.BadgeContainer, Rock" Label="Badge Type" Required="true" OnSelectedIndexChanged="compBadgeType_SelectedIndexChanged" AutoPostBack="true"/>
                    </div>
                    <div class="col-md-6">
                        <div class="attributes">
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
