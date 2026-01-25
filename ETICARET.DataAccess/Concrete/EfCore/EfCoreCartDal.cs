using ETICARET.DataAccess.Abstract;
using ETICARET.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.DataAccess.Concrete.EfCore
{
    public class EfCoreCartDal : EfCoreGenericRepository<Cart, DataContext>, ICartDal
    {
        public void ClearCart(string cartId)
        {
            using (var context = new DataContext())
            {
                var cmd = @"DELETE FROM CartItem WHERE CartId = @p0";
                context.Database.ExecuteSqlRaw(cmd, cartId);
            }
        }

        public void DeleteFromCart(int cartId, int productId)
        {
            using (var context = new DataContext())
            {
                var cmd = @"DELETE FROM CartItem WHERE CartId = @p0 AND ProductId = @p1";
                context.Database.ExecuteSqlRaw(cmd, cartId, productId);
            }
        }

        public Cart GetCartByUserId(string userId)
        {
            using (var context = new DataContext())
            {
                return context.Carts
                    .Include(i => i.CartItems) // Include ile CartItems tablosunu da ekliyoruz
                    .ThenInclude(i => i.Product) // ThenInclude ile CartItems içindeki Product tablosunu da ekliyoruz
                    .ThenInclude(i => i.Images) // ThenInclude ile Product içindeki Images tablosunu da ekliyoruz
                    .FirstOrDefault(i => i.UserId == userId);
            }
        }

        public override void Update(Cart entity)
        {
            using (var context = new DataContext())
            {
                context.Carts.Update(entity);
                context.SaveChanges();
            }
        }
    }
}
