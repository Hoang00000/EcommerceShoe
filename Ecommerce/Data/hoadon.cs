namespace dotMVC.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("hoadon")]
    public partial class hoadon
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public hoadon()
        {
            cthoadons = new HashSet<cthoadon>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int masohd { get; set; }

        [Required]
        [StringLength(128)]
        public string makh { get; set; }

        [Column(TypeName = "date")]
        public DateTime ngaydat { get; set; }

        public int tongtien { get; set; }

        [StringLength(500)]
        public string diachi { get; set; }

        [StringLength(200)]
        public string sdt { get; set; }
        public int? mavc { get; set; }

        public int matt { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cthoadon> cthoadons { get; set; }

        public virtual tinhtrang tinhtrang { get; set; }

        public virtual voucher voucher { get; set; }
    }
}
