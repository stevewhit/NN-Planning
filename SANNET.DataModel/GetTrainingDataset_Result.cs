//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SANNET.DataModel
{
    using System;
    
    public partial class GetTrainingDataset_Result
    {
        public Nullable<int> QuoteId { get; set; }
        public bool I_IsMACDAboveZeroLine { get; set; }
        public bool I_IsStochasticOverBought { get; set; }
        public bool I_IsStochasticOverSold { get; set; }
        public bool I_IsStochasticNeitherOverBoughtOrOverSold { get; set; }
        public bool I_IsRSIOverBought { get; set; }
        public bool I_IsRSIOverSold { get; set; }
        public bool I_IsRSINeitherOverBoughtOrOverSold { get; set; }
        public bool I_IsCCIOverBought { get; set; }
        public bool I_IsCCIOverSold { get; set; }
        public bool I_IsCCINeitherOverBoughtOrOverSold { get; set; }
        public bool I_IsSMAShortGreaterThanLong { get; set; }
        public bool I_IsEMAShortGreaterThanLong { get; set; }
        public bool O_TriggeredRiseFirst { get; set; }
        public bool O_TriggeredFallFirst { get; set; }
    }
}
