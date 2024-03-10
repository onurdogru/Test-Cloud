namespace ProPlan2250QC.DatabaseLibrary
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DbModel : DbContext
    {
        public DbModel()
            : base("name=DbModel")
        {
        }

        public virtual DbSet<TestRecord> TestRecords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestRecord>()
                .Property(e => e.Operator)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.Model)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.SerialNumber)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.Loading)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.Mounting)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.AlpplasMetadata1)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.AlpplasMetadata2)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.AlpplasMetadata3)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.PowerSupply)
                .IsUnicode(true);

            modelBuilder.Entity<TestRecord>()
                .Property(e => e.Accessories)
                .IsUnicode(true);
        }
    }
}
