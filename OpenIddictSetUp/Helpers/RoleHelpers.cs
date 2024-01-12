namespace OpenIddictSetUp.Helpers
{
    public static class RoleHelpers
    {
        public static Guid SYS_ADMIN_ID() => Guid.Parse("f564a1ff-03b4-4c74-a1a3-928ce780ee7a");
        public const string SYS_ADMIN = nameof(SYS_ADMIN);

        public static List<string> GetAll()
        {
            return new List<string>
            {
                SYS_ADMIN,
            };
        }
    }
}
