﻿using System;
using System.Collections.Generic;

namespace NorthwindConsole.Model;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
