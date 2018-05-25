using Ordos.Core.Models;

namespace Ordos.IEDService
{
    interface IConnectionService
    {
        void GetComtrades();
        bool TestConnection(Device device);
    }
}
