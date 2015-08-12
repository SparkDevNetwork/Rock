<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityGroupDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityGroupDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lGroupIconHtml" runat="server" />
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlblInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlblType" runat="server" LabelType="Type" />
                        <Rock:HighlightLabel ID="hlblCampus" runat="server" LabelType="Campus" />
                    </div>
                </div>

                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbRoleLimitWarning" runat="server" NotificationBoxType="Warning" Heading="Role Limit Warning" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="dtbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" Label="Group Type" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="dtbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>

                        <asp:PlaceHolder ID="phGroupAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>

                        <div class="actions">
                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                        </div>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">

                        <p class="description">
                            <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal>
                        </p>

                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lMainDetails" runat="server" />
                                <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                            </div>
                            <div class="col-md-6 location-maps">
                                <asp:PlaceHolder ID="phMaps" runat="server" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="maDeleteWarning" runat="server" />
                            <span class="pull-right">
                                <asp:HyperLink ID="hlMap" runat="server" CssClass="btn btn-sm btn-default" ToolTip="Interactive Map"><i class="fa fa-map-marker"></i></asp:HyperLink>
                                <Rock:SecurityButton ID="sbtnSecurity" runat="server" class="btn btn-sm btn-security" Title="Secure Group" />
                            </span>
                        </div>

                    </fieldset>
                </div>
            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
