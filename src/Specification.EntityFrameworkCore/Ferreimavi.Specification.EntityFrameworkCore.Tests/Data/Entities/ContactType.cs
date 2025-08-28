namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    public class ContactType
    {
        /// <summary>
        /// Primary key for ContactType records.
        /// </summary>
        public int ContactTypeId { get; set; }

        /// <summary>
        /// Contact type description.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<BusinessEntityContact> BusinessEntityContacts { get; set; } = new List<BusinessEntityContact>();
    }
}