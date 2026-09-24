namespace Accounting.Shared.Results;

public class PagedResult<T> : Result, IDataResult<IReadOnlyList<T>>
{
    #region Constructor
    private PagedResult(
        string status ,
        IReadOnlyList<T> items ,
        int pageNumber ,
        int pageSize ,
        int totalCount ,
        string message ,
        IReadOnlyList<Error> errors)
        : base(status , message , errors)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
    #endregion Constructor

    #region Properties
    public IReadOnlyList<T> Items { get; }
    public IReadOnlyList<T> Data => Items;
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => PageSize > 0 ? (TotalCount + PageSize - 1) / PageSize : 0;
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
    #endregion Properties

    #region Operations
    public static PagedResult<T> Success(
        IReadOnlyList<T> items ,
        int pageNumber ,
        int pageSize ,
        int totalCount ,
        string message = "")
    {
        return new PagedResult<T>(
            ResultStatus.Success ,
            items ,
            pageNumber ,
            pageSize ,
            totalCount ,
            message ,
            []);
    }

    public static new PagedResult<T> Failure(string message , string status = ResultStatus.InternalError)
    {
        return new PagedResult<T>(
            status ,
            [] ,
            0 ,
            0 ,
            0 ,
            message ,
            [new Error(status , message , status , string.Empty)]);
    }

    public static new PagedResult<T> NotFound(string message)
    {
        return Failure(message , ResultStatus.NotFound);
    }
    #endregion Operations
}