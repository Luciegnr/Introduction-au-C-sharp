using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace User.Models
{

  
        public class users
    {
        public string email { get; set; }      
        public int id { get; set; }

        public string password { get; set; }
        public string role { get; set; }
    }

   

    public static class role
    {
        public const string admin = "admin";
        public const string utilisateur = "utilisateur";
    }
}
