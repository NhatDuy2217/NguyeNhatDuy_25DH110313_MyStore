using System.Collections.Generic;
using PagedList;

namespace NguyeNhatDuy_25DH110313_MyStore.Models.ViewModel
{
    public class HomeProductVM
    {
        // Danh sách 10 sản phẩm bán chạy nhất
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách 20 sản phẩm mới, có phân trang
        public IPagedList<Product> NewProducts { get; set; }

        public HomeProductVM()
        {
            FeaturedProducts = new List<Product>();
        }
    }
}