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
    
    public partial class Apartment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Apartment()
        {
            this.Appointments = new HashSet<Appointment>();
            this.Messages = new HashSet<Message>();
            this.RentalAgreements = new HashSet<RentalAgreement>();
        }
    
        public int ApartmentId { get; set; }
        public int BuildingId { get; set; }
        public string AppartmentNumber { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public System.DateTime DateListed { get; set; }
        public int StatusId { get; set; }
        public string ImagePath { get; set; }
        public decimal Price { get; set; }
    
        public virtual Building Building { get; set; }
        public virtual Status Status { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Appointment> Appointments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Message> Messages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RentalAgreement> RentalAgreements { get; set; }
    }
}
