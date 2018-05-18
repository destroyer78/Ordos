using Ordos.Core.Models;

namespace Ordos.IEDService.Services
{
    interface IConnectionService
    {
        void GetComtrades();
        bool TestConnection(Device device);
    }
}
