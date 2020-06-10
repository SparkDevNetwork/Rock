<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecurityAudit.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.Bema.Reporting.SecurityAudit" %>
<style>
    .header-success {
        background-color: #dff0d8 !important;
    }

    .header-warning {
        background-color: #fcf8e3 !important;
    }

    .header-danger {
        background-color: #f2dede !important;
    }

    .heading-thin {
        padding-top: 5px;
        padding-bottom: 5px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <h2>
            <asp:Literal ID="lChecksPassed" runat="server" /></h2>
        <div id="divProgressBar" runat="server" class="progress" style="background-color: #dcdcdc;">
        </div>

        <h3>Rock Permissions</h3>
        <div class="well" style="background-color: white;">
            <div class="panel panel-widget rock-panel-widget">
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderRockAdmin" runat="server" data-toggle="collapse" data-target="#divBodyRockAdmin">
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
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderNonStaff" runat="server" data-toggle="collapse" data-target="#divBodyNonStaff">
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
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderPersonAuth" runat="server" data-toggle="collapse" data-target="#divBodyPersonAuth">
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
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderFinanceDataViews" runat="server" data-toggle="collapse" data-target="#divBodyFinanceDataViews">
                </div>

                <div id="divBodyFinanceDataViews" class="collapse panel-body">
                    <div class="alert alert-info">
                        <p>
                            <asp:Literal ID="lDescriptionFinanceDataViews" runat="server" />
                        </p>
                    </div>
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gFinanceDataViews" runat="server" AllowSorting="true">
                            <Columns>
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
            <div class="panel panel-widget rock-panel-widget">
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderFinancePages" runat="server" data-toggle="collapse" data-target="#divBodyFinancePages">
                </div>

                <div id="divBodyFinancePages" class="collapse panel-body">
                    <div class="alert alert-info">
                        <p>
                            <asp:Literal ID="lDescriptionFinancePages" runat="server" />
                        </p>
                    </div>
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gFinancePages" runat="server" AllowSorting="true">
                            <Columns>
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                                <Rock:RockBoundField DataField="PageTitle" HeaderText="Name" SortExpression="Name" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
            <div class="panel panel-widget rock-panel-widget">
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderAdminPages" runat="server" data-toggle="collapse" data-target="#divBodyAdminPages">
                </div>

                <div id="divBodyAdminPages" class="collapse panel-body">
                    <div class="alert alert-info">
                        <p>
                            <asp:Literal ID="lDescriptionAdminPages" runat="server" />
                        </p>
                    </div>
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gAdminPages" runat="server" AllowSorting="true">
                            <Columns>
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                                <Rock:RockBoundField DataField="PageTitle" HeaderText="Name" SortExpression="Name" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
            <div class="panel panel-widget rock-panel-widget">
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderFileTypeSecurity" runat="server" data-toggle="collapse" data-target="#divBodyFileTypeSecurity">
                </div>

                <div id="divBodyFileTypeSecurity" class="collapse panel-body">
                    <div class="alert alert-info">
                        <p>
                            <asp:Literal ID="lDescriptionFileTypeSecurity" runat="server" />
                        </p>
                    </div>
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gFileTypeSecurity" runat="server" AllowSorting="true">
                            <Columns>
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </div>

        <h3>Website Security</h3>
        <div class="well" style="background-color: white;">
            <div class="panel panel-widget rock-panel-widget">
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderSslEnabled" runat="server" data-toggle="collapse" data-target="#divBodySslEnabled">
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
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderPageParameterSql" runat="server" data-toggle="collapse" data-target="#divBodyPageParameterSql">
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
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderSqlLavaCommand" runat="server" data-toggle="collapse" data-target="#divBodySqlLavaCommand">
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
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderGlobalLavaCommands" runat="server" data-toggle="collapse" data-target="#divBodyGlobalLavaCommands">
                </div>

                <div id="divBodyGlobalLavaCommands" class="collapse panel-body">
                    <div class="alert alert-info">
                        <p>
                            <asp:Literal ID="lDescriptionGlobalLavaCommands" runat="server" />
                        </p>
                    </div>
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gGlobalLavaCommands" runat="server" AllowSorting="true">
                            <Columns>
                                <asp:TemplateField HeaderText="Globally Enabled Lava Command">
                                    <ItemTemplate>
                                        <%#Container.DataItem %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
            <div class="panel panel-widget rock-panel-widget">
                <div class="panel-heading heading-thin clearfix clickable" id="divHeaderUnencryptedSensitiveData" runat="server" data-toggle="collapse" data-target="#divBodyUnencryptedSensitiveData">
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
        </div>




    </ContentTemplate>
</asp:UpdatePanel>
