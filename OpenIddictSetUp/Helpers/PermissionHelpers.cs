using OpenIddictSetUp.Enums;
using System.ComponentModel;
using System.Reflection;

namespace OpenIddictSetUp.Helpers
{
    public static class PermissionHelpers
    {
        public static string GetPermissionCategory(this Permission value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    CategoryAttribute? attr = Attribute.GetCustomAttribute(field,
                             typeof(CategoryAttribute)) as CategoryAttribute;

                    if (attr != null)
                    {
                        return attr.Category;
                    }
                }
            }

            return null;
        }
    }
}
