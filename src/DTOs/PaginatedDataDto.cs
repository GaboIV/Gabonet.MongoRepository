namespace Gabonet.MongoRepository.DTOs;

public class PaginatedDataDto<T>
{
    public List<T>? Items { get; set; }
    public PaginationInfo? Pagination { get; set; }
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}