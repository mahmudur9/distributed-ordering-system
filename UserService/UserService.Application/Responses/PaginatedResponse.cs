namespace UserService.Application.Responses;

public sealed class PaginatedResponse<T> where T : class
{
    public IEnumerable<T> Data { get; private set; } = [];
    public int PageNumber { get; private set; }
    public int ItemsPerPage { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalItems { get; private set; }

    public PaginatedResponse(IEnumerable<T> data, int totalItems, double itemsPerPage, int pageNumber)
    {
        if (itemsPerPage <= 0)
        {
            itemsPerPage = 2;
        }

        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        PageNumber = pageNumber;
        Data = data;
        TotalItems = totalItems;
        ItemsPerPage = (int)itemsPerPage;
        TotalPages = (int)Math.Ceiling(totalItems / itemsPerPage);
    }
}