namespace Auth.BusinessContract.DTO
{
  public class TokensResponse
  {
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
  }
}
