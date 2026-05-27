using pc2_7420_u20231f226.sale.domain.model.agreggates;

namespace pc2_7420_u20231f226.sale.domain.repositories;

public interface IbillRepository
{
    /// <summary>
    /// Adds a new bill asynchronously
    /// </summary>
    /// <param name="bill">Bill entity to add</param>
    /// <returns>Added bill with generated bill number</returns>
    Task<bill> AddAsync(bill bill);
    
    /// <summary>
    /// Checks if a bill exists with the same invoice details
    /// </summary>
    /// <param name="serialNumber">Invoice serial number</param>
    /// <param name="sequentialNumber">Invoice sequential number</param>
    /// <returns>True if bill exists, otherwise false</returns>
    Task<bool> ExistByInvoiceAsync(string serialNumber, string sequentialNumber);
    
    /// <summary>
    /// Gets a bill by bill number
    /// </summary>
    /// <param name="billNumber">Bill number to search for</param>
    /// <returns>Bill if found, otherwise null</returns>
    Task<bill?> GetByBillNumberAsync(int billNumber);
}