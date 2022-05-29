namespace VH.MiniService.Common.Application
{
    // user for scheduled or system operations
    public static class SystemUser
    {
        public static string Id => Email;
        public static string Email => "system@vh.com";
    }
}
