<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PostUserNotification.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.Misc.PostUserNotification" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment"></i> Post Notification</h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlPost" runat="server" Visible="true">
                    <Rock:RockRadioButtonList ID="rblSource" runat="server" Label="Source" Help="Where to get the list of individuals from." RepeatDirection="Horizontal" OnSelectedIndexChanged="rblSource_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false">
                        <asp:ListItem Value="Manual Selection" Text="Manual Selection" Selected="True"></asp:ListItem>
                        <asp:ListItem Value="Group" Text="Group"></asp:ListItem>
                        <asp:ListItem Value="Data View" Text="Data View"></asp:ListItem>
                    </Rock:RockRadioButtonList>

                    <asp:Panel ID="pnlManualSelection" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Add Person" OnSelectPerson="ppPerson_SelectPerson" />
                            </div>

                            <div class="col-md-6">
                                <asp:Repeater ID="rptPeople" runat="server">
                                    <ItemTemplate>
                                        <div>
                                            <span><%# Eval( "Name" ) %></span>
                                            <asp:LinkButton ID="lbRemovePerson" runat="server" OnCommand="lbRemovePerson_Command" CommandArgument='<%# Eval( "Id" ) %>'>
                                                <i class="fa fa-times text-danger"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlGroup" runat="server" Visible="false">
                        <Rock:GroupPicker ID="gpGroup" runat="server" Label="Group" Help="All active members of this group will receive the notification." OnSelectItem="gpGroup_SelectItem" />
                    </asp:Panel>

                    <asp:Panel ID="pnlDataView" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataViewItemPicker ID="dvDataView" runat="server" Label="Data View" OnSelectedIndexChanged="dvDataView_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                            </div>
                            
                            <div class="col-md-6"></div>
                        </div>
                    </asp:Panel>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NotificationBox ID="nbCount" runat="server" NotificationBoxType="Info"></Rock:NotificationBox>
                        </div>

                        <div class="col-md-6"></div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" Help="The title will be displayed at the top of the notification." Required="true" />
                            <Rock:RockDropDownList ID="ddlClassification" runat="server" Label="Classification" Required="true" />
                        </div>

                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbIconCssClass" runat="server" Label="Icon Css Class" />
                        </div>
                    </div>

                    <Rock:CodeEditor ID="ceMessage" runat="server" Label="Message" EditorMode="Html" EditorTheme="Rock" Required="true" />

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlResults" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbResults" runat="server" NotificationBoxType="Success">
                        Notification has been posted.
                    </Rock:NotificationBox>

                    <asp:LinkButton ID="lbDone" runat="server" Text="Done" CssClass="btn btn-link" OnClick="lbDone_Click" />
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
