using System.ComponentModel.DataAnnotations.Schema;

namespace pc2_7420_u20231f226.sale.domain.model.agreggates;

public partial class bilordersaudit
{
    [Column("CreatedAt")] 
    public DateTime CreatedDate { get; set; }
    
    [Column("UpdatedAt")] 
    public DateTime UpdatedDate { get; set; }
}