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
    
    public partial class Appointment
    {
        public int AppointmentId { get; set; }
        public int ApartmentId { get; set; }
        public int TenantId { get; set; }
        public System.DateTime Date { get; set; }
        public int StatusId { get; set; }
    
        public virtual Apartment Apartment { get; set; }
        public virtual Status Status { get; set; }
        public virtual User User { get; set; }
    }
}
