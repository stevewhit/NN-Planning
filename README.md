# NN Planning

## Requirements
- [x] NeuralNetwork.Generic library
- [ ] Stock Analysis Neural Network (SANNET) Application
- [ ] SANNET.DataModel Library

### NeuralNetwork.Generic
The idea is to have a fully abstract neural network for on-demand creation. With that, a few items need to be addressed:
- [x] Feed-forward activations
- [x] Backwards propogation

#### Feed-Forward Activation Level Calculations
```
Key
A - Activation Level of a neuron
W - Weight between each neuron
B - Bias of a neuron

Inputs: (I1, I2)
Hidden Neurons: (H1)
Outputs: (O1)

A(H1) = ActivationFunction( A(I1) * W(I1 --> H1) + A(I2) * W(I2 --> H2) + B )
```

### SANNET Application
This application should be dynamic in nature and should store or display the expected outcome for each company with a probability that it will occur. The application <i>may</i> create multiple NNs for each company; one NN for each potential trading strategy.

<b>Independent Variables</b>:
* x: # of days worth of data for training NN
* y: # of days worth of data for testing NN accuracy

The SANNET Application should perform the following items:
- [ ] Determine which stocks should be included in the analysis (database table with company and flag?)
- [ ] Construct & train Neural Network (NN) for EACH company (Possibly multiple NNs)
    * Compute and collect inputs/indicators
    * Train NN with (x) days worth of data
    * Run simulation on (y) days worth of data to figure out accuracy of NN model
    * Apply latest quote to model to get expected outcome for latest quote
- [ ] Store or display expected outcome for latest quote WITH the probability/accuracy of the model.

#### Layout
For all stocks in table marked for comparison:
 1. Verify stock exists in sm.company table. If not, run downloader for company.
 1. Gather data from stored procedure for 'x' days of training/testing.
 1. Verify stock contains 'x' days worth of quotes. If not, run downloader for quotes.
 1. Setup NN
 1. Train NN (all data between 1 week and 20 weeks ago?)
 1. Test NN (tomorrow)
 1. Test NN (week)
 1. Test NN (month)

### SANNET Database
Tables, views, and stored procedures that should reside in the SANNET.DataModel library

#### Tables
NeuralNetworkConfigurations (Id [PK], Inputs, Outputs, NumHiddenLayers, NumHiddenLayerNeurons, TrainingStartDate, TrainingEndDate, TestingStartDate, TestingEndDate, Indicators)
CompanyPredictions (Id [PK], CompanyId [FK], ConfigId [FK], Prediction)

```
Requirements
Stock needs to be volatileish
Stock price needs to be between 10 and 150???

Inputs
RSI tangent (slope) (Short-term)
RSI 2 days ago (Short-term)
RSI Yesterday (Short-term)

RSI tangent (slope) (Long-term)
RSI 2 days ago (Long-term)
RSI Yesterday (Long-term)

RSI Cross(Did they cross?) -- How long ago?

CCI 2 days ago (Short-term)
CCI Yesterday (Short-term)

CCI 2 days ago (Long-term)
CCI Yesterday (Long-term)

CCI Cross(Did they cross?) -- How long ago?

SMA Short-term
SMA long-term
SMA Cross?

The shape of the 

Outputs
Did it go up 4% within the next week BEFORE it goes down 2%
```

#### Stored Procedures
- [x] GetRSI (company, period, start & end date arguments)
- [x] GetCCI (company, period, start & end date arguments)
- [x] GetSMA (company, period, start & end date arguments)
- [ ] GetRSICross (period1, period2)
- [ ] GetTrainingDataset
   
--------------
``` SQL
-- Inputs: RSI, CCI, SMA, & all associated crosses
-- Outputs: Before the next 5 dates are up, did it:
--          1. Rise 4% or more to trigger sell?
--          2. Fall 2% or more to trigger sell?
--          3. Close at end above the Open?
--          4. Close at end below the Open?
```
``` SQL
	DECLARE @companyId INT = 2
	DECLARE @startDate DATE = '01-01-2018'
	DECLARE @endDate DATE = '01-01-2019'
	DECLARE @rsiPeriodShort INT = 7
	DECLARE @rsiPeriodLong INT = 14
	
	DECLARE @RSIShortTermTable TABLE
	(
		[QuoteId] INT, 
	    [CompanyId] INT, 
	    [Date] DATE, 
	    [RSIShort] DECIMAL(12, 4)
	)

	INSERT INTO @RSIShortTermTable
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiPeriodShort

	DECLARE @RSILongTermTable TABLE
	(
		[QuoteId] INT, 
	    [CompanyId] INT, 
	    [Date] DATE, 
	    [RSILong] DECIMAL(12, 4)
	)

	INSERT INTO @RSILongTermTable
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiPeriodLong

	SELECT short.[QuoteId], 
			short.[CompanyId], 
			short.[Date], 
			RSIShort,
			RSILong,
			CASE WHEN long.RSILong > short.RSIShort 
				 THEN NULL 
				 ELSE (SELECT MAX(long2.Date) FROM @RSILongTermTable long2 INNER JOIN @RSIShortTermTable short2 on long2.QuoteId = short2.QuoteId WHERE long2.Date <= short.Date AND long2.RSILong > short2.RSIShort)
				 END as LastDateLongAboveShort
	FROM @RSIShortTermTable short INNER JOIN @RSILongTermTable long ON short.QuoteId = long.QuoteId
```
``` SQL

	-- Dataset
	DECLARE @rsiShort TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4)
	)

	INSERT INTO @rsiShort values(0, 2, '01-01-2019', 1);
	INSERT INTO @rsiShort values(1, 2, '01-03-2019', 2);
	INSERT INTO @rsiShort values(2, 2, '01-04-2019', 3);
	INSERT INTO @rsiShort values(3, 2, '01-05-2019', 4);
	INSERT INTO @rsiShort values(4, 2, '01-06-2019', 5);
	INSERT INTO @rsiShort values(5, 2, '01-07-2019', 6);
	INSERT INTO @rsiShort values(6, 2, '01-08-2019', 5);
	INSERT INTO @rsiShort values(7, 2, '01-09-2019', 5);
	INSERT INTO @rsiShort values(8, 2, '01-10-2019', 5);
	INSERT INTO @rsiShort values(9, 2, '01-11-2019', 5);
	INSERT INTO @rsiShort values(10, 2, '01-12-2019', 5);


	DECLARE @rsiLong TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSILong] DECIMAL(12, 4)
	)

	INSERT INTO @rsiLong values(0, 2, '01-01-2019', 2);
	INSERT INTO @rsiLong values(1, 2, '01-03-2019', 1);
	INSERT INTO @rsiLong values(2, 2, '01-04-2019', 4);
	INSERT INTO @rsiLong values(3, 2, '01-05-2019', 3);
	INSERT INTO @rsiLong values(4, 2, '01-06-2019', 6);
	INSERT INTO @rsiLong values(5, 2, '01-07-2019', 7);
	INSERT INTO @rsiLong values(6, 2, '01-08-2019', 8);
	INSERT INTO @rsiLong values(7, 2, '01-09-2019', 9);
	INSERT INTO @rsiLong values(8, 2, '01-10-2019', 4);
	INSERT INTO @rsiLong values(9, 2, '01-11-2019', 4);
	INSERT INTO @rsiLong values(10, 2, '01-12-2019', 4);

	DECLARE @combinedDataset TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4),
		[RSILong] DECIMAL(12, 4),
		[ConsecutiveDaysShortAboveLong] INT,
		[ConsecutiveDaysLongAboveShort] INT
	)

	INSERT INTO @combinedDataset
	SELECT rsiShort.*,
		   rsiLong.[RSILong],
		   CASE WHEN rsiShort.RSIShort < rsiLong.RSILong
			    THEN 0
				ELSE (SELECT COUNT(*)+1 
					  FROM @rsiShort rsiShort1 
					  WHERE rsiShort1.Date < rsiShort.[Date] AND 
							rsiShort1.Date > (SELECT MAX(rsiLong2.[Date]) 
										      FROM @rsiShort rsiShort2 INNER JOIN @rsiLong rsiLong2 ON rsiShort2.QuoteId = rsiLong2.QuoteId 
										      WHERE rsiLong2.[Date] < rsiShort.[Date] AND rsiLong2.RSILong > rsiShort2.RSIShort))
				END as [ConsecutiveDaysShortAboveLong],
		   CASE WHEN rsiShort.RSIShort > rsiLong.RSILong
			    THEN 0
				ELSE (SELECT COUNT(*)+1 
					  FROM @rsiShort rsiShort1 
					  WHERE rsiShort1.Date < rsiShort.[Date] AND 
							rsiShort1.Date > (SELECT MAX(rsiLong2.[Date]) 
										      FROM @rsiShort rsiShort2 INNER JOIN @rsiLong rsiLong2 ON rsiShort2.QuoteId = rsiLong2.QuoteId 
										      WHERE rsiLong2.[Date] < rsiShort.[Date] AND rsiLong2.RSILong < rsiShort2.RSIShort))
				END as [ConsecutiveDaysLongAboveShort]
	FROM @rsiShort rsiShort INNER JOIN @rsiLong rsiLong ON rsiShort.QuoteId = rsiLong.QuoteId


	DECLARE @combinedDatasetNormalized TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4),
		[RSILong] DECIMAL(12, 4),
		[ConsecutiveDaysShortAboveLong] INT,
		[ConsecutiveDaysLongAboveShort] INT
	)

	--INSERT INTO @combinedDatasetNormalized
	SELECT [QuoteId],
		   [CompanyId],
		   [Date],
		   RSIShort / 100.0 as [RSIShort],
		   RSILong / 100.0 as [RSILong],
		   ConsecutiveDaysShortAboveLong / 10.0 as [ConsecutiveDaysShortAboveLong],
		   ConsecutiveDaysLongAboveShort / 10.0 as [ConsecutiveDaysLongAboveShort]
	FROM @combinedDataset
```
