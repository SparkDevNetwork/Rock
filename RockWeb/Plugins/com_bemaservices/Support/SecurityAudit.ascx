<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecurityAudit.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Support.SecurityAudit" %>
<style>
    .header-success {
        background-color: #16c98d !important;
    }

    .header-warning {
        background-color: #ffc870 !important;
    }

    .header-danger {
        background-color: #d4442e !important;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <h3>
            <asp:Literal ID="lChecksPassed" runat="server" /></h3>
        <div id="divProgressBar" runat="server" class="progress" style="background-color: #dcdcdc;">
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderRockAdmin" runat="server" data-toggle="collapse" data-target="#divBodyRockAdmin">
            </div>

            <div id="divBodyRockAdmin" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionRockAdmin" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gRockAdmins" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Person.FullName" HeaderText="Name" SortExpression="FullName" />
                            <Rock:RockBoundField DataField="DateTimeAdded" HeaderText="Date Added" SortExpression="AddedDateTime" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderSslEnabled" runat="server" data-toggle="collapse" data-target="#divBodySslEnabled">
            </div>

            <div id="divBodySslEnabled" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionSslEnabled" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gSslEnabled" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                            <Rock:RockBoundField DataField="Layout.Site.Name" HeaderText="Site" SortExpression="Site" />
                            <Rock:RockBoundField DataField="PageTitle" HeaderText="Name" SortExpression="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderNonStaff" runat="server" data-toggle="collapse" data-target="#divBodyNonStaff">
            </div>

            <div id="divBodyNonStaff" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionNonStaff" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gNonStaff" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Person.Id" HeaderText="PersonId" SortExpression="PersonId" />
                            <Rock:RockBoundField DataField="Person.LastName" HeaderText="LastName" SortExpression="LastName" />
                            <Rock:RockBoundField DataField="Person.FirstName" HeaderText="FirstName" SortExpression="FirstName" />
                            <Rock:RockBoundField DataField="Group.Name" HeaderText="Security Role" SortExpression="Group" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderPageParameterSql" runat="server" data-toggle="collapse" data-target="#divBodyPageParameterSql">
            </div>

            <div id="divBodyPageParameterSql" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionPageParameterSql" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gPageParameterSql" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="PageId" HeaderText="Page Id" SortExpression="PageId" />
                            <Rock:RockBoundField DataField="PageTitle" HeaderText="Page Title" SortExpression="PageTitle" />
                            <Rock:RockBoundField DataField="BlockId" HeaderText="Block Id" SortExpression="BlockId" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Block Name" SortExpression="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderSqlLavaCommand" runat="server" data-toggle="collapse" data-target="#divBodySqlLavaCommand">
            </div>

            <div id="divBodySqlLavaCommand" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionSqlLavaCommand" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gSqlLavaCommand" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="PageId" HeaderText="Page Id" SortExpression="PageId" />
                            <Rock:RockBoundField DataField="PageTitle" HeaderText="Page Title" SortExpression="PageTitle" />
                            <Rock:RockBoundField DataField="BlockId" HeaderText="Block Id" SortExpression="BlockId" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Block Name" SortExpression="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderPersonAuth" runat="server" data-toggle="collapse" data-target="#divBodyPersonAuth">
            </div>

            <div id="divBodyPersonAuth" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionPersonAuth" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gPersonAuth" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="PersonAlias.Person.Id" HeaderText="PersonId" SortExpression="PersonId" />
                            <Rock:RockBoundField DataField="PersonAlias.Person.LastName" HeaderText="LastName" SortExpression="LastName" />
                            <Rock:RockBoundField DataField="PersonAlias.Person.FirstName" HeaderText="FirstName" SortExpression="FirstName" />
                            <Rock:RockBoundField DataField="EntityType.Name" HeaderText="Entity Type" SortExpression="EntityType" />
                            <Rock:RockBoundField DataField="EntityId" HeaderText="Entity Id" SortExpression="EntityId" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderUnencryptedSensitiveData" runat="server" data-toggle="collapse" data-target="#divBodyUnencryptedSensitiveData">
            </div>

            <div id="divBodyUnencryptedSensitiveData" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionUnencryptedSensitiveData" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gUnencryptedSensitiveData" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Attribute Id" SortExpression="AttributeId" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Attribute Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="EntityType" HeaderText="Entity Type" SortExpression="EntityType" />
                            <Rock:RockBoundField DataField="EntityTypeQualifierColumn" HeaderText="Entity Type Qualifier Column" SortExpression="EntityTypeQualifierColumn" />
                            <Rock:RockBoundField DataField="EntityTypeQualifierValue" HeaderText="Entity Type Qualifier Value" SortExpression="EntityTypeQualifierValue" />
                            <Rock:RockBoundField DataField="SensitiveRecords" HeaderText="Sensitive Records" SortExpression="SensitiveRecords" />

                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
