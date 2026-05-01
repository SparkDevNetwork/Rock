# Finance Documentation

The Finance domain is Rock's giving, batch, account, pledge, scheduled-transaction, statement, and benevolence system. The bulk of financial value lives in `FinancialTransaction` -> `FinancialTransactionDetail` -> `FinancialAccount` (one transaction, many details, each routed to an account) and `FinancialBatch` -> `FinancialTransaction` (a batch wraps a set of transactions for reconciliation).

If you are new to the domain, start with [finance-overview.md](finance-overview.md). Per-subsystem docs (transactions, batches, accounts, pledges, scheduled transactions, gateways, statements, benevolence) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Accounts and Campus Mapping](accounts-and-campus-mapping.md) | Chart of accounts, the parent-child tree, save-time campus mapping, the self-parent fix. |
| [Benevolence](benevolence.md) | `BenevolenceRequest`/`BenevolenceResult`/`BenevolenceType` model, free-form vs linked requesters, workflow triggers, document attachments. |
| [Finance Domain Overview](finance-overview.md) | Top-level mental model, key entities, save-hook behavior, and the immutable-batch reconciliation rule. |
| [Gateways and Payments](gateways-and-payments.md) | `GatewayComponent` extension point, `FinancialPaymentDetail` masking, saved accounts (tokens), webhook flow. |
| [Pledges and Statements](pledges-and-statements.md) | Loose-coupled pledges, the Statement Generator, Lava-rendered templates, independent pledge / giving date filters. |
| [Scheduled Transactions](scheduled-transactions.md) | Recurring giving, the gateway-as-scheduler model, payment plans, mobile saved-account flow. |
| [Transactions and Batches](transactions-and-batches.md) | Transaction-vs-detail allocation, batch reconciliation, refunds, the closed-batch-immutability convention. |
