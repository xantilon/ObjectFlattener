using System.Collections;
using System.Reflection;

namespace ObjectFlattener
{
    public static class ToDictionaryExtension
    {
        public static Dictionary<string, string> Flatten<T>(this T obj) where T : class
        {
            var dict = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();
            foreach(var property in properties)
            {
                var value = property.GetValue(obj);
                if(value is null) continue;
                if(value is IList list)
                {
                    var nestedDict = FlattenNestedList(list, $"{property.Name}");
                    foreach(var kvp in nestedDict)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    dict[property.Name] = value.ToString() ?? "";
                }
            }
            return dict;
        }


        public static Dictionary<string, string> FlattenList<T>(this IList<T>? list, string prefix)
        {
            if(list is null)
                return [];

            var dict = new Dictionary<string, string>();
            for(int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var properties = typeof(T).GetProperties();
                foreach(var property in properties)
                {
                    var value = property.GetValue(item);
                    if(value is null) continue;
                    if(value is IList nestedList)
                    {
                        var nestedPrefix = $"{prefix}:{i}:{property.Name}";
                        var nestedDict = FlattenNestedList(nestedList, nestedPrefix);
                        foreach(var kvp in nestedDict)
                        {
                            dict[kvp.Key] = kvp.Value;
                        }
                    }
                    else
                    {
                        dict[$"{prefix}:{i}:{property.Name}"] = value.ToString() ?? "";
                    }
                }
            }
            return dict;
        }
        private static Dictionary<string, string> FlattenNestedList(IList nestedList, string prefix)
        {
            var elementType = nestedList.GetType().GetGenericArguments()[0];
            var method = typeof(ToDictionaryExtension).GetMethod(nameof(FlattenList), BindingFlags.Static | BindingFlags.Public);
            var genericMethod = method?.MakeGenericMethod(elementType)
                ?? throw new InvalidOperationException($"Could not create generic method for type {elementType.Name}");

            return (Dictionary<string, string>)(genericMethod?.Invoke(null, new object[] { nestedList, prefix })
                ?? throw new InvalidOperationException("Failed to invoke generic method"));
        }


        public static T Unflatten<T>(this Dictionary<string, string> dict) where T : new()
        {
            var item = new T();
            var properties = typeof(T).GetProperties();
            foreach(var property in properties)
            {
                var key = property.Name;
                if(typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    var elementType = property.PropertyType.GetGenericArguments()[0];
                    var nestedList = UnflattenNestedList(dict, key, elementType);
                    property.SetValue(item, nestedList);
                }
                else if(dict.TryGetValue(key, out var value))
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(item, convertedValue);
                }
            }
            return item;
        }


        public static List<T> UnflattenList<T>(this Dictionary<string, string> dict, string prefix) where T : new()
        {
            var list = new List<T>();
            int groupIndex = 1;
            
            var groupedDict = dict
                .Where(kvp => kvp.Key.StartsWith(prefix))
                .GroupBy(kvp => kvp.Key.Substring(prefix.Length - 1).Split(':')[groupIndex])
                .ToDictionary(g => g.Key, g => g.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            foreach(var groupKey in groupedDict.Keys)
            {
                var item = new T();
                var properties = typeof(T).GetProperties();
                foreach(var property in properties)
                {
                    var key = $"{prefix}:{groupKey}:{property.Name}";
                    
                    if(typeof(IList).IsAssignableFrom(property.PropertyType))
                    {

                        var elementType = property.PropertyType.GetGenericArguments()[0];
                        var nestedList = UnflattenNestedList(groupedDict[groupKey], key, elementType);
                        property.SetValue(item, nestedList);
                        
                    }
                    else
                    {
                        if(groupedDict[groupKey].TryGetValue(key, out var value))
                        {
                            var convertedValue = Convert.ChangeType(value, property.PropertyType);
                            property.SetValue(item, convertedValue);
                        }
                    }
                }

                list.Add(item);
            }
            return list;
        }

        private static IList UnflattenNestedList(Dictionary<string, string> dict, string prefix, Type elementType)
        {
            var method = typeof(ToDictionaryExtension).GetMethod(nameof(UnflattenList), BindingFlags.Static | BindingFlags.Public);
            var genericMethod = method?.MakeGenericMethod(elementType)
                ?? throw new InvalidOperationException($"Could not create generic method for type {elementType.Name}");

            return (IList)(genericMethod?.Invoke(null, new object[] { dict, prefix })
                ?? throw new InvalidOperationException("Failed to invoke generic method"));
        }
    }
}
