using System.Collections;
using System.Reflection;

namespace ObjectFlattener
{
    public static class ListExtension
    {
        public static Dictionary<string, string> Flatten<T>(this List<T> list, string prefix)
        {
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
                        dict[$"{prefix}:{i}:{property.Name}"] = value.ToString();
                    }
                }
            }
            return dict;
        }
        private static Dictionary<string, string> FlattenNestedList(IList nestedList, string prefix)
        {
            var elementType = nestedList.GetType().GetGenericArguments()[0];
            var method = typeof(ListExtension).GetMethod(nameof(Flatten), BindingFlags.Static | BindingFlags.Public);
            var genericMethod = method?.MakeGenericMethod(elementType)
                ?? throw new InvalidOperationException($"Could not create generic method for type {elementType.Name}");

            return (Dictionary<string, string>)(genericMethod?.Invoke(null, new object[] { nestedList, prefix })
                ?? throw new InvalidOperationException("Failed to invoke generic method"));
        }

        public static List<T> UnflattenList<T>(this Dictionary<string, string> dict, string prefix) where T : new()
        {
            var list = new List<T>();
            var groupedDict = dict
                .Where(kvp => kvp.Key.StartsWith(prefix))
                .GroupBy(kvp => kvp.Key.Split(':')[1])
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
                        //key += ":0";
                        //if(groupedDict[groupKey].TryGetValue(key, out var value))
                        {
                            var elementType = property.PropertyType.GetGenericArguments()[0];
                            //var nestedPrefix = $"{prefix}:{groupKey}:{property.Name}";
                            var nestedList = UnflattenNestedList(dict, key, elementType);
                            property.SetValue(item, nestedList);
                        }
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
            var method = typeof(ListExtension).GetMethod(nameof(UnflattenList), BindingFlags.Static | BindingFlags.Public);
            var genericMethod = method?.MakeGenericMethod(elementType)
                ?? throw new InvalidOperationException($"Could not create generic method for type {elementType.Name}");

            return (IList)(genericMethod?.Invoke(null, new object[] { dict, prefix })
                ?? throw new InvalidOperationException("Failed to invoke generic method"));
        }
    }
}
