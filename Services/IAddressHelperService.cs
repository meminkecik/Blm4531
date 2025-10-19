namespace Nearest.Services
{
    public interface IAddressHelperService
    {
        Task<(bool Success, string Message)> FetchRemoteAddressAsync();
    }
}
