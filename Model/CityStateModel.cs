using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace EazyPOS.Model
{


    [DataContract(Name = "CityModel")]
    public class CityStateModel
    {
        [DataMember(Name = "CityId")]
        public int cityid { get; set; }

        [DataMember(Name = "CityCode")]
        public string citycode { get; set; }

        [DataMember(Name = "CityName")]
        public string cityname { get; set; }

        [DataMember(Name = "StateId")]
        public int stateid { get; set; }

        [DataMember(Name = "StateName")]
        public string statename { get; set; }

        [DataMember(Name = "alteredon")]
        public string alteredon { get; set; }
    }


    [DataContract(Name = "StateModel")]
    public class StateModel
    {
        [DataMember(Name = "StateId")]
        public int stateid { get; set; }

        [DataMember(Name = "StateCode")]
        public string statecode { get; set; }

        [DataMember(Name = "StateName")]
        public string statename { get; set; }

        [DataMember(Name = "CountryId")]
        public int countryid { get; set; }

        [DataMember(Name = "CountryName")]
        public string countryname { get; set; }

        [DataMember(Name = "IsActive")]
        public bool isactive { get; set; }

        [DataMember(Name = "alteredon")]
        public string alteredon { get; set; }

    }


}
