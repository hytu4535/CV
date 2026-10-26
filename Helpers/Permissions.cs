namespace ShopMvc.Helpers;

public static class Permissions
{
    public const string CategoryView = "Category.View", CategoryManage = "Category.Manage";
    public const string ProductView = "Product.View", ProductManage = "Product.Manage";
    public const string OrderView = "Order.View", OrderUpdate = "Order.Update";
    public const string UserManage = "User.Manage", RoleManage = "Role.Manage";
    public static readonly string[] All = [CategoryView, CategoryManage, ProductView, ProductManage, OrderView, OrderUpdate, UserManage, RoleManage];
}