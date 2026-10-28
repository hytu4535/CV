namespace ShopMvc.Helpers;

public static class Permissions
{
    public const string DashboardView = "Dashboard.View";
    public const string CategoryView = "Category.View", CategoryManage = "Category.Manage";
    public const string ProductView = "Product.View", ProductManage = "Product.Manage";
    public const string OrderView = "Order.View", OrderUpdate = "Order.Update";
    public const string UserView = "User.View", UserManage = "User.Manage";
    public const string RoleView = "Role.View", RoleManage = "Role.Manage";
    public static readonly string[] All = [DashboardView, CategoryView, CategoryManage, ProductView, ProductManage, OrderView, OrderUpdate, UserView, UserManage, RoleView, RoleManage];
}