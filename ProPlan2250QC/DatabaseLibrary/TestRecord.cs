namespace ProPlan2250QC.DatabaseLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TestRecord")]
    public partial class TestRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(50)]
        public string Operator { get; set; }

        public DateTime? TestDateTime { get; set; }

        public bool TestResult { get; set; }

        public double? Rpm { get; set; }

        public double? Level { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        [StringLength(200)]
        public string SerialNumber { get; set; }

        [StringLength(200)]
        public string Loading { get; set; }

        [StringLength(200)]
        public string Mounting { get; set; }

        [StringLength(200)]
        public string AlpplasMetadata1 { get; set; }

        [StringLength(200)]
        public string AlpplasMetadata2 { get; set; }

        [StringLength(200)]
        public string AlpplasMetadata3 { get; set; }

        [StringLength(200)]
        public string PowerSupply { get; set; }

        [StringLength(200)]
        public string Accessories { get; set; }
    }
}
