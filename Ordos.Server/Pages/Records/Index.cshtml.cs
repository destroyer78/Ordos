using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ordos.Core.Models;
using Ordos.DataService.Data;

namespace DRM.Pages_Records
{
    public class IndexModel : PageModel
    {
        private readonly SystemContext _context;

        public IndexModel(SystemContext context)
        {
            _context = context;
        }

        public IList<DisturbanceRecording> Records { get;set; }

        public async Task OnGetAsync()
        {
            Records = await _context.DisturbanceRecordings.ToListAsync();
        }
    }
}
