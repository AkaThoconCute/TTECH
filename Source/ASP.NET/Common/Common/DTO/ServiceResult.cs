namespace Common.DTO
{
  // IServiceResult
  public interface IServiceResult
  {
    bool Success { get; }
    CustomError? Error { get; }
  }

  // ServiceResult<T>
  public class ServiceResult<T> : IServiceResult
  {
    public bool Success { get; init; }
    public T? Result { get; init; }
    public CustomError? Error { get; init; }

    // Factory methods
    public static ServiceResult<T> FromResult(T result)
      => new() { Success = true, Result = result, Error = null };

    public static ServiceResult<T> FromError(CustomError error)
      => new() { Success = false, Result = default, Error = error };
  }

  // ServiceResult
  public class ServiceResult
  {
    public static ServiceResult<T> FromResult<T>(T result)
      => ServiceResult<T>.FromResult(result);

    public static ServiceResult<T> FromError<T>(CustomError error)
      => ServiceResult<T>.FromError(error);
  }
}
