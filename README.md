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

## Indicators
### Stochastic Indicator
The stochastic indicator is a momentum indicator developed by George C. Lane in the 1950s, which shows the position of the most recent closing price relative to the previous high-low range. The indicator measures momentum by comparing the closing price with the previous trading range over a specific period of time.

```
%K = (Most Recent Closing Price - Lowest Low) / (Highest High - Lowest Low) × 100
%D = 3-day SMA of %K
---
Lowest Low = lowest low of the specified time period
Highest High = highest high of the specified time period
```

As a range-bound indicator, the stochastic oscillator can be used to identify overbought and oversold market conditions. A reading over 80 reflects overbought market conditions, and a reading below 20 reflects oversold market conditions. The stochastic indicator itself can range only from 0 to 100, no matter how fast the price of the underlying currency pair changes. In a standard 14-period setting, a reading above 80 indicates that the pair has been trading near the top of its trading range over the last 14 periods, while a reading below 20 indicates that the pair has been trading near the low of its trading range over the last 14 periods.

It is important to note that oversold readings are not necessarily bullish, just like overbought readings are not necessarily bearish. During a sustained uptrend or downtrend, the stochastic indicator can remain in the oversold or overbought area for a long period of time. It is, therefore, advised to always trade in the direction of the trend and wait for occasional oversold readings during uptrends and overbought readings during downtrends. [[1]](https://alpari.com/en/beginner/articles/intro-stochastic-indicator/)

Key takeaways: 
1. Overbought and oversold conditions do not necessarily indicate good trading indicators.
1. Suggested use is as follows:
  * Look at 6-month daily chart to get a feel for overall momentum of stock
  * If 6-month stock moving average is positive, and the stochastic dips below 20 and shoots back up above 20, this may be a good indicat
