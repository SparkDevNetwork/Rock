<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlGroupMembers" runat="server">

                <div class="panel panel-block">
                
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Members" />
                        </h1>

                        <div class="panel-labels">
                            <asp:HyperLink ID="hlSyncSource" runat="server"><Rock:HighlightLabel ID="hlSyncStatus" runat="server" LabelType="Info" Visible="false" Text="<i class='fa fa-exchange'></i>" /></asp:HyperLink> &nbsp;
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbRoleWarning" runat="server" CssClass="alert-grid" NotificationBoxType="Warning" Title="No roles!" Visible="false" />
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                                <Rock:RockCheckBoxList ID="cblGroupMemberStatus" runat="server" Label="Group Member Status" RepeatDirection="Horizontal" />
                                <Rock:CampusPicker ID="cpCampusFilter" runat="server" Label="Family Campus" />
                                <Rock:RockCheckBoxList ID="cblGenderFilter" runat="server" RepeatDirection="Horizontal" Label="Gender">
                                    <asp:ListItem Text="Male" Value="Male" />
                                    <asp:ListItem Text="Female" Value="Female" />
                                    <asp:ListItem Text="Unknown" Value="Unknown" />
                                </Rock:RockCheckBoxList>
                                <Rock:RockDropDownList ID="ddlRegistration" runat="server" Label="Registration" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockDropDownList ID="ddlSignedDocument" runat="server" Label="Signed Document" >
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Yes" Value="Yes" />
                                    <asp:ListItem Text="No" Value="No" />
                                </Rock:RockDropDownList>
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gGroupMembers_Edit" CssClass="js-grid-group-members" >
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Person.LastName,Person.NickName" HtmlEncode="false" />
                                    <Rock:DefinedValueField DataField="MaritalStatusValueId" HeaderText="Marital Status" SortExpression="Person.MaritalStatusValue.Value"/>
                                    <Rock:DefinedValueField DataField="ConnectionStatusValueId" HeaderText="Connection Status" SortExpression="Person.ConnectionStatusValue.Value"/>
                                    <Rock:RockTemplateFieldUnselected HeaderText="Registration">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistration" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <Rock:RockBoundField DataField="GroupRole" HeaderText="Role" SortExpression="GroupRole.Name" />
                                    <Rock:RockBoundField DataField="GroupMemberStatus" HeaderText="Member Status" SortExpression="GroupMemberStatus" />
                                    <Rock:DateField DataField="DateTimeAdded" HeaderText="Date Added" SortExpression="DateTimeAdded" />
                                    <Rock:DateField DataField="FirstAttended" HeaderText="First Attended" SortExpression="FirstAttended" />
                                    <Rock:DateField DataField="LastAttended" HeaderText="Last Attended" SortExpression="LastAttended" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

             <script>
                Sys.Application.add_load( function () {
                    $("div.photo-icon").lazyload({
                        effect: "fadeIn"
                    });
                });
            </script>

            <Rock:ModalDialog ID="mdPlaceElsewhere" runat="server" Visible="false" ValidationGroup="vgPlaceElsewhere"
                Title="<i class='fa fa-share'></i> Place Elsewhere" OnSaveClick="mdPlaceElsewhere_SaveClick"
                SaveButtonText="Place">
                <Content>
                    <asp:ValidationSummary ID="vsPlaceElsewhere" runat="server" ValidationGroup="vgPlaceElsewhere" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:RockLiteral ID="lWorkflowTriggerName" runat="server" Label="Workflow Trigger" />
                    <Rock:RockControlWrapper ID="rcwSelectMemberTrigger" runat="server" Label="Select Workflow Trigger">
                        <Rock:HiddenFieldWithClass ID="hfPlaceElsewhereTriggerId" CssClass="js-hidden-selected" runat="server" />
                        <div class="controls">
                            <div class="btn-group-vertical">
                                <asp:Repeater ID="rptSelectMemberTrigger" runat="server" OnItemDataBound="rptSelectMemberTrigger_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnMemberTrigger" runat="server" CssClass="btn btn-default" CausesValidation="false" Text='<%# Eval("Name") %>' OnClick="btnMemberTrigger_Click" CommandArgument='<%# Eval("Id") %>' CommandName="TriggerId" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </Rock:RockControlWrapper>
                    <Rock:NotificationBox ID="nbPlaceElsewhereWarning" runat="server" NotificationBoxType="Warning" />
                    <asp:HiddenField ID="hfPlaceElsewhereGroupMemberId" runat="server" />
                    <Rock:RockLiteral ID="lPlaceElsewhereGroupMemberName" runat="server" Label="Group Member" />
                    <Rock:RockLiteral ID="lWorkflowName" runat="server" Label="Workflow" />
                    <Rock:RockTextBox ID="tbPlaceElsewhereNote" runat="server" Label="Note" Rows="4" TextMode="MultiLine" ValidationGroup="vgPlaceElsewhere" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
