using pc2_7420_u20231f226.sale.domain.model.valueobjects;

namespace pc2_7420_u20231f226.sale.domain.model.agreggates;

public partial class bill : bilordersaudit
{
    public int BillNumber { get; set; }
    public string Customer { get; set; } = string.Empty;
    public EService ServiceId { get; set; }
    public string Plate { get; set; } = string.Empty;
    public DateTime Emission { get; set; }
    public Invoice Invoice { get; set; } = new Invoice();
    public double Amount { get; set; }
    public string Adviser { get; set; } = string.Empty;
}