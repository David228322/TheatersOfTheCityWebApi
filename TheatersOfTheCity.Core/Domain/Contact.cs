﻿using System.ComponentModel.DataAnnotations;
using Dapper;
using Dapper.Contrib.Extensions;
using TheatersOfTheCity.Core.Enums;

namespace TheatersOfTheCity.Core.Domain;

[Table("Contact")]
public class Contact
{
    [Dapper.Contrib.Extensions.Key]
    public  int ContactId { get; set; }
    
    public string FirstName { get; set; }

    public string SecondName { get; set; }

    public DateTime Birth { get; set; }

    public  string Email { get; set; }
    
    public string Phone { get; set; }
}