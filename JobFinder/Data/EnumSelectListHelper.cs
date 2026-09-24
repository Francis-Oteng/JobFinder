using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace JobFinder.Data
{
    public static class EnumSelectListHelper
    {

        public static List<SelectListItem> GetSelectList<TEnum>(
          bool includeEmpty = false, string emptyText = "-- Select --")
          where TEnum : struct, Enum
        {
            var list = Enum.GetValues<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = GetDisplayName(e)
                })
                .ToList();

            if (includeEmpty)
                list.Insert(0, new SelectListItem { Value = "", Text = emptyText });

            return list;
        }

        private static string GetDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }
    }
}
