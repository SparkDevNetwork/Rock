<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormSubmissionList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormSubmissionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading panel-follow">

                <div class="pull-left">
                    <h1 class="panel-title"><span class="fa fa-user"></span>&nbsp;
                    <asp:Label ID="lTitle" runat="server">My Ministry Form</asp:Label></h1>
                </div>

                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <div class="panel-body">
                <div>
                    <ul class="nav nav-pills">
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

                <hr />

                <div>
                    <h4 class="step-title text-break">Submissions</h4>
                    <div class="row">
                        <div class="col-sm-8">
                            Below is a listing of submissions for this form.
                        </div>
                    </div>
                    <hr />
                </div>

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfWorkflows" runat="server">
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" PersonIdField="PersonId" OnRowSelected="gWorkflows_RowSelected" EnableStickyHeaders="true">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:DateField DataField="ActivatedDateTime" HeaderText="Submitted" SortExpression="SubmittedDate" />
                            <Rock:CampusField DataField="Campus" HeaderText="Campus" SortExpression="Campus" />
                            <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person" />
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
