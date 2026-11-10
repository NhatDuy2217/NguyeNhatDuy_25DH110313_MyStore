using System.Web;
using System.Web.Mvc;

namespace NguyeNhatDuy_25DH110313_MyStore
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
