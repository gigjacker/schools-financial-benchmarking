﻿using System;

namespace SFB.Web.UI.Models
{
    public class SchoolSummaryViewModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }
        public string EducationPhases { get; set; }
        public string NFType { get; set; }

        public SchoolSummaryViewModel(dynamic model)
        {
            var location = model["Location"];
            if (location != null && location.coordinates != null)
            {
                Latitude = location.coordinates[1];
                Longitude = location.coordinates[0];
            }
            Name = model["EstablishmentName"];
            Id = model["URN"];
            Address = String.Format("{0}, {1}, {2}", model["Street"], model["Town"], model["Postcode"]);
            EducationPhases = model["PhaseOfEducation"];
            NFType = model["TypeOfEstablishment"];
        }
    }
}