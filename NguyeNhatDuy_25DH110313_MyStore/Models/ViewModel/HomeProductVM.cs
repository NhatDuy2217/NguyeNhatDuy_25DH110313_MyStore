using System.Collections.Generic;
using PagedList;

namespace NguyeNhatDuy_25DH110313_MyStore.Models.ViewModel
{
    public class HomeProductVM
    {
        // Danh sách sách bán chạy
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sách mới có phân trang
        public IPagedList<Product> NewProducts { get; set; }

        // Danh sách danh mục sách
        public List<Category> Categories { get; set; }

        public HomeProductVM()
        {
            FeaturedProducts = new List<Product>();
            Categories = new List<Category>();
        }
    }
}