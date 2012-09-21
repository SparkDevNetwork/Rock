using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Core;
using Rock.Financial;


public partial class Blocks_Administration_Financials : Rock.Web.UI.Block
{
    private TransactionService transactionService = new TransactionService();
    private FundService fundService = new FundService();
    private DefinedValueService definedValueService = new DefinedValueService();
    private bool _canConfigure = false;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        _canConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );
        if (!_canConfigure)
        {
            DisplayError("You are not authorized to configure this page");
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack && _canConfigure)
        {
            Rock.Web.UI.Page.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.core.min.js" );
            LoadDropDowns();
            BindGrid();
        }

        btnSearch.Click += delegate
        {
            TransactionSearchValue searchValue = GetSearchValue();
            var searchResults = transactionService.GetTransactionBySearch(searchValue);
            grdTransactions.DataSource = searchResults.ToList();
            grdTransactions.DataBind();
        };
    }

    private void LoadDropDowns()
    {
        ddlFundType.Items.Add(new ListItem("[All]", "-1"));
        foreach (Fund fund in fundService.Queryable())
        {
            ddlFundType.Items.Add(new ListItem(fund.Name, fund.Id.ToString()));
        }
        ddlCurrencyType.Items.Add(new ListItem("[All]", "-1"));
        foreach (DefinedValue definedValue in GetDefinedValues("Currency Type"))
        {
            ddlCurrencyType.Items.Add(new ListItem(definedValue.Name, definedValue.Id.ToString()));
        }
        ddlCreditCardType.Items.Add(new ListItem("[All]", "-1"));
        foreach (DefinedValue definedValue in GetDefinedValues("Credit Card Type"))
        {
            ddlCreditCardType.Items.Add(new ListItem(definedValue.Name, definedValue.Id.ToString()));
        }
        ddlSourceType.Items.Add(new ListItem("[All]", "-1"));
        foreach (DefinedValue definedValue in GetDefinedValues("Source Type"))
        {
            ddlSourceType.Items.Add(new ListItem(definedValue.Name, definedValue.Id.ToString()));
        }
    }

    private void BindGrid()
    {
        grdTransactions.DataSource = transactionService.GetAllTransactions();
        grdTransactions.DataBind();
    }

    private List<DefinedValue> GetDefinedValues(string definedTypeName)
    {
        List<DefinedValue> definedValues = new List<DefinedValue>();
        using (new Rock.Data.UnitOfWorkScope())     
        {
            definedValues = definedValueService
                .Queryable()
                .Where(definedValue => definedValue.DefinedType.Name == definedTypeName)
                .ToList();
        }
        return definedValues;
    }

    private TransactionSearchValue GetSearchValue()
    {
        TransactionSearchValue searchValue = new TransactionSearchValue();
        decimal? fromAmountRange = null;
        if (!String.IsNullOrEmpty(txtFromAmount.Text))
        {
            fromAmountRange = Decimal.Parse(txtFromAmount.Text);
        }
        decimal? toAmountRange = null;
        if (!String.IsNullOrEmpty(txtToAmount.Text))
        {
            toAmountRange = Decimal.Parse(txtToAmount.Text);
        }
        searchValue.AmountRange = new RangeValue<decimal?>(fromAmountRange, toAmountRange);
        if (ddlCreditCardType.SelectedValue != "-1")
        {
            searchValue.CreditCardType = definedValueService.Get(int.Parse(ddlCreditCardType.SelectedValue));            
        }
        if (ddlCurrencyType.SelectedValue != "-1")
        {
            searchValue.CurrencyType = definedValueService.Get(int.Parse(ddlCurrencyType.SelectedValue));
        }
        DateTime? fromTransactionDate = null;
        if (!String.IsNullOrEmpty(txtFromDate.Text))
        {
            fromTransactionDate = DateTime.Parse(txtFromDate.Text);
        }
        DateTime? toTransactionDate = null;
        if (!String.IsNullOrEmpty(txtToDate.Text))
        {
            toTransactionDate = DateTime.Parse(txtToDate.Text);
        }
        searchValue.DateRange = new RangeValue<DateTime?>(fromTransactionDate, toTransactionDate);
        if (ddlFundType.SelectedValue != "-1")
        {
            searchValue.Fund = fundService.Get(int.Parse(ddlFundType.SelectedValue));
        }
        searchValue.TransactionCode = txtTransactionCode.Text;
        searchValue.SourceType = definedValueService.Get(int.Parse(ddlSourceType.SelectedValue));
        return searchValue;
    }

    private void DisplayError(string message)
    {
        pnlCanConfigure.Controls.Clear();
        pnlCanConfigure.Controls.Add(new LiteralControl(message));
        pnlCanConfigure.Visible = true;
        pnlFinancialContent.Visible = false;
    }
}