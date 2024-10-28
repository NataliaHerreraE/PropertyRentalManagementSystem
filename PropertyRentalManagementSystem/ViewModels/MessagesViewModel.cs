using PropertyRentalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PropertyRentalManagementSystem.ViewModels
{
    public class MessagesViewModel
    {
        public List<Message> UnreadMessages { get; set; }
        public List<Message> ReadMessages { get; set; }
        public List<Message> AllMessages { get; set; } 
    }


}