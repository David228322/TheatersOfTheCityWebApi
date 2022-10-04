﻿using Dapper.Contrib.Extensions;

namespace TheatersOfTheCity.Core.Domain;

[Table("Theaters")]
public class Theater : BaseEntity
{

    public  string Name { get; set; }
    
    public string City { get; set; }
    
    public  string Address { get; set; }
    
    public string Email { get; set; }
    
    public string Phone { get; set; }
    
    public string ArtisticDirector { get; set; }
}