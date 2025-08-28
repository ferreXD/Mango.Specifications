namespace Mango.Specifications.Tests.Specification.Models
{
    internal class Customer(string name, bool isActive)
    {
        public string Name { get; init; } = name;
        public string Surname { get; set; } = string.Empty;
        public bool IsActive { get; init; } = isActive;
        public IEnumerable<string> Orders { get; set; } = new List<string>();
    }
}