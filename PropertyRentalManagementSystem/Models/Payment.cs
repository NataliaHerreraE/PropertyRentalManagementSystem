//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PropertyRentalManagementSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Payment
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int AgreementId { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime DatePaid { get; set; }
        public string MethodOfPayment { get; set; }
        public int StatusId { get; set; }
    
        public virtual RentalAgreement RentalAgreement { get; set; }
        public virtual Status Status { get; set; }
        public virtual User User { get; set; }
    }
}
