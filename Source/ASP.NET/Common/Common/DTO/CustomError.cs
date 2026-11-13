namespace Common.DTO
{
  public class CustomError
  {
    public int Code { get; init; }
    public string Message { get; init; } = string.Empty;
    public int HttpStatusCode { get; init; }

    // Allows overriding the default static message with dynamic runtime details
    public CustomError WithMessage(string dynamicMessage)
    {
      return new CustomError
      {
        Code = this.Code,
        Message = dynamicMessage,
        HttpStatusCode = this.HttpStatusCode
      };
    }
  }
}
