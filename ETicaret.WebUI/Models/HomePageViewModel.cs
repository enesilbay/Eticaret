using ETicaret.Core.Entities;
using ETicaret.WebUI.ViewComponents;

namespace ETicaret.WebUI.Models
{
    public class HomePageViewModel
    {
        internal List<News> News;

        public List<Slider>? Sliders { get; set; }
        public List<Product>? Products { get; set; }
        public List<News>? news { get; set; }
    }
}
