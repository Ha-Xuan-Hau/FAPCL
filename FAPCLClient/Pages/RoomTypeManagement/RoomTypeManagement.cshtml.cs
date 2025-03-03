using BookClassRoom.Hubs;
using FAPCL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookClassRoom.Pages.RoomTypeManagement
{
    public class RoomTypeModel : PageModel
    {
        private readonly BookClassRoomContext _context;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IHubContext<SignalRServer> _signalRHub;
        [BindProperty]
        public RoomType RoomType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public RoomTypeModel(UserManager<AspNetUser> userManager, BookClassRoomContext context, IHubContext<SignalRServer> signalRHub)
        {
            _context = context;
            _userManager = userManager;
            _signalRHub = signalRHub;
        }

        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            IQueryable<RoomType> query = _context.RoomTypes;

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(c => c.RoomType1.Contains(SearchQuery));
            }

            RoomTypes = await query.ToListAsync();
        }



        public async Task<IActionResult> OnPostCreate(RoomType roomType)
        {

            var user = await _userManager.GetUserAsync(User);
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdate(RoomType roomType)
        {
            var existingRoomType = await _context.RoomTypes.FindAsync(roomType.RoomTypeId);
            if (existingRoomType != null)
            {
                roomType.RoomTypeId = existingRoomType.RoomTypeId;
                existingRoomType.RoomType1 = roomType.RoomType1;
                existingRoomType.Description = roomType.Description;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
