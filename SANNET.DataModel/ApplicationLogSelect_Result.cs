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
    
    public partial class ApplicationLogSelect_Result
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Thread { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }
    }
}
