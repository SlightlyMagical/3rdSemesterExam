﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Coach : User
    {
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public List<DayOfWeek> AvailableDays { get; set; }
    }
}
