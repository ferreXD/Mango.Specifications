namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    public class Culture
    {
        /// <summary>
        /// Primary key for Culture records.
        /// </summary>
        public string CultureId { get; set; } = null!;

        /// <summary>
        /// Culture description.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}