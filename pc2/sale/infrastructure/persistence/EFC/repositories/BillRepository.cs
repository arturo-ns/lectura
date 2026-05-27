using Microsoft.EntityFrameworkCore;
using pc2_7420_u20231f226.sale.domain.model.agreggates;
using pc2_7420_u20231f226.sale.domain.repositories;
using pc2_7420_u20231f226.sale.infrastructure.persistence.EFC.context;

namespace pc2_7420_u20231f226.sale.infrastructure.persistence.EFC.repositories;

public class BillRepository : IbillRepository
{
    private readonly BillContext _context;
    
    public BillRepository(BillContext context)
    {
        _context = context;
    }

    public async Task<bill> AddAsync(bill bill)
    {
        await _context.Bills.AddAsync(bill);
        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task<bool> ExistByInvoiceAsync(string serialNumber, string sequentialNumber)
    {
        return await _context.Bills
            .AnyAsync(b => b.Invoice.SerialNumber == serialNumber && 
                           b.Invoice.SequentialNumber == sequentialNumber);
    }

    public async Task<bill?> GetByBillNumberAsync(int billNumber)
    {
        return await _context.Bills
            .FirstOrDefaultAsync(b => b.BillNumber == billNumber);
    }
}