# NN Planning
In the current state, the application generates and analyzes predictions based on the training dataset and network configuration that is applied. The application creates and trains Neural Networks very well, but is not very correct in its predictions.

## How-Tos
1. Add new Network Configuration
   * Add new network configuration to the NetworkConfigurations table.
   * Add new [GetTrainingDataset] stored procedure to the SANNET database.
   * Update SANNET.DataModel .edmx to pull new stored procedure into model.
   * Add retrieval method(s) to the SANNET.Business.DatasetRepository class & interface.
   * Update SANNET.Business.DatasetRepository.GetTrainingDataset() method.

## TODO:
- [ ] Unit Tests!
- [ ] Consider more volatile stocks. Will possibly have to ONLY generate predictions for more volatile stocks?
- [ ] Add stored procedures for new indicators. (Stochastic oscillator..)
- [ ] Try different periods for existing technical indicator stored procedures.
- [ ] Try different NetworkConfigurations (hidden layers, hidden layer neurons) to help speed up training without sacrificing correctness.
- [ ] Try different training date-ranges.
