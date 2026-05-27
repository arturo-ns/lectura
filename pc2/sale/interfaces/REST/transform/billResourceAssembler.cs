using pc2_7420_u20231f226.sale.domain.model.agreggates;
using pc2_7420_u20231f226.sale.domain.model.valueobjects;
using  pc2_7420_u20231f226.sale.interfaces.REST.resources;

namespace pc2_7420_u20231f226.sale.interfaces.REST.transform;

/// <summary>
/// Assembler for transforming Bill entities to Bill resources
/// </summary>
/// <remarks>
/// Alex Sanchez Ponce
/// </remarks>
public static class BillResourceAssembler
{
    /// <summary>
    /// Transforms a Bill entity to a BillResource
    /// </summary>
    /// <param name="bill">Bill entity to transform</param>
    /// <returns>Bill resource</returns>
    public static BillResource ToResource(bill bill) => new()
    {
        BillNumber = bill.BillNumber,
        Customer = bill.Customer,
        ServiceId = bill.ServiceId,
        Emission = bill.Emission,
        InvoiceSerialNumber = bill.Invoice.SerialNumber,
        InvoiceSequentialNumber = bill.Invoice.SequentialNumber,
        Amount = bill.Amount
        // Note: Plate, Adviser, and audit fields are intentionally excluded as per requirements
    };

    /// <summary>
    /// Transforms a CreateBillResource to a Bill entity
    /// </summary>
    /// <param name="resource">Create bill resource to transform</param>
    /// <returns>Bill entity</returns>
    public static bill ToEntity(CreateBillResource resource) => new()
    {
        Customer = resource.Customer,
        ServiceId = resource.ServiceId,
        Plate = resource.Plate,
        Emission = resource.Emission,
        Invoice = new Invoice
        {
            SerialNumber = resource.InvoiceSerialNumber,
            SequentialNumber = resource.InvoiceSequentialNumber
        },
        Amount = resource.Amount,
        Adviser = resource.Adviser,
        CreatedDate = DateTime.UtcNow,  
        UpdatedDate = DateTime.UtcNow
    };
}