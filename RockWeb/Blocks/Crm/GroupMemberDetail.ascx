<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupMemberDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfGroupMemberId" runat="server" />

            <div id="pnlEditDetails" runat="server">
                
                <div class="banner"><h1><asp:Literal ID="lActionTitle" runat="server" /></h1></div>
                
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true"/>
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList runat="server" ID="ddlGroupRole" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupRole, Rock" PropertyName="Name" Label="Group Role" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Member Status" />
                        </div>
                    </div>

                    <div class="attributes">
                        <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-default" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <div class="banner"><h1><asp:Literal ID="lGroupIconHtml" runat="server" /> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1></div>

                <div class="row">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                </div>
                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>
                <div class="attributes">
                    <asp:PlaceHolder ID="phGroupMemberAttributesReadOnly" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                </div>

            </fieldset>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
