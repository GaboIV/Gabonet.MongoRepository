namespace Gabonet.MongoRepository.Abstractions.Request;

public class PagedRequest
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public string? OrderColumn { get; set; }
    public string? OrderBy { get; set; }
    public Dictionary<string, string>? Filters { get; set; }
}