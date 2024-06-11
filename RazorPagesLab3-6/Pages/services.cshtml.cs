using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesLab3.Pages
{
    public class servicesModel : PageModel
    {
        private IService<Product> _service;
        public IEnumerable<Product> Products { get; set; }
        public servicesModel(IService<Product> service)
        {
            this._service = service;
            Products = service.getContent("Product.json");
        }

        public void OnGet()
        {
        }
    }
}
