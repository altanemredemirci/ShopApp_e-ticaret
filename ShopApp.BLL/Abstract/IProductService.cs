using ShopApp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopApp.BLL.Abstract
{
    public interface IProductService
    {
        Product GetById(int id);

        Product GetProductDetails(int id); 

        List<Product> GetAll();

        List<Product> GetProductsByCategory(string category,int page, int pageSize);

        void Create(Product entity);
        void Update(Product entity);
        void Delete(Product entity);
        int GetCountByCategory(string category);
        Product GetByIdWithCategories(int id);
        void Update(Product entity, int[] categoryIds);
    }
}
