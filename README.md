# NN Planning

## Requirements
- [ ] NeuralNetwork.Generic library
- [ ] Stock Analysis Neural Network (SANNET) Application
- [ ] SANNET.DataModel Library

### NeuralNetwork.Generic
Blah..

### SANNET Application
The SANNET Application should perform the following items:
1. Determine which stocks should be included in the analysis (database table with company and flag?)
1. Construct & train Neural Network (NN) for EACH company
  * Blah
  * Blah2
1. Store or display results.

### SANNET Database
Tables, views, and stored procedures that should reside in the SANNET.DataModel library

#### Tables
Neural net stuff?

#### Stored Procedures
- [ ] Technical Indicator Stored Procedure (needs companyId and indicator value arguments)
```SQL
-- Selects all quote information and also the close from 2 days ago --> Very useful for technical indicators.
SELECT Id, CompanyId, Date, [Open], High, Low, [Close], Volume, LastModifiedDate, LAG([Close], 2) OVER (ORDER BY Date) AS TwoDaysAgoClose
FROM StockMarketData.dbo.Quotes
WHERE CompanyId = 2
ORDER BY Date
```
