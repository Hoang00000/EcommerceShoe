namespace dotMVC.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("about")]
    public partial class about
    {
        [Key]
        public int maab { get; set; }

        [Required]
        [StringLength(500)]
        public string tieude { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string noidung { get; set; }
    }
}
