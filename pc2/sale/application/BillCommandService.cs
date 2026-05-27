using pc2_7420_u20231f226.sale.domain.model.agreggates;
using pc2_7420_u20231f226.sale.domain.model.valueobjects;
using pc2_7420_u20231f226.sale.domain.repositories;
using pc2_7420_u20231f226.sale.domain.service;
using pc2_7420_u20231f226.sale.interfaces.REST.resources;

namespace pc2_7420_u20231f226.sale.application;

public class BillCommandService : IBillCommandService
{
    private readonly IbillRepository _repository;

    /// <summary>
    /// Initializes a new instance of BillCommandService
    /// </summary>
    /// <param name="repository">Bill repository</param>
    public BillCommandService(IbillRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the creation of a new bill
    /// </summary>
    public async Task<BillResource> Handle(CreateBillResource resource)
    {
        // Validate business rules
        
        // 1. Validate customer (required, max 100 characters)
        if (string.IsNullOrWhiteSpace(resource.Customer) || resource.Customer.Length > 100)
            throw new ArgumentException("Customer is required and must be maximum 100 characters.");

        // 2. Validate adviser (required, max 100 characters)
        if (string.IsNullOrWhiteSpace(resource.Adviser) || resource.Adviser.Length > 100)
            throw new ArgumentException("Adviser is required and must be maximum 100 characters.");

        // 3. Validate plate (max 10 characters)
        if (!string.IsNullOrWhiteSpace(resource.Plate) && resource.Plate.Length > 10)
            throw new ArgumentException("Plate must be maximum 10 characters.");

        // 4. Validate amount (must be greater than 0)
        if (resource.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        // 5. Validate emission date (cannot be less than system date)
        if (resource.Emission < DateTime.UtcNow.Date)
            throw new ArgumentException("Emission date cannot be less than the current system date.");

        // 6. Validate service ID is within allowed values
        if (!Enum.IsDefined(typeof(EService), resource.ServiceId))
            throw new ArgumentException("Invalid service ID.");

        // 7. Validate invoice serial number (required, max 10 characters)
        if (string.IsNullOrWhiteSpace(resource.InvoiceSerialNumber) || resource.InvoiceSerialNumber.Length > 10)
            throw new ArgumentException("Invoice serial number is required and must be maximum 10 characters.");

        // 8. Validate invoice sequential number (required, max 10 characters)
        if (string.IsNullOrWhiteSpace(resource.InvoiceSequentialNumber) || resource.InvoiceSequentialNumber.Length > 10)
            throw new ArgumentException("Invoice sequential number is required and must be maximum 10 characters.");

        // 9. Check if bill with same invoice already exists
        if (await _repository.ExistByInvoiceAsync(resource.InvoiceSerialNumber, resource.InvoiceSequentialNumber))
            throw new InvalidOperationException("A bill with the same invoice already exists.");

        var bill = new bill
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

        await _repository.AddAsync(bill);

        return new BillResource
        {
            BillNumber = bill.BillNumber,
            Customer = bill.Customer,
            ServiceId = bill.ServiceId,
            Emission = bill.Emission,
            InvoiceSerialNumber = bill.Invoice.SerialNumber,
            InvoiceSequentialNumber = bill.Invoice.SequentialNumber,
            Amount = bill.Amount
        };
    }
}