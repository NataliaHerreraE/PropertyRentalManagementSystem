using PropertyRentalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyRentalManagementSystem.ViewModels
{
    public class TenantDashboardViewModel
    {
        public List<Apartment> RentedApartments { get; set; }
        public List<Message> Messages { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<RentalAgreement> RentalAgreements { get; set; }
    }
}