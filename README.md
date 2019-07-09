# NN Planning

## Requirements
- [ ] NeuralNetwork.Generic library
- [ ] Stock Analysis Neural Network (SANNET) Application

### SANNET Database
- [ ] Technical Indicator StoredProcedure (needs companyId and indicator value arguments)
```SQL
-- Selects all quote information and also the close from 2 days ago --> Very useful for technical indicators.
SELECT Id, CompanyId, Date, [Open], High, Low, [Close], Volume, LastModifiedDate, LAG([Close], 2) OVER (ORDER BY Date) AS TwoDaysAgoClose
FROM StockMarketData.dbo.Quotes
WHERE CompanyId = 2
ORDER BY Date
```
