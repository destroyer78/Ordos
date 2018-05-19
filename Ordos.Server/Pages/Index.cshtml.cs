using System.Collections.Generic;
using System.Threading.Tasks;
using Ordos.Core.Models;
using Ordos.DataService.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Ordos.Server.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SystemContext _context;
        public IList<Device> Device { get; set; }
        public IList<DisturbanceRecording> Records { get; set; }

        public IndexModel(SystemContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Device = await _context.Devices
                .ToListAsync();

            Records = await _context.DisturbanceRecordings.ToListAsync();
        }
    }
}
