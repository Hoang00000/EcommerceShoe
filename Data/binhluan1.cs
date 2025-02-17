namespace dotMVC.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class binhluan1
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int mabl { get; set; }

        public int mahh { get; set; }

        [Required]
        [StringLength(128)]
        public string makh { get; set; }

        [Column(TypeName = "date")]
        public DateTime ngaybl { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string noidung { get; set; }

        public int? duyet { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual hanghoa hanghoa { get; set; }
    }
}
