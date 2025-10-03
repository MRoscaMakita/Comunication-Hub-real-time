using Comunication_Hub_real_time.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comunication_Hub_real_time.Pages
{
    public class RenderChatModel : PageModel
    {
        private readonly RenderChatService _chatService;
        
        public RenderChatModel(RenderChatService chatService)
        {
            _chatService = chatService;
        }
        
        public async Task OnGetAsync()
        {
            // Connect to the render server when the page loads
            await _chatService.ConnectAsync();
        }
    }
}
