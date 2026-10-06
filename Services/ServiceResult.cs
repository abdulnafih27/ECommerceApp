public class ServiceResult
{
    public bool IsSuccess {get; protected set;}
    public string? Error {get; protected set;}
    public ErrorType ErrorType {get; protected set;}

    public static ServiceResult Success() => new() { IsSuccess = true }; 
    public static ServiceResult Failed(string error, ErrorType errorType) => new() { IsSuccess = false, Error = error, ErrorType = errorType}; 
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data {get; private set;} 
    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data }; 

    new public static ServiceResult<T> Failed(string error, ErrorType errorType) => new() { IsSuccess = false, Error = error, ErrorType = errorType}; 
}