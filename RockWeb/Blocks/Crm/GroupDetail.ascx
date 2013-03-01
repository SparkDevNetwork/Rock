<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupDetail" %>

<asp:UpdatePanel ID="upGroupList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server" class="well">

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            <Rock:LabeledCheckBox ID="cbIsActive" runat="server" LabelText="Active" />
                            <Rock:DataDropDownList ID="ddlParentGroup" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" LabelText="Parent Group" AutoPostBack="true" OnSelectedIndexChanged="ddlParentGroup_SelectedIndexChanged" />
                            <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" LabelText="Group Type" />
                        </div>
                        <div class="span6">
                            <Rock:DataDropDownList ID="ddlCampus" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" LabelText="Campus" />
                            <Rock:LabeledCheckBox ID="cbIsSecurityRole" runat="server" LabelText="Security Role" />

                            <h5>Group Member Attributes
                            </h5>
                            <p>
                                Group member attributes allow for providing different values for each group member.
                            </p>
                            <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false">
                                <Columns>
                                    <asp:BoundField DataField="Name" />
                                    <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                    <div class="attributes">
                        <asp:PlaceHolder ID="phGroupTypeAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                    <div class="attributes">
                        <asp:PlaceHolder ID="phGroupAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <legend>
                    <asp:Literal ID="lGroupIconHtml" runat="server" />&nbsp;
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </legend>
                <asp:Literal ID="lblActiveHtml" runat="server" />
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="attributes">
                        <asp:PlaceHolder ID="phGroupTypeAttributesReadOnly" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                    <div class="attributes">
                        <asp:PlaceHolder ID="phGroupAttributesReadOnly" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-mini" OnClick="btnDelete_Click" />
                    </div>
                </div>
            </fieldset>
        </asp:Panel>

        <asp:Panel ID="pnlGroupMemberAttribute" runat="server" Visible="false">
            <Rock:RockAttributeEditor ID="edtGroupMemberAttributes" runat="server" OnSaveClick="btnSaveGroupMemberAttribute_Click" OnCancelClick="btnCancelGroupMemberAttribute_Click" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
