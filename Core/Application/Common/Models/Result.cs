namespace Application.Common.Models;

public record Result
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];

    public static Result Success(string message = "Operation successful") =>
        new() { IsSuccess = true, Message = message };

    public static Result Failure(string error, string message = "Operation failed") =>
        new() { IsSuccess = false, Message = message, Errors = [error] };

    public static Result Failure(string message, List<string> errors) =>
        new() { IsSuccess = false, Message = message, Errors = errors };    
}

public record Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T data, string message = "Operation successful") =>
        new() { IsSuccess = true, Message = message, Data = data };

    public new static Result<T> Failure(string error, string message = "Operation failed") =>
        new() { IsSuccess = false, Message = message, Errors = [error] };

    public new static Result<T> Failure(string message, List<string> errors) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}

public record Result<TData, TMeta> : Result<TData>
{
    public TMeta? MetaData { get; set; }

    public static Result<TData, TMeta> Success(TData data, TMeta metaData, string message = "Operation successful") =>
        new() { IsSuccess = true, Message = message, Data = data, MetaData = metaData };

    public new static Result<TData, TMeta> Failure(string error, string message = "Operation failed") =>
        new() { IsSuccess = false, Message = message, Errors = [error] };

    public new static Result<TData, TMeta> Failure(string message, List<string> errors) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}