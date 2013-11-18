<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupDetail" %>

<asp:UpdatePanel ID="upGroupList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lGroupIconHtml" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />

            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <Rock:NotificationBox ID="nbRoleLimitWarning" runat="server" NotificationBoxType="Warning" Heading="Role Limit Warning" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpParentGroup" runat="server" Required="false" Label="Parent Group" OnSelectItem="ddlParentGroup_SelectedIndexChanged" />
                        <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" Label="Group Type" />
                        <Rock:DataDropDownList ID="ddlCampus" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" Label="Campus" />
                        <Rock:RockCheckBox ID="cbIsSecurityRole" runat="server" Text="Security Role"  />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributesInherited" runat="server" Label="Inherited Group Member Attributes" 
                            Help="Inherited Attributes for members in this group.">
                            <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <span class='text-muted'>
                                                <%# Eval("Name") %><br/>
                                                <small>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a> Group Type)</small>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributes" runat="server" Label="Group Member Attributes" 
                            Help="Attributes for members in this group. Each member can have different values (i.e. Hours Serving) that are set on the member detail.">
                            <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <asp:BoundField DataField="Name" />
                                    <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <asp:PlaceHolder ID="phGroupAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                    <div class="col-md-6">
                        <asp:PlaceHolder ID="phGroupTypeAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn  btn-sm" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <p class="description">
                    <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal></p>

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" OnClick="btnEdit_Click" CausesValidation="false" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-action btn-sm" OnClick="btnDelete_Click" CausesValidation="false" />
                </div>

            </fieldset>
        </asp:Panel>

        <asp:HiddenField ID="hfAttributeId" runat="server" />
        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberAttribute" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
