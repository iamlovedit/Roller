using Mapster;

namespace Roller.Infrastructure;

public class PageData<T>
{
    public PageData()
    {
    }

    public PageData(int pageIndex, int pageCount, int dataCount, int pageSize, List<T> data)
    {
        Page = pageIndex;
        PageCount = pageCount;
        DataCount = dataCount;
        PageSize = pageSize;
        Data = data;
    }

    public int Page { get; set; }

    public int PageCount { get; set; }

    public int DataCount { get; set; }

    public int PageSize { get; set; }

    public List<T> Data { get; set; }

    public PageData<TResult> ConvertTo<TResult>()
    {
        var result = new PageData<TResult>(Page, PageCount, DataCount, PageSize, Data?.Adapt<List<TResult>>());
        return result;
    }
}