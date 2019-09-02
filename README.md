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
 1. ~~Test NN (week)~~
 1. ~~Test NN (month)~~

#### SANNET.DataModel Library
- [x] Database creation scripts
- [x] ISANNETContext

##### SANNET Database
Tables, views, and stored procedures that should reside in the SANNET.DataModel library

##### Tables
- [x] NeuralNetworkConfigurations (Id [PK], Inputs, Outputs, NumHiddenLayers, NumHiddenLayerNeurons, TrainingStartDate, TrainingEndDate, TestingStartDate, TestingEndDate, Indicators)
- [x] CompanyPredictions (Id [PK], CompanyId [FK], ConfigId [FK], Prediction)

##### Stored Procedures
- [x] GetRSI (company, period, start & end date arguments)
- [x] GetRSICross (period1, period2)
- [x] GetCCI (company, period, start & end date arguments)
- [x] GetCCICross (period1, period2)
- [x] GetSMA (company, period, start & end date arguments)
- [x] GetSMACross (period1, period2)
- [x] GetFiveDayFuturePerformance(fromDate)
- [x] Create User Defined Table type
- [x] Create FUNCTION that returns table crosses
   
#### SANNET.Business Library
- [ ] Repositories
   - [x] TechnicalIndicatorRepository(ISANNETContext)
      - [x] GetRSIValues(int period)
      - [x] GetCCIValues(int period)
      - [x] GetSMAValues(int period)
      - [x] GetRSICrossValues(int shortPeriod, int longPeriod)
      - [x] GetCCICrossValues(int shortPeriod, int longPeriod)
      - [x] GetSMACrossValues(int shortPeriod, int longPeriod)
- [ ] Services
   - [ ] DatasetService(ITechnicalIndicatorRepository)
      - [ ] GetTrainingDataset1(Date) ---> Returns ~2 months worth of NN training data to train for a specific date. Returns NetworkTrainingDatasetMethod with unique id.
      - [ ] GetTestingDataset1(Date) ---> Returns the necessary inputs for a specific date that will be inserted into the NN where outputs will be captured. Returns NetworkTestingDatasetMethod with unique id.
   - [x] PredictionService(INeuralNetwork, IDatasetService, IPredictionRepository)
      - [x] <b>Keep in mind, we MAY have more than 1 dataset method that we want to compare with other datasets.. we need to be able to store predictions for ALL methods.. <i>SOLUTION: NetworkTrainingDatasetMethod & NetworkTestingDatasetMethod</i></b>
      - [x] (From Main) GeneratePredictions() ---> Foreach(company quote date that doesn't already have a matching prediction date AND uses the same NetworkTrainingDatasetMethod id), GeneratePrediction(quoteId);
      - [x] GeneratePrediction(quoteId) ---> Gets the NetworkTrainingDatasetMethod, trains the network, Gets the NetworkTestingDatasetMethod with matching id and applys as input to NN. Analyzes results and generates entry in predictions table with confidence of prediction. Returns prediction??
      - [ ] AnalyzePredictions() ---> Foreach prediction without an outcome (outside the 5-day window!), AnalyzePrediction();
      - [ ] AnalyzePrediction(id) ---> GetFutureFiveDayPerformance stored procedure; Determine if prediction was correct/incorrect and updated prediction entry.
      - [x] CreatePrediction()
      - [x] GetPredictions()
      - [x] GetPredictionById(id)
      - [x] UpdatePrediction()
      - [x] DeletePrediction()
