﻿using System;

namespace CountingStrings.Service.Data.Models
{
    public class Session
    {
        public Guid Id { get; set; }

        public int Status { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
