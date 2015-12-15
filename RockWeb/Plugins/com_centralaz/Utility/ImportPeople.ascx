<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ImportPeople.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.ImportPeople" %>
<asp:Panel ID="pnlContent" runat="server">
    <div class="panel panel-block">
        <div class="panel panel-heading">
            Import People To Group
        </div>
        <div class="panel panel-body">
            <asp:Wizard ID="Wizard1" runat="server" ActiveStepIndex="0"
                OnFinishButtonClick="Wizard1_FinishButtonClick"
                StartNextButtonStyle-CssClass="btn btn-default"
                FinishCompleteButtonStyle-CssClass="btn btn-primary margin-l-md"
                StepNextButtonStyle-CssClass="btn btn-default  margin-l-md"
                StepPreviousButtonStyle-CssClass="btn btn-default margin-r-md" SideBarStyle-VerticalAlign="Top"
                SideBarButtonStyle-CssClass="btn btn-default" SideBarButtonStyle-Width="100%"
                FinishPreviousButtonStyle-CssClass="btn btn-default" 
                SideBarStyle-CssClass="padding-all-md  well" SideBarStyle-BackColor="#dddddd"
                NavigationStyle-CssClass="padding-all-md well"
                StepStyle-CssClass="alert "
                OnNextButtonClick="Wizard1_NextButtonClick">
                <WizardSteps>
                    <asp:WizardStep ID="WizardStep1" runat="server" Title="Upload List Data">
                        <div class="col-md-12">
                            <h3>Step 1 - Choose List</h3>
                            Upload a file (csv) containing the following three (3) required, ordered fields:<br />
                            <asp:BulletedList ID="BulletedList1" runat="server">
                                <asp:ListItem>first name</asp:ListItem>
                                <asp:ListItem>last name</asp:ListItem>
                                <asp:ListItem>email address</asp:ListItem>
                            </asp:BulletedList>
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:FileUpload ID="fuUpload" runat="server" CssClass="btn btn-primary" />
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
                                        ControlToValidate="fuUpload" CssClass="text-danger" ErrorMessage="Please select a file to upload">*</asp:RequiredFieldValidator>
                                </div>
                            </div>

                            <div class="row alert alert-danger" runat="server" id="divErrors" visible="false">
                                <div class="col-md-12">
                                    <asp:Label ID="lblErrorMessage" runat="server"
                                        Text="There was a problem with the file you are trying to upload."></asp:Label>
                                    <div>
                                        <asp:Label ID="lblErrors" runat="server" ></asp:Label>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                </div>
                            </div>
                        </div>
                    </asp:WizardStep>

                    <asp:WizardStep ID="WizardStep2" runat="server" Title="Resolve People">
                        <div class="col-md-12">
                            <h3>Step  2 - Resolve Multiple Matching Records</h3>
                            <p>
                                Resolve
                                <asp:Label ID="lblRemainingRecords" runat="server" Text="TBD" />
                                remaining ambiguous matches
                            </p>
                            <br />
                            <div class="row">
                                <div class="col-md-12" style="text-align: left">
                                    <asp:Label ID="lblRawLineData" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:RadioButtonList ID="rblPeople" runat="server"></asp:RadioButtonList>
                                </div>
                            </div>

                            <hr />
                            <p>
                                <i>* Ready to import
                                <asp:Label ID="lblReadyToImportPeople" runat="server" Text="TBD" />
                                    people.</i>
                            </p>
                            <div class="alert alert-info">
                                <b>Note:</b> There are
                                <asp:Label ID="lblUnmatchedItems" runat="server" Text="TBD" />
                                unmatched (new) items.<br />
                                There were
                                <asp:Label ID="lblCleanMatches" runat="server" Text="TBD" />
                                clean matches.<br />
                            </div>
                        </div>
                    </asp:WizardStep>

                    <asp:WizardStep ID="WizardStep3" runat="server" Title="Select Group">
                        <div class="col-md-12">
                            <h3>Step 3 - Select Group</h3>
                            <br />
                            Select a group to import the people into.<br />
                            <br />
                            <Rock:RockDropDownList ID="ddlGroupType" runat="server" Label="Group Type" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" AutoPostBack="true" />
                            <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Group Member Role" />
                            <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status" />
                            <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group" />
                        </div>
                    </asp:WizardStep>

                    <asp:WizardStep runat="server" StepType="Finish" Title="Summary">
                        <div class="col-md-12">
                            <h3>Summary</h3>
                            <div runat="server" id="divAlert" class="alert alert-warning">
                                WARNING! This tool is configured to Add unmatched people as new records to the database.
                                That means it will add them as new records to the database and then add them to the group.<br />
                                However you can check this checkbox to disable it temporarily:<br />
                                <asp:CheckBox runat="server" ID="cbDisableAutoAdd" />
                                (check to disable automatic adding)
                            </div>
                            <br />
                            <asp:Label ID="lblSummary" runat="server"></asp:Label>
                            <br />
                        </div>
                    </asp:WizardStep>

                    <asp:WizardStep runat="server" StepType="Complete" Title="Complete">
                        <div class="col-md-12">
                            <h3>Complete</h3>
                            <br />
                            <asp:Label ID="lblComplete" runat="server"
                                Text="Finished!"></asp:Label>
                            <br />
                            <br />
                            <asp:Label ID="lblCompleteMsg" runat="server"></asp:Label>
                        </div>
                    </asp:WizardStep>

                </WizardSteps>
            </asp:Wizard>
        </div>
    </div>
</asp:Panel>


