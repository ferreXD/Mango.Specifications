namespace Mango.Specifications.EntityFrameworkCore.Tests.Data.Context
{
    using Entities;
    using Microsoft.EntityFrameworkCore;

    internal partial class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<AddressType> AddressTypes { get; set; }
        public virtual DbSet<BusinessEntity> BusinessEntities { get; set; }
        public virtual DbSet<BusinessEntityAddress> BusinessEntityAddresses { get; set; }
        public virtual DbSet<BusinessEntityContact> BusinessEntityContacts { get; set; }
        public virtual DbSet<ContactType> ContactTypes { get; set; }
        public virtual DbSet<CountryRegion> CountryRegions { get; set; }
        public virtual DbSet<CountryRegionCurrency> CountryRegionCurrencies { get; set; }
        public virtual DbSet<CreditCard> CreditCards { get; set; }
        public virtual DbSet<Culture> Cultures { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<CurrencyRate> CurrencyRates { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<EmailAddress> EmailAddresses { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; }
        public virtual DbSet<EmployeePayHistory> EmployeePayHistories { get; set; }
        public virtual DbSet<JobCandidate> JobCandidates { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Password> Passwords { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PersonCreditCard> PersonCreditCards { get; set; }
        public virtual DbSet<PersonPhone> PersonPhones { get; set; }
        public virtual DbSet<PhoneNumberType> PhoneNumberTypes { get; set; }
        public virtual DbSet<Shift> Shifts { get; set; }
        public virtual DbSet<StateProvince> StateProvinces { get; set; }
        public virtual DbSet<UnitMeasure> UnitMeasures { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Data Source=DESKTOP-SB85G0U;Initial Catalog=AdventureWorks2022;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.AddressId).HasName("PK_Address_AddressID");

                entity.ToTable("Address", "Person", tb => tb.HasComment("Street address information for customers, employees, and vendors."));

                entity.HasIndex(e => e.Rowguid, "AK_Address_rowguid").IsUnique();

                entity.HasIndex(e => new { e.AddressLine1, e.AddressLine2, e.City, e.StateProvinceId, e.PostalCode }, "IX_Address_AddressLine1_AddressLine2_City_StateProvinceID_PostalCode").IsUnique();

                entity.HasIndex(e => e.StateProvinceId, "IX_Address_StateProvinceID");

                entity.Property(e => e.AddressId)
                    .HasComment("Primary key for Address records.")
                    .HasColumnName("AddressID");
                entity.Property(e => e.AddressLine1)
                    .HasMaxLength(60)
                    .HasComment("First street address line.");
                entity.Property(e => e.AddressLine2)
                    .HasMaxLength(60)
                    .HasComment("Second street address line.");
                entity.Property(e => e.City)
                    .HasMaxLength(30)
                    .HasComment("Name of the city.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.PostalCode)
                    .HasMaxLength(15)
                    .HasComment("Postal code for the street address.");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
                entity.Property(e => e.StateProvinceId)
                    .HasComment("Unique identification number for the state or province. Foreign key to StateProvince table.")
                    .HasColumnName("StateProvinceID");

                entity.HasOne(d => d.StateProvince).WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.StateProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<AddressType>(entity =>
            {
                entity.HasKey(e => e.AddressTypeId).HasName("PK_AddressType_AddressTypeID");

                entity.ToTable("AddressType", "Person", tb => tb.HasComment("Types of addresses stored in the Address table. "));

                entity.HasIndex(e => e.Name, "AK_AddressType_Name").IsUnique();

                entity.HasIndex(e => e.Rowguid, "AK_AddressType_rowguid").IsUnique();

                entity.Property(e => e.AddressTypeId)
                    .HasComment("Primary key for AddressType records.")
                    .HasColumnName("AddressTypeID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Address type description. For example, Billing, Home, or Shipping.");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
            });

            modelBuilder.Entity<BusinessEntity>(entity =>
            {
                entity.HasKey(e => e.BusinessEntityId).HasName("PK_BusinessEntity_BusinessEntityID");

                entity.ToTable("BusinessEntity", "Person", tb => tb.HasComment("Source of the ID that connects vendors, customers, and employees with address and contact information."));

                entity.HasIndex(e => e.Rowguid, "AK_BusinessEntity_rowguid").IsUnique();

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Primary key for all customers, vendors, and employees.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
            });

            modelBuilder.Entity<BusinessEntityAddress>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.AddressId, e.AddressTypeId }).HasName("PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID");

                entity.ToTable("BusinessEntityAddress", "Person", tb => tb.HasComment("Cross-reference table mapping customers, vendors, and employees to their addresses."));

                entity.HasIndex(e => e.Rowguid, "AK_BusinessEntityAddress_rowguid").IsUnique();

                entity.HasIndex(e => e.AddressId, "IX_BusinessEntityAddress_AddressID");

                entity.HasIndex(e => e.AddressTypeId, "IX_BusinessEntityAddress_AddressTypeID");

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Primary key. Foreign key to BusinessEntity.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.AddressId)
                    .HasComment("Primary key. Foreign key to Address.AddressID.")
                    .HasColumnName("AddressID");
                entity.Property(e => e.AddressTypeId)
                    .HasComment("Primary key. Foreign key to AddressType.AddressTypeID.")
                    .HasColumnName("AddressTypeID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");

                entity.HasOne(d => d.Address).WithMany(p => p.BusinessEntityAddresses)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.AddressType).WithMany(p => p.BusinessEntityAddresses)
                    .HasForeignKey(d => d.AddressTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.BusinessEntityAddresses)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<BusinessEntityContact>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.PersonId, e.ContactTypeId }).HasName("PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID");

                entity.ToTable("BusinessEntityContact", "Person", tb => tb.HasComment("Cross-reference table mapping stores, vendors, and employees to people"));

                entity.HasIndex(e => e.Rowguid, "AK_BusinessEntityContact_rowguid").IsUnique();

                entity.HasIndex(e => e.ContactTypeId, "IX_BusinessEntityContact_ContactTypeID");

                entity.HasIndex(e => e.PersonId, "IX_BusinessEntityContact_PersonID");

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Primary key. Foreign key to BusinessEntity.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.PersonId)
                    .HasComment("Primary key. Foreign key to Person.BusinessEntityID.")
                    .HasColumnName("PersonID");
                entity.Property(e => e.ContactTypeId)
                    .HasComment("Primary key.  Foreign key to ContactType.ContactTypeID.")
                    .HasColumnName("ContactTypeID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.BusinessEntityContacts)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ContactType).WithMany(p => p.BusinessEntityContacts)
                    .HasForeignKey(d => d.ContactTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Person).WithMany(p => p.BusinessEntityContacts)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<ContactType>(entity =>
            {
                entity.HasKey(e => e.ContactTypeId).HasName("PK_ContactType_ContactTypeID");

                entity.ToTable("ContactType", "Person", tb => tb.HasComment("Lookup table containing the types of business entity contacts."));

                entity.HasIndex(e => e.Name, "AK_ContactType_Name").IsUnique();

                entity.Property(e => e.ContactTypeId)
                    .HasComment("Primary key for ContactType records.")
                    .HasColumnName("ContactTypeID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Contact type description.");
            });

            modelBuilder.Entity<CountryRegion>(entity =>
            {
                entity.HasKey(e => e.CountryRegionCode).HasName("PK_CountryRegion_CountryRegionCode");

                entity.ToTable("CountryRegion", "Person", tb => tb.HasComment("Lookup table containing the ISO standard codes for countries and regions."));

                entity.HasIndex(e => e.Name, "AK_CountryRegion_Name").IsUnique();

                entity.Property(e => e.CountryRegionCode)
                    .HasMaxLength(3)
                    .HasComment("ISO standard code for countries and regions.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Country or region name.");
            });

            modelBuilder.Entity<CountryRegionCurrency>(entity =>
            {
                entity.HasKey(e => new { e.CountryRegionCode, e.CurrencyCode }).HasName("PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode");

                entity.ToTable("CountryRegionCurrency", "Sales", tb => tb.HasComment("Cross-reference table mapping ISO currency codes to a country or region."));

                entity.HasIndex(e => e.CurrencyCode, "IX_CountryRegionCurrency_CurrencyCode");

                entity.Property(e => e.CountryRegionCode)
                    .HasMaxLength(3)
                    .HasComment("ISO code for countries and regions. Foreign key to CountryRegion.CountryRegionCode.");
                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("ISO standard currency code. Foreign key to Currency.CurrencyCode.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.CountryRegionCodeNavigation).WithMany(p => p.CountryRegionCurrencies)
                    .HasForeignKey(d => d.CountryRegionCode)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.CountryRegionCurrencies)
                    .HasForeignKey(d => d.CurrencyCode)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<CreditCard>(entity =>
            {
                entity.HasKey(e => e.CreditCardId).HasName("PK_CreditCard_CreditCardID");

                entity.ToTable("CreditCard", "Sales", tb => tb.HasComment("Customer credit card information."));

                entity.HasIndex(e => e.CardNumber, "AK_CreditCard_CardNumber").IsUnique();

                entity.Property(e => e.CreditCardId)
                    .HasComment("Primary key for CreditCard records.")
                    .HasColumnName("CreditCardID");
                entity.Property(e => e.CardNumber)
                    .HasMaxLength(25)
                    .HasComment("Credit card number.");
                entity.Property(e => e.CardType)
                    .HasMaxLength(50)
                    .HasComment("Credit card name.");
                entity.Property(e => e.ExpMonth).HasComment("Credit card expiration month.");
                entity.Property(e => e.ExpYear).HasComment("Credit card expiration year.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Culture>(entity =>
            {
                entity.HasKey(e => e.CultureId).HasName("PK_Culture_CultureID");

                entity.ToTable("Culture", "Production", tb => tb.HasComment("Lookup table containing the languages in which some AdventureWorks data is stored."));

                entity.HasIndex(e => e.Name, "AK_Culture_Name").IsUnique();

                entity.Property(e => e.CultureId)
                    .HasMaxLength(6)
                    .IsFixedLength()
                    .HasComment("Primary key for Culture records.")
                    .HasColumnName("CultureID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Culture description.");
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.HasKey(e => e.CurrencyCode).HasName("PK_Currency_CurrencyCode");

                entity.ToTable("Currency", "Sales", tb => tb.HasComment("Lookup table containing standard ISO currencies."));

                entity.HasIndex(e => e.Name, "AK_Currency_Name").IsUnique();

                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("The ISO code for the Currency.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Currency name.");
            });

            modelBuilder.Entity<CurrencyRate>(entity =>
            {
                entity.HasKey(e => e.CurrencyRateId).HasName("PK_CurrencyRate_CurrencyRateID");

                entity.ToTable("CurrencyRate", "Sales", tb => tb.HasComment("Currency exchange rates."));

                entity.HasIndex(e => new { e.CurrencyRateDate, e.FromCurrencyCode, e.ToCurrencyCode }, "AK_CurrencyRate_CurrencyRateDate_FromCurrencyCode_ToCurrencyCode").IsUnique();

                entity.Property(e => e.CurrencyRateId)
                    .HasComment("Primary key for CurrencyRate records.")
                    .HasColumnName("CurrencyRateID");
                entity.Property(e => e.AverageRate)
                    .HasComment("Average exchange rate for the day.")
                    .HasColumnType("money");
                entity.Property(e => e.CurrencyRateDate)
                    .HasComment("Date and time the exchange rate was obtained.")
                    .HasColumnType("datetime");
                entity.Property(e => e.EndOfDayRate)
                    .HasComment("Final exchange rate for the day.")
                    .HasColumnType("money");
                entity.Property(e => e.FromCurrencyCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("Exchange rate was converted from this currency code.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.ToCurrencyCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("Exchange rate was converted to this currency code.");

                entity.HasOne(d => d.FromCurrencyCodeNavigation).WithMany(p => p.CurrencyRateFromCurrencyCodeNavigations)
                    .HasForeignKey(d => d.FromCurrencyCode)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ToCurrencyCodeNavigation).WithMany(p => p.CurrencyRateToCurrencyCodeNavigations)
                    .HasForeignKey(d => d.ToCurrencyCode)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId).HasName("PK_Customer_CustomerID");

                entity.ToTable("Customer", "Sales", tb => tb.HasComment("Current customer information. Also see the Person and Store tables."));

                entity.HasIndex(e => e.AccountNumber, "AK_Customer_AccountNumber").IsUnique();

                entity.HasIndex(e => e.Rowguid, "AK_Customer_rowguid").IsUnique();

                entity.HasIndex(e => e.TerritoryId, "IX_Customer_TerritoryID");

                entity.Property(e => e.CustomerId)
                    .HasComment("Primary key.")
                    .HasColumnName("CustomerID");
                entity.Property(e => e.AccountNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComputedColumnSql("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))", false)
                    .HasComment("Unique number identifying the customer assigned by the accounting system.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.PersonId)
                    .HasComment("Foreign key to Person.BusinessEntityID")
                    .HasColumnName("PersonID");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
                entity.Property(e => e.StoreId)
                    .HasComment("Foreign key to Store.BusinessEntityID")
                    .HasColumnName("StoreID");
                entity.Property(e => e.TerritoryId)
                    .HasComment("ID of the territory in which the customer is located. Foreign key to SalesTerritory.SalesTerritoryID.")
                    .HasColumnName("TerritoryID");

                entity.HasOne(d => d.Person).WithMany(p => p.Customers).HasForeignKey(d => d.PersonId);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId).HasName("PK_Department_DepartmentID");

                entity.ToTable("Department", "HumanResources", tb => tb.HasComment("Lookup table containing the departments within the Adventure Works Cycles company."));

                entity.HasIndex(e => e.Name, "AK_Department_Name").IsUnique();

                entity.Property(e => e.DepartmentId)
                    .HasComment("Primary key for Department records.")
                    .HasColumnName("DepartmentID");
                entity.Property(e => e.GroupName)
                    .HasMaxLength(50)
                    .HasComment("Name of the group to which the department belongs.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Name of the department.");
            });

            modelBuilder.Entity<EmailAddress>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.EmailAddressId }).HasName("PK_EmailAddress_BusinessEntityID_EmailAddressID");

                entity.ToTable("EmailAddress", "Person", tb => tb.HasComment("Where to send a person email."));

                entity.HasIndex(e => e.EmailAddress1, "IX_EmailAddress_EmailAddress");

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Primary key. Person associated with this email address.  Foreign key to Person.BusinessEntityID")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.EmailAddressId)
                    .ValueGeneratedOnAdd()
                    .HasComment("Primary key. ID of this email address.")
                    .HasColumnName("EmailAddressID");
                entity.Property(e => e.EmailAddress1)
                    .HasMaxLength(50)
                    .HasComment("E-mail address for the person.")
                    .HasColumnName("EmailAddress");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.EmailAddresses)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.BusinessEntityId).HasName("PK_Employee_BusinessEntityID");

                entity.ToTable("Employee", "HumanResources", tb =>
                {
                    tb.HasComment("Employee information such as salary, department, and title.");
                    tb.HasTrigger("dEmployee");
                });

                entity.HasIndex(e => e.LoginId, "AK_Employee_LoginID").IsUnique();

                entity.HasIndex(e => e.NationalIdnumber, "AK_Employee_NationalIDNumber").IsUnique();

                entity.HasIndex(e => e.Rowguid, "AK_Employee_rowguid").IsUnique();

                entity.Property(e => e.BusinessEntityId)
                    .ValueGeneratedNever()
                    .HasComment("Primary key for Employee records.  Foreign key to BusinessEntity.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.BirthDate).HasComment("Date of birth.");
                entity.Property(e => e.CurrentFlag)
                    .HasDefaultValue(true)
                    .HasComment("0 = Inactive, 1 = Active");
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasComment("M = Male, F = Female");
                entity.Property(e => e.HireDate).HasComment("Employee hired on this date.");
                entity.Property(e => e.JobTitle)
                    .HasMaxLength(50)
                    .HasComment("Work title such as Buyer or Sales Representative.");
                entity.Property(e => e.LoginId)
                    .HasMaxLength(256)
                    .HasComment("Network login.")
                    .HasColumnName("LoginID");
                entity.Property(e => e.MaritalStatus)
                    .HasMaxLength(1)
                    .IsFixedLength()
                    .HasComment("M = Married, S = Single");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.NationalIdnumber)
                    .HasMaxLength(15)
                    .HasComment("Unique national identification number such as a social security number.")
                    .HasColumnName("NationalIDNumber");
                entity.Property(e => e.OrganizationLevel)
                    .HasComputedColumnSql("([OrganizationNode].[GetLevel]())", false)
                    .HasComment("The depth of the employee in the corporate hierarchy.");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
                entity.Property(e => e.SalariedFlag)
                    .HasDefaultValue(true)
                    .HasComment("Job classification. 0 = Hourly, not exempt from collective bargaining. 1 = Salaried, exempt from collective bargaining.");
                entity.Property(e => e.SickLeaveHours).HasComment("Number of available sick leave hours.");
                entity.Property(e => e.VacationHours).HasComment("Number of available vacation hours.");

                entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Employee)
                    .HasForeignKey<Employee>(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<EmployeeDepartmentHistory>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.StartDate, e.DepartmentId, e.ShiftId }).HasName("PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID");

                entity.ToTable("EmployeeDepartmentHistory", "HumanResources", tb => tb.HasComment("Employee department transfers."));

                entity.HasIndex(e => e.DepartmentId, "IX_EmployeeDepartmentHistory_DepartmentID");

                entity.HasIndex(e => e.ShiftId, "IX_EmployeeDepartmentHistory_ShiftID");

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Employee identification number. Foreign key to Employee.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.StartDate).HasComment("Date the employee started work in the department.");
                entity.Property(e => e.DepartmentId)
                    .HasComment("Department in which the employee worked including currently. Foreign key to Department.DepartmentID.")
                    .HasColumnName("DepartmentID");
                entity.Property(e => e.ShiftId)
                    .HasComment("Identifies which 8-hour shift the employee works. Foreign key to Shift.Shift.ID.")
                    .HasColumnName("ShiftID");
                entity.Property(e => e.EndDate).HasComment("Date the employee left the department. NULL = Current department.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.EmployeeDepartmentHistories)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Shift).WithMany(p => p.EmployeeDepartmentHistories)
                    .HasForeignKey(d => d.ShiftId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<EmployeePayHistory>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.RateChangeDate }).HasName("PK_EmployeePayHistory_BusinessEntityID_RateChangeDate");

                entity.ToTable("EmployeePayHistory", "HumanResources", tb => tb.HasComment("Employee pay history."));

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Employee identification number. Foreign key to Employee.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.RateChangeDate)
                    .HasComment("Date the change in pay is effective")
                    .HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.PayFrequency).HasComment("1 = Salary received monthly, 2 = Salary received biweekly");
                entity.Property(e => e.Rate)
                    .HasComment("Salary hourly rate.")
                    .HasColumnType("money");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.EmployeePayHistories)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<JobCandidate>(entity =>
            {
                entity.HasKey(e => e.JobCandidateId).HasName("PK_JobCandidate_JobCandidateID");

                entity.ToTable("JobCandidate", "HumanResources", tb => tb.HasComment("Résumés submitted to Human Resources by job applicants."));

                entity.HasIndex(e => e.BusinessEntityId, "IX_JobCandidate_BusinessEntityID");

                entity.Property(e => e.JobCandidateId)
                    .HasComment("Primary key for JobCandidate records.")
                    .HasColumnName("JobCandidateID");
                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Employee identification number if applicant was hired. Foreign key to Employee.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Resume)
                    .HasComment("Résumé in XML format.")
                    .HasColumnType("xml");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.JobCandidates).HasForeignKey(d => d.BusinessEntityId);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.LocationId).HasName("PK_Location_LocationID");

                entity.ToTable("Location", "Production", tb => tb.HasComment("Product inventory and manufacturing locations."));

                entity.HasIndex(e => e.Name, "AK_Location_Name").IsUnique();

                entity.Property(e => e.LocationId)
                    .HasComment("Primary key for Location records.")
                    .HasColumnName("LocationID");
                entity.Property(e => e.Availability)
                    .HasComment("Work capacity (in hours) of the manufacturing location.")
                    .HasColumnType("decimal(8, 2)");
                entity.Property(e => e.CostRate)
                    .HasComment("Standard hourly cost of the manufacturing location.")
                    .HasColumnType("smallmoney");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Location description.");
            });

            modelBuilder.Entity<Password>(entity =>
            {
                entity.HasKey(e => e.BusinessEntityId).HasName("PK_Password_BusinessEntityID");

                entity.ToTable("Password", "Person", tb => tb.HasComment("One way hashed authentication information"));

                entity.Property(e => e.BusinessEntityId)
                    .ValueGeneratedNever()
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasComment("Password for the e-mail account.");
                entity.Property(e => e.PasswordSalt)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasComment("Random value concatenated with the password string before the password is hashed.");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");

                entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Password)
                    .HasForeignKey<Password>(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.BusinessEntityId).HasName("PK_Person_BusinessEntityID");

                entity.ToTable("Person", "Person", tb =>
                {
                    tb.HasComment("Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.");
                    tb.HasTrigger("iuPerson");
                });

                entity.HasIndex(e => e.Rowguid, "AK_Person_rowguid").IsUnique();

                entity.HasIndex(e => new { e.LastName, e.FirstName, e.MiddleName }, "IX_Person_LastName_FirstName_MiddleName");

                entity.HasIndex(e => e.AdditionalContactInfo, "PXML_Person_AddContact");

                entity.HasIndex(e => e.Demographics, "PXML_Person_Demographics");

                entity.HasIndex(e => e.Demographics, "XMLPATH_Person_Demographics");

                entity.HasIndex(e => e.Demographics, "XMLPROPERTY_Person_Demographics");

                entity.HasIndex(e => e.Demographics, "XMLVALUE_Person_Demographics");

                entity.Property(e => e.BusinessEntityId)
                    .ValueGeneratedNever()
                    .HasComment("Primary key for Person records.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.AdditionalContactInfo)
                    .HasComment("Additional contact information about the person stored in xml format. ")
                    .HasColumnType("xml");
                entity.Property(e => e.Demographics)
                    .HasComment("Personal information such as hobbies, and income collected from online shoppers. Used for sales analysis.")
                    .HasColumnType("xml");
                entity.Property(e => e.EmailPromotion).HasComment("0 = Contact does not wish to receive e-mail promotions, 1 = Contact does wish to receive e-mail promotions from AdventureWorks, 2 = Contact does wish to receive e-mail promotions from AdventureWorks and selected partners. ");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasComment("First name of the person.");
                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasComment("Last name of the person.");
                entity.Property(e => e.MiddleName)
                    .HasMaxLength(50)
                    .HasComment("Middle name or middle initial of the person.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.NameStyle).HasComment("0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.");
                entity.Property(e => e.PersonType)
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .HasComment("Primary type of person: SC = Store Contact, IN = Individual (retail) customer, SP = Sales person, EM = Employee (non-sales), VC = Vendor contact, GC = General contact");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
                entity.Property(e => e.Suffix)
                    .HasMaxLength(10)
                    .HasComment("Surname suffix. For example, Sr. or Jr.");
                entity.Property(e => e.Title)
                    .HasMaxLength(8)
                    .HasComment("A courtesy title. For example, Mr. or Ms.");

                entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Person)
                    .HasForeignKey<Person>(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PersonCreditCard>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.CreditCardId }).HasName("PK_PersonCreditCard_BusinessEntityID_CreditCardID");

                entity.ToTable("PersonCreditCard", "Sales", tb => tb.HasComment("Cross-reference table mapping people to their credit card information in the CreditCard table. "));

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Business entity identification number. Foreign key to Person.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.CreditCardId)
                    .HasComment("Credit card identification number. Foreign key to CreditCard.CreditCardID.")
                    .HasColumnName("CreditCardID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.PersonCreditCards)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.CreditCard).WithMany(p => p.PersonCreditCards)
                    .HasForeignKey(d => d.CreditCardId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PersonPhone>(entity =>
            {
                entity.HasKey(e => new { e.BusinessEntityId, e.PhoneNumber, e.PhoneNumberTypeId }).HasName("PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID");

                entity.ToTable("PersonPhone", "Person", tb => tb.HasComment("Telephone number and type of a person."));

                entity.HasIndex(e => e.PhoneNumber, "IX_PersonPhone_PhoneNumber");

                entity.Property(e => e.BusinessEntityId)
                    .HasComment("Business entity identification number. Foreign key to Person.BusinessEntityID.")
                    .HasColumnName("BusinessEntityID");
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(25)
                    .HasComment("Telephone number identification number.");
                entity.Property(e => e.PhoneNumberTypeId)
                    .HasComment("Kind of phone number. Foreign key to PhoneNumberType.PhoneNumberTypeID.")
                    .HasColumnName("PhoneNumberTypeID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.BusinessEntity).WithMany(p => p.PersonPhones)
                    .HasForeignKey(d => d.BusinessEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.PhoneNumberType).WithMany(p => p.PersonPhones)
                    .HasForeignKey(d => d.PhoneNumberTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PhoneNumberType>(entity =>
            {
                entity.HasKey(e => e.PhoneNumberTypeId).HasName("PK_PhoneNumberType_PhoneNumberTypeID");

                entity.ToTable("PhoneNumberType", "Person", tb => tb.HasComment("Type of phone number of a person."));

                entity.Property(e => e.PhoneNumberTypeId)
                    .HasComment("Primary key for telephone number type records.")
                    .HasColumnName("PhoneNumberTypeID");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Name of the telephone number type");
            });

            modelBuilder.Entity<StateProvince>(entity =>
            {
                entity.HasKey(e => e.StateProvinceId).HasName("PK_StateProvince_StateProvinceID");

                entity.ToTable("StateProvince", "Person", tb => tb.HasComment("State and province lookup table."));

                entity.HasIndex(e => e.Name, "AK_StateProvince_Name").IsUnique();

                entity.HasIndex(e => new { e.StateProvinceCode, e.CountryRegionCode }, "AK_StateProvince_StateProvinceCode_CountryRegionCode").IsUnique();

                entity.HasIndex(e => e.Rowguid, "AK_StateProvince_rowguid").IsUnique();

                entity.Property(e => e.StateProvinceId)
                    .HasComment("Primary key for StateProvince records.")
                    .HasColumnName("StateProvinceID");
                entity.Property(e => e.CountryRegionCode)
                    .HasMaxLength(3)
                    .HasComment("ISO standard country or region code. Foreign key to CountryRegion.CountryRegionCode. ");
                entity.Property(e => e.IsOnlyStateProvinceFlag)
                    .HasDefaultValue(true)
                    .HasComment("0 = StateProvinceCode exists. 1 = StateProvinceCode unavailable, using CountryRegionCode.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("State or province description.");
                entity.Property(e => e.Rowguid)
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")
                    .HasColumnName("rowguid");
                entity.Property(e => e.StateProvinceCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("ISO standard state or province code.");
                entity.Property(e => e.TerritoryId)
                    .HasComment("ID of the territory in which the state or province is located. Foreign key to SalesTerritory.SalesTerritoryID.")
                    .HasColumnName("TerritoryID");

                entity.HasOne(d => d.CountryRegionCodeNavigation).WithMany(p => p.StateProvinces)
                    .HasForeignKey(d => d.CountryRegionCode)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UnitMeasure>(entity =>
            {
                entity.HasKey(e => e.UnitMeasureCode).HasName("PK_UnitMeasure_UnitMeasureCode");

                entity.ToTable("UnitMeasure", "Production", tb => tb.HasComment("Unit of measure lookup table."));

                entity.HasIndex(e => e.Name, "AK_UnitMeasure_Name").IsUnique();

                entity.Property(e => e.UnitMeasureCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("Primary key.");
                entity.Property(e => e.ModifiedDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("Unit of measure description.");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}