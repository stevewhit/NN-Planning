# NN Planning

## Requirements
- [x] NeuralNetwork.Generic library
- [ ] Stock Analysis Neural Network (SANNET) Application
- [ ] SANNET.DataModel Library
- [ ] SANNET.Business

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

For all stocks in table marked for comparison:
 1. Verify stock exists in sm.company table. If not, run downloader for company.
 1. Gather data from stored procedure for 'x' days of training/testing.
 1. Verify stock contains 'x' days worth of quotes. If not, run downloader for quotes.
 1. Setup NN
 1. Train NN (all data between 1 week and 20 weeks ago?)
 1. Test NN (tomorrow)
 1. Test NN (week)
 1. Test NN (month)

#### SANNET.DataModel Library
- [ ] Database creation scripts
- [ ] ApplicationDbContext

##### SANNET Database
Tables, views, and stored procedures that should reside in the SANNET.DataModel library

##### Tables
NeuralNetworkConfigurations (Id [PK], Inputs, Outputs, NumHiddenLayers, NumHiddenLayerNeurons, TrainingStartDate, TrainingEndDate, TestingStartDate, TestingEndDate, Indicators)
CompanyPredictions (Id [PK], CompanyId [FK], ConfigId [FK], Prediction)

##### Stored Procedures
- [x] GetRSI (company, period, start & end date arguments)
- [x] GetCCI (company, period, start & end date arguments)
- [x] GetSMA (company, period, start & end date arguments)
- [x] GetRSICross (period1, period2)
- [ ] GetTrainingDataset
- [x] Create User Defined Table type
- [x] Create FUNCTION that returns table crosses
   
--------------
``` SQL
-- Inputs: RSI, CCI, SMA, & all associated crosses
-- Outputs: Before the next 5 dates are up, did it:
--          1. Rise 4% or more to trigger sell?
--          2. Fall 2% or more to trigger sell?
--          3. Close at end above the Open?
--          4. Close at end below the Open?
```

#### SANNET.Business Library
- [ ] Repositories
   - [ ] TechnicalIndicatorRepositories
      * GetRSIValues(int period) ==> Dictionary<quoteId, RSIValue>
      * GetCCIValues(int period) ==> Dictionary<quoteId, CCIValue>
      * GetSMAValues(int period) ==> Dictionary<quoteId, SMAValue>
      * GetRSICrossValues(int shortPeriod, int longPeriod) ==> Dictionary<quoteId, RSICrossValue>
      * GetCCICrossValues(int shortPeriod, int longPeriod) ==> Dictionary<quoteId, CCICrossValue>
      * GetSMACrossValues(int shortPeriod, int longPeriod) ==> Dictionary<quoteId, SMACrossValue>
- [ ] Services
   - [ ] DatasetService
      * GetTrainingDataset() ---> Combines all technical indicator values into one large training dataset to use in NN
      * ...
