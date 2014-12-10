<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PhotoVerify.ascx.cs" Inherits="RockWeb.Blocks.Crm.PhotoVerify" %>
<%@ Import Namespace="Rock.Web.UI.Controls" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading text-right">
                <h1 class="panel-title"><i class="fa fa-camera"></i> Verify Photo List</h1>
                <Rock:RockCheckBox runat="server" ID="cbShowAll" Text="show verified photos" OnCheckedChanged="cbShowAll_CheckedChanged" AutoPostBack="true" />
            </div>
            <div class="panel-body">
                <Rock:NotificationBox  runat="server" ID="nbConfigError" NotificationBoxType="Danger" Text="Block must be configured for a particular group."
                    Visible="false"></Rock:NotificationBox>
                <Rock:NotificationBox runat="server" ID="nbMessage" CssClass="margin-b-lg" NotificationBoxType="Success" Visible="false"></Rock:NotificationBox>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" OnRowDataBound="gList_RowDataBound" OnRowSelected="gList_RowSelected" DataKeyNames="PersonId" EmptyDataText="No one to show." RowItemText="photo">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockTemplateField >
                                <HeaderTemplate>
                                    Photo
                                </HeaderTemplate>
                                <itemtemplate>
                                <a href='<%# FormatPersonLink(Eval("PersonId").ToString()) %>'>
                                    <img class="person-image" id="imgPersonImage" src="" runat="server"/>
                                </a>
                                </itemtemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" />
                            <Rock:RockBoundField DataField="Person.FullName" HeaderText="Name" SortExpression="Person.FullName" />
                            <Rock:RockBoundField DataField="Person.Gender" HeaderText="Gender" SortExpression="Person.Gender" />
                            <Rock:RockBoundField DataField="Person.Email" HeaderText="Email" SortExpression="Person.Email" />
                            <Rock:RockTemplateField>
                                <HeaderTemplate>Status
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Literal runat="server" ID="lStatus"></asp:Literal>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="rGrid_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
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