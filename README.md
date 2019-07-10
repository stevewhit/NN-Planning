# NN Planning

## Requirements
- [ ] NeuralNetwork.Generic library
- [ ] Stock Analysis Neural Network (SANNET) Application
- [ ] SANNET.DataModel Library

### SANNET Application
- Inputs: none
- Outputs: Probabilities of EACH stock that it will increase

The program should take a look at the database and see which stocks are 'marked' to be included in the analysis. After that, 

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
