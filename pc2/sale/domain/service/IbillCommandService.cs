using pc2_7420_u20231f226.sale.interfaces.REST.resources;

namespace pc2_7420_u20231f226.sale.domain.service;

/// <summary>
/// Command service interface for bill operations
/// </summary>
/// <remarks>
/// Alex Sanchez Ponce
/// </remarks>
public interface IBillCommandService
{
    /// <summary>
    /// Handles the creation of a new bill
    /// </summary>
    /// <param name="resource">The bill creation resource</param>
    /// <returns>The created bill resource</returns>
    Task<BillResource> Handle(CreateBillResource resource);
}