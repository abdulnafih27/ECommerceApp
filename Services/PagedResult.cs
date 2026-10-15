public class PagedResult<T>
{
    public int PageItemsCount {get; private set;}
    public int PageNumber {get; private set;}
    public int PageSize {get; private set;}
    public int TotalItems {get; private set;}
    public int TotalPages {get; private set;}
    public List<T> Data {get; private set;} = new List<T>{};

    public static PagedResult<T> Create(List<T> data, Pagination pagination, int totalItems) =>
    new() {
        PageNumber = pagination.Page,
        PageItemsCount = data.Count,
        PageSize = pagination.PageSize,
        TotalItems = totalItems,
        TotalPages =  (totalItems + pagination.PageSize - 1) / pagination.PageSize,
        Data = data, 
    };
}