<%@ Control AutoEventWireup="true" CodeFile="SupportRequestList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Support.SupportRequestList" Language="C#" %>

<asp:UpdatePanel ID="upnlError" runat="server" Visible="false">
    <ContentTemplate>
        <asp:Literal ID="lErrors" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    .btn-excelexport {
        visibility: hidden;
    }
    .btn-merge-template {
        visibility: hidden;
    }
</style>

<asp:UpdatePanel ID="upnlFirstTimeSetup" runat="server" Visible="false">
    <ContentTemplate>
        <div class="container">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h4 class="panel-title"><i class="fa fa-phone"></i>&nbsp;BEMA Services</h4>
                </div>
                <div class="panel-body">
                    
                    <img class="center-block" style="width: 400px; padding-bottom: 5px;" src="/Plugins/com_bemaservices/Support/Assets/Images/BEMALogo.svg" />
                    
                    <asp:Panel ID="pnlSignupDetails" runat="server">
                        <!--

                        <div class="row">
                            <div class="col-md-12">
                                <p class="margin-v-md">
                                    Love Rock but wish there was someone to reach out to when things just don't go as planned, or you just need an extra helping hand? 
                                    Our highly trained Rock Support Specialists are available Monday - Friday to help your ministries have access to the quick help they need.
                                </p>
                                <p class="margin-v-md">
                                    BEMA offers support for Rock RMS starting at only $500 per month.
                                </p>
                                <p class="margin-v-md">
                                     This plugin provides the ability to submit a support request and manage your support requests straight from within your own Rock environment.
                                </p>
                                <p class="margin-v-md">
                                     We strive for responses within 2 business hours, and if a solution needs more than just a ticket to fix, our support specialists will give you a call to get more information and help resolve the problem.
                                </p>
                                <p class="margin-v-md">
                                    Problems our Support Specialist can help solve include:<br />
                                    <ul>
                                        <li>General Use Questions</li>
                                        <li>Configuration & Setup</li>
                                        <li>Dataviews & Reporting</li>
                                        <li>Check-In</li>
                                        <li>Event Registrations</li>
                                        <li>Connections</li>
                                        <li>Troubleshooting</li>
                                    </ul>
                                </p>
                                <br />
                            </div>
                        </div>
                        -->
                        <div class="well">
                            <h3 style="text-align: center;">Sign-up Form</h3>
                            <br />
                            <div class="row">
                                <div class="col-md-12" style="padding-bottom: 20px;" >Please provide the staff contact information for the person that will be responsible for signing the Rock Support contact.</div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:EmailBox ID="ebEmail" runat="server" Label="Email" Required="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbJobTitle" runat="server" Label="Job Title" Required="true" />
                                </div>

                            </div>

                            <hr />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbOrganization" runat="server" Label="Church Name" Required="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:PhoneNumberBox ID="pnContactNumber" runat="server" Label="Contact Number" Required="true" />
                                </div>
                                <div class="col-md-12">
                                    <Rock:AddressControl ID="acAddress" runat="server" Label="Church Address" Required="true" />
                                </div>
                                <div class="col-md-12">
                                    <asp:LinkButton ID="lbRegister" runat="server" OnClick="lbRegister_Click" CssClass="btn btn-primary center-block margin-t-md" Text="Get Started" />
                                </div>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Literal ID="lSignupAlerts" runat="server"></asp:Literal>
                </div>
            </div>
        </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upnlHeader" runat="server" Visible="true">
    <ContentTemplate>
        <div class="row" style="padding-bottom: 20px;">
            <div class="col-md-8">
                <h2 style="margin-top: 0;"><strong>BEMA</strong> Support Requests</h2>
            </div>
            <div class="col-md-4">
                <Rock:BootstrapButton runat="server" ID="btnCreateSupportRequest" OnClick="btnCreateSupportRequest_Click" CssClass="btn btn-primary pull-right">Create Support Request</Rock:BootstrapButton>
            </div>
        </div>
        <asp:Literal ID="lAlerts" runat="server"></asp:Literal>

        <asp:UpdatePanel ID="upnlContent" runat="server" Visible="false">
            <ContentTemplate>

                <ul class="nav nav-pills margin-b-md">
                    <li class="active" id="lbMySupportRequestsParent" runat="server">
                        <asp:LinkButton runat="server" ID="lbMySupportRequests" OnClick="lbMySupportRequests_Click">My Support Requests</asp:LinkButton>
                    </li>
                    <li id="lbMyCompletedSupportRequestParent" runat="server">
                        <asp:LinkButton runat="server" ID="lbMyCompletedSupportRequest" OnClick="lbMyCompletedSupportRequest_Click">Completed Support Requests</asp:LinkButton>
                    </li>
                    <li id="lbAllSupportRequestParent" runat="server">
                        <asp:LinkButton runat="server" ID="lbAllSupportRequest" OnClick="lbAllSupportRequest_Click">All Support Request</asp:LinkButton>
                    </li>
                </ul>

                <asp:Panel ID="pnlSupportRequests" runat="server" Title="My Support Requests" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <asp:Literal ID="lGridTitle" runat="server" Text="Support Requests" /></h1>
                    </div>
                    <div class="panel-body">
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="gfSupportRequests" runat="server">
                                <Rock:RockTextBox ID="rtbFilterSubject" runat="server" Label="Subject"></Rock:RockTextBox>
                                <Rock:RockCheckBoxList ID="cblFilterUrgency" runat="server" Label="Urgency" RepeatDirection="Vertical">
                                    <asp:ListItem Selected="False" Text="Normal" Value="Normal" />
                                    <asp:ListItem Selected="False" Text="Emergency" Value="Emergency" />
                                </Rock:RockCheckBoxList>
                                <Rock:RockTextBox ID="rtbFilterSubmitter" runat="server" Label="Submitter"></Rock:RockTextBox>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gSupportRequests" runat="server" AllowSorting="false" OnRowSelected="gSupportRequests_RowSelected" DataKeyNames="Id">
                                <Columns>
                                    <Rock:RockBoundField DataField="Subject" HtmlEncode="false" HeaderText="Subject" SortExpression="Subject" />
                                    <Rock:RockBoundField DataField="Urgency" HeaderText="Urgency" SortExpression="Urgency" />
                                    <Rock:PersonField DataField="Submitter" HeaderText="Submitter" SortExpression="Submitter" />
                                    <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                                    <Rock:DateTimeField DataField="Created" HeaderText="Created&nbsp;&nbsp;" SortExpression="Created"
                                        FormatAsElapsedTime="true" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:UpdatePanel ID="upnlDetails" runat="server" Visible="false">
            <ContentTemplate>
                <asp:Literal ID="lMessageDetail" runat="server"></asp:Literal>

                <div class="row">
                    <div class="col-md-6">
                        <div class="panel panel-block">
                            <div class="panel-heading">
                                <h1 class="panel-title">
                                    <i class="fa fa-ticket-alt"></i>
                                    Support Request Detail
                                </h1>
                                <div class="panel-labels">
                                    <Rock:HighlightLabel ID="detailUrgency" runat="server" />
                                    <Rock:HighlightLabel ID="detailStatus" runat="server" />
                                </div>
                            </div>
                            <div class="panel-body">

                                <div class="form-group">
                                    <label class="control-label">Subject</label>
                                    <div class="control-wrapper">
                                        <div class="form-control-static" runat="server" id="detailSubject"></div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Description</label>
                                    <div class="control-wrapper">
                                        <div class="form-control-static" runat="server" id="detailDescription"></div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Submitter</label>
                                    <div class="control-wrapper">
                                        <div class="form-control-static" runat="server" id="detailSubmitter"></div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Email</label>
                                    <div class="control-wrapper">
                                        <div class="form-control-static" runat="server" id="detailEmail"></div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Rock Version</label>
                                    <div class="control-wrapper">
                                        <div class="form-control-static" runat="server" id="detailRockVersion"></div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label">Created</label>
                                    <div class="control-wrapper">
                                        <div class="form-control-static" runat="server" id="detailCreated"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <section class="panel panel-note">
                            <div class="panel-heading clearfix">
                                <h3 class="panel-title">
                                    <i class="fa fa-pencil"></i> Updates
                                </h3>
                                <asp:LinkButton CssClass="add-note btn btn-xs btn-action" runat="server" ID="btnAddNote" Text="" OnClick="btnAddNote_Click">
                                    <i class="fa fa-plus"></i>
                                </asp:LinkButton>
                            </div>
                            <div class="panel-body">
                                <asp:Repeater ID="rSupportRequestNotes" runat="server">
                                    <ItemTemplate>
                                        <div class="note note-usernote">
                                            <article class="clearfix rollover-container">
                                                <i class="fa fa-comment"></i>
                                                <div class="details">
                                                    <h5><%# Eval("Worker") %> <span class="date"><%# Eval("Created") %></span></h5>
                                                    <p><%# Eval("UpdateText") %></p>
                                            </article>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>

                                <div class="note-new" runat="server" visible="false" id="dNewNote">
                                    <div class="note note-personalnote">
                                        <div class="panel panel-noteentry" style="">
                                            <div class="panel-body">
                                                <div class="noteentry-control">
                                                    <Rock:RockTextBox ID="rtbUpdate" runat="server" TextMode="MultiLine" Placeholder="Update to Support Request"></Rock:RockTextBox>
                                                </div>
                                            </div>
                                            <div class="panel-footer">
                                                <Rock:BootstrapButton runat="server" ID="btnSaveUpdate" Text="Save Update" CssClass="btn btn-primary" OnClick="btnSaveUpdate_Click"></Rock:BootstrapButton>
                                                <Rock:BootstrapButton runat="server" ID="btnCancelUpdate" Text="Cancel" CssClass="btn btn-info" OnClick="btnCancelUpdate_Click"></Rock:BootstrapButton>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </section>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:UpdatePanel ID="upnlAdd" runat="server" Visible="false">
            <ContentTemplate>
                <div class="row">
                    <div class="col-md-offset-3 col-md-6">
                        <form class="form-horizontal">
                            <div class="row" style="padding-bottom: 20px;">
                                <label class="control-label col-sm-2">Subject:</label>
                                <div class="col-sm-10">
                                    <Rock:RockTextBox runat="server" ID="rtbSubject" Placeholder="Subject" TextMode="SingleLine"></Rock:RockTextBox>
                                </div>
                            </div>

                            <div class="row" style="padding-bottom: 20px;">
                                <label class="control-label col-sm-2">Description:</label>
                                <div class="col-sm-10">
                                    <Rock:RockTextBox runat="server" ID="rtbDescription" Placeholder="Description" TextMode="MultiLine"></Rock:RockTextBox>
                                </div>
                            </div>

                            <div class="row" style="padding-bottom: 20px;">
                                <label class="control-label col-sm-2">Urgency:</label>
                                <div class="col-sm-10">
                                    <Rock:RockRadioButtonList ID="ddlUrgency" runat="server"></Rock:RockRadioButtonList>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <div class="btn-toolbar pull-right">
                                        <Rock:BootstrapButton ID="btnSubmitNew" runat="server" CssClass=" btn btn-success" OnClick="btnSubmitNew_Click">Submit</Rock:BootstrapButton>
                                        <Rock:BootstrapButton ID="btnCancelNew" runat="server" CssClass="btn btn-danger" OnClick="btnCancelNew_Click">Cancel</Rock:BootstrapButton>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</asp:UpdatePanel>
