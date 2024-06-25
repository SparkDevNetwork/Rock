<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PdfMergerV2.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.CMS.PdfMergerV2" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block" runat="server" id="pnlMain">
            <div class="panel-heading">
                <h1 class="panel-title">PDF Merger</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Panel CssClass="alert alert-warning" runat="server" ID="pnlError" Visible="false">
                            <h4 class="alert-heading">Error</h4>
                            <asp:Literal runat="server" ID="lError" />
                        </asp:Panel>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Panel runat="server" ID="pnlStatus" Visible="false">
                            <h2 id="status-heading" class="text-center">Creating...</h2>
                            <h3 id="status-text" class="text-center">0.00% completed</h3>
                            <div id="spinner"></div>
                            <asp:HiddenField runat="server" ID="hfOutputWebPath" ClientIDMode="Static" />
                            <asp:HiddenField runat="server" ID="hfApiStatusUrl" ClientIDMode="Static" />
                        </asp:Panel>
                    </div>
                </div>
                <asp:Panel runat="server" ID="pnlEntryForm">
                    <div class="row">
                        <div class="col-md-8">
                            <Rock:RockDropDownList runat="server" ID="ddlQuery" Label="Query" />
                        </div>
                        <div class="col-md-4">
                            <Rock:Toggle runat="server" ID="tglDoubleSidedMode" Label="Double-Sided Mode"
                                Help="When this is on, extra blank pages are added when needed to allow for proper double-sided printing." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-xs-12">
                            <asp:Button runat="server" ID="btnDownload" Text="Download" OnClick="btnDownload_Click" CssClass="btn btn-primary" />
                        </div>
                    </div>
                </asp:Panel>
                <style>
                    #status-heading {
                        font-size: 32px;
                    }

                    #status-text .fa {
                        font-size: 75%;
                    }

                    #spinner {
                        border: 8px solid #3498db;
                        border-top: 8px solid transparent;
                        border-radius: 50%;
                        width: 48px;
                        height: 48px;
                        animation: spin 0.75s linear infinite;
                        margin: auto;
                    }
    
                    @keyframes spin {
                        0% { transform: rotate(0deg); }
                        100% { transform: rotate(360deg); }
                    }
                </style>
                <script>
                    var prm = Sys.WebForms.PageRequestManager.getInstance();
                    prm.add_endRequest(function () {
                        // Get data
                        const pdfUrl = document.getElementById("hfOutputWebPath")?.value;
                        const apiUrl = document.getElementById("hfApiStatusUrl")?.value;

                        // Get elements
                        const statusHeading = document.getElementById("status-heading");
                        const statusText = document.getElementById("status-text");
                        const spinner = document.getElementById("spinner");

                        // If the required data exists
                        if (pdfUrl != null && pdfUrl != "" && apiUrl != null && apiUrl != "") {
                            // Loop every half second
                            var interval = window.setInterval(async function () {
                                // Send request to status API endpoint
                                let response;
                                try {
                                    response = await fetch(apiUrl);
                                }
                                catch (e) {
                                    if (e instanceof TypeError) {
                                        statusHeading.innerHTML = "Error";
                                        statusText.innerHTML = `An error occurred while making the request. Error: ${e.name}: ${e.message}`;
                                        spinner.style.borderColor = "transparent";
                                        clearInterval(interval);
                                    }
                                    else {
                                        throw e;
                                    }
                                }
                                let body = "";
                                try {
                                    body = await response.text();
                                    body = body.replace(/^"|"$/g, '');
                                }
                                catch (e) {
                                    if (!(e instanceof SyntaxError)) {
                                        throw e;
                                    }
                                }
                                // If no active processes show link to completed PDF and stop loop
                                if (response.status == 404) {
                                    statusHeading.innerHTML = "Done!";
                                    statusText.innerHTML = `All done! Click here to go to the PDF: <a href="${pdfUrl}?no-cache=${Date.now()}" target="_blank" rel="noopener noreferrer">Generated PDF <i class="fa fa-external-link-alt"></i></a>`;
                                    spinner.style.borderColor = "transparent";
                                    clearInterval(interval);
                                }
                                // Otherwise update the status
                                else {
                                    statusText.innerHTML = body;
                                }
                            }, 500);
                        }
                    });
                </script>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
