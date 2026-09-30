namespace Ecommerce.Web.ViewModels.Products;

/// <summary>
/// Paginated collection of products.
/// </summary>
public sealed class PaginatedProductsViewModel
{
    /// <summary>
    /// The products on the current page.
    /// </summary>
    public IReadOnlyList<ProductSummaryViewModel> Items { get; set; } = [];

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of products (across all pages).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (TotalCount + PageSize - 1) / PageSize : 0;

    /// <summary>
    /// Whether there is a next page available.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
