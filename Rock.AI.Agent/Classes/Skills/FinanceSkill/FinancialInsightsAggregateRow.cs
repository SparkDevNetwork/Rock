using System;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

internal class FinancialInsightsAggregateRow
{
    public int Id { get; set; }

    public DateTime? TransactionDateTime { get; set; }

    public int? CurrencyTypeId { get; set; }

    public string CurrencyType { get; set; }

    public string Frequency { get; set; }

    public decimal AmountFiltered { get; set; }
}

