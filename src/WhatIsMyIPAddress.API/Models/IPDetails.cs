using System.Collections.Generic;

namespace WhatIsMyIPAddress.API.Models
{
    public class IPDetails
    {
        public string IP { get; set; }
        public string Decimal { get; set; }
        public string HostName { get; set; }
        public string ASN { get; set; }
        public string ISP { get; set; }
        public string Organization { get; set; }
        public List<string> Services { get; set; }
        public string ServicesComment { get; set; }
        public string Type { get; set; }
        public string Assignment { get; set; }
        public string Continent { get; set; }
        public string Country { get; set; }
        public string CountryFlagUrl { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PostalCode { get; set; }

        public IPDetails()
        {
            this.Services = new List<string>();
        }
    }
}
