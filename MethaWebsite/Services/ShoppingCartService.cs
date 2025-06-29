using MethaWebsite.Components.ClientPages;
using MethaWebsite.Data;

namespace MethaWebsite.Services
{
    public class ShoppingCartService
    {
        private ShoppingCart? ShoppingCart { get; set; }

        public ShoppingCart getShoppingCart() {
            if (ShoppingCart is null)
            {
                ShoppingCart = new();
                ShoppingCart.ProductIds = new List<string>();
            }
            return ShoppingCart;
        }
        public void AddToShoppingCart(Product product)
        {
            ShoppingCart.ProductIds.Add(product.Id);
            ShoppingCart.Subtotal += product.Price; 
        }
        public void RemoveFromShoppingCart(Product product)
        {
            ShoppingCart.ProductIds.Remove(product.Id);
            ShoppingCart.Subtotal -= product.Price;
        }
    }
}
