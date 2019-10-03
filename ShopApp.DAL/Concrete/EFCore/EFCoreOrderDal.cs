using Microsoft.EntityFrameworkCore;
using ShopApp.DAL.Abstract;
using ShopApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopApp.DAL.Concrete.EFCore
{
    public class EFCoreOrderDal : EFCoreGenericRepository<Order, ShopContext>, IOrderDal
    {
        public List<Order> GetOrders(string userId)
        {
            using(var context = new ShopContext())
            {
                var orders = context.Orders
                    .Include(i => i.OrderItems)
                    .ThenInclude(i => i.Product)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                {
                    orders = orders.Where(i => i.UserId == userId);
                }

                return orders.ToList();
            }
        }
    }
}
