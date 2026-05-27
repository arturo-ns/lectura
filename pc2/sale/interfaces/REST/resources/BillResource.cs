using pc2_7420_u20231f226.sale.domain.model.valueobjects;

namespace pc2_7420_u20231f226.sale.interfaces.REST.resources;

public class BillResource
{
    public int BillNumber { get; set; }
    public string Customer { get; set; } = string.Empty;
    public EService ServiceId { get; set; }
    public DateTime Emission { get; set; }
    public string InvoiceSerialNumber { get; set; } = string.Empty;
    public string InvoiceSequentialNumber { get; set; } = string.Empty;
    public double Amount { get; set; }
}