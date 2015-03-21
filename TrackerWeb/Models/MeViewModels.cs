using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackerWeb.Models
{
    // Models returned by MeController actions.
    public class GetViewModel
    {
        public string Name { get; set; }
    }
}