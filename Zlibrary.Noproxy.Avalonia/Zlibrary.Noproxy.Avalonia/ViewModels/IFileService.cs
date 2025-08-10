using System.Threading.Tasks;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public interface IFileService
    {
        Task<string> CreateAndWriteTextFileAsync(string fileName, string content);
        Task<string> CreateAndWriteBinaryFileAsync(string fileName, byte[] data);
        Task<bool> OpenFileAsync(string filePath);
    }
}