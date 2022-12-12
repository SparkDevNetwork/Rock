<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormSubmissionList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormSubmissionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block styled-scroll">

            <div class="panel-heading panel-follow">
                <h1 class="panel-title"><i class="fa fa-poll-h"></i>&nbsp;<asp:Literal ID="lTitle" runat="server" /></h1>

                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

                <div class="panel-toolbar panel-toolbar-shadow">
                    <ul class="nav nav-pills nav-sm">
                        <li id="tabSubmissions" runat="server" class="active">
                            <asp:LinkButton ID="lnkSubmissions" runat="server" Text="Submissions" CssClass="show-pill" OnClick="lnkSubmissions_Click" pill="submissions-tab" />
                        </li>
                        <li id="tabFormBuilder" runat="server">
                            <asp:LinkButton ID="lnkFormBuilder" runat="server" Text="Form Builder" CssClass="show-pill" OnClick="lnkFormBuilder_Click" pill="formBuilder-tab" />
                        </li>
                        <li id="tabCommunications" runat="server">
                            <asp:LinkButton ID="lnkComminucations" runat="server" Text="Communications" CssClass="show-pill" OnClick="lnkComminucations_Click" pill="communications-tab" />
                        </li>
                        <li id="tabSettings" runat="server">
                            <asp:LinkButton ID="lnkSettings" runat="server" Text="Settings" CssClass="show-pill" OnClick="lnkSettings_Click" pill="settings-tab" />
                        </li>
                        <li id="tabAnalytics" runat="server">
                            <asp:LinkButton ID="lnkAnalytics" runat="server" Text="Analytics" CssClass="show-pill" OnClick="lnkAnalytics_Click" pill="analytics-tab" />
                        </li>
                    </ul>
                </div>

            <div class="panel-body">

                <div class="rock-header">
                    <h3 class="title">Submissions</h3>
                    <div class="description">
                        Below is a listing of submissions for this form.
                    </div>
                    <hr class="section-header-hr" />
                </div>

                <div class="grid">
                    <Rock:GridFilter ID="gfWorkflows" runat="server">
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" PersonIdField="PersonId" OnRowSelected="gWorkflows_RowSelected" EnableStickyHeaders="true" RowItemText="Submission">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:DateField DataField="ActivatedDateTime" HeaderText="Submitted" SortExpression="ActivatedDateTime" />
                            <Rock:CampusField DataField="Campus" HeaderText="Campus" SortExpression="Campus.Name" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize('body');
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
