namespace Mango.Specifications.EntityFrameworkCore.Tests
{
    public class UnitMeasure
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public string UnitMeasureCode { get; set; } = null!;

        /// <summary>
        /// Unit of measure description.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}