<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Crm.PhotoVerify, RockWeb" %>
<%@ Import Namespace="Rock.Web.UI.Controls" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading text-right">
                <Rock:RockCheckBox runat="server" ID="cbShowAll" Text="show all" OnCheckedChanged="cbShowAll_CheckedChanged" AutoPostBack="true" />
            </div>
            <div class="panel-body">
                <Rock:NotificationBox  runat="server" ID="nbConfigError" NotificationBoxType="Danger" Text="Block must be configured for a particular group."
                    Visible="false"></Rock:NotificationBox>
                <Rock:NotificationBox runat="server" ID="nbMessage" NotificationBoxType="Success" Visible="false"></Rock:NotificationBox>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" OnRowDataBound="gList_RowDataBound" DataKeyNames="PersonId" EmptyDataText="No one to show." RowItemText="people">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:CheckBox runat="server" ID="cbAll" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox runat="server" ID="cbSelected" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField >
                                <HeaderTemplate>
                                    Photo
                                </HeaderTemplate>
                                <itemtemplate>
                                <a href='<%# FormatPersonLink(Eval("PersonId").ToString()) %>'>
                                    <img class="person-image" id="imgPersonImage" src="" runat="server"/>
                                </a>
                                </itemtemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" />
                            <asp:BoundField DataField="Person.FullName" HeaderText="Name" SortExpression="Person.FullName" />
                            <asp:TemplateField>
                                <HeaderTemplate>Status
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <i runat="server" ID="iStatus" data-toggle="tooltip" class="flag"></i>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Person.Gender" HeaderText="Gender" SortExpression="Person.Gender" />
                            <asp:BoundField DataField="Person.Email" HeaderText="Email" SortExpression="Person.Email" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
            <div class="actions margin-h-md margin-v-md">
                <Rock:BootstrapButton ID="bbtnVerify" runat="server" Text="Verify" CssClass="btn btn-primary" OnClick="bbtnVerify_Click"></Rock:BootstrapButton>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">

    var allCheckBoxSelector = '#<%=gList.ClientID%> input[id*="cbAll"]:checkbox';
    var checkBoxSelector = '#<%=gList.ClientID%> input[id*="cbSelected"]:checkbox';

    function ToggleCheckUncheckAllOptionAsNeeded() {
        var allCheckboxes = $(checkBoxSelector),
            checkedCheckboxes = allCheckboxes.filter(":checked"),
            noCheckboxesAreChecked = (checkedCheckboxes.length === 0),
            allCheckboxesAreChecked = (allCheckboxes.length === checkedCheckboxes.length);
        if (allCheckboxes.length === 0) {
            $(allCheckBoxSelector).hide();
        }
        else {
            $(allCheckBoxSelector).prop('checked', allCheckboxesAreChecked);
        }
    }

    function PerformCheck() {
        $("#<%=gList.ClientID%>").on('click', 'input[id*="cbAll"]:checkbox', function () {
            $(checkBoxSelector).attr('checked', $(this).is(':checked'));
            $(checkBoxSelector).prop('checked', $(this).is(':checked'));
            ToggleCheckUncheckAllOptionAsNeeded();
        });

        $("#<%=gList.ClientID%>").on('click', 'input[id*="cbSelected"]:checkbox', ToggleCheckUncheckAllOptionAsNeeded);
        ToggleCheckUncheckAllOptionAsNeeded();
    }

    //$(document).ready(function () {
    //    PerformCheck()
    //});

    Sys.Application.add_load(function () {
        PerformCheck();
        $('.grid-table i.flag').tooltip({ html: true, container: 'body', delay: { show: 250, hide: 100 } });

    });

</script>