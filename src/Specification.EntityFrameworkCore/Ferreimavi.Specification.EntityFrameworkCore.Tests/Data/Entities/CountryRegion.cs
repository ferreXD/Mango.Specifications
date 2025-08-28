namespace Mango.Specifications.EntityFrameworkCore.Tests.Data.Entities
{
    public class CountryRegion
    {
        /// <summary>
        /// ISO standard code for countries and regions.
        /// </summary>
        public string CountryRegionCode { get; set; } = null!;

        /// <summary>
        /// Country or region name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<CountryRegionCurrency> CountryRegionCurrencies { get; set; } = new List<CountryRegionCurrency>();

        public virtual ICollection<StateProvince> StateProvinces { get; set; } = new List<StateProvince>();
    }
}