using System.Collections;
using System.Reflection;
using Bogus;
using System.Linq;

namespace ObjectFlattener
{
	public static class ToDictionaryExtension
	{
		public static Dictionary<string, string> Flatten<T>(this T obj) where T : class
		{
			var dict = new Dictionary<string, string>();
			var properties = typeof(T).GetProperties().Where(p => p.GetIndexParameters().Length == 0);
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
				else if(value is Dictionary<string, string> nestedDict)
				{
					var flattenedDict = nestedDict.FlattenDictionary(property.Name);
					foreach(var kvp in flattenedDict)
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
					else if(value is Dictionary<string, string> nestedDict)
					{
						var flattenedDict = nestedDict.FlattenDictionary(property.Name);
						foreach(var kvp in flattenedDict)
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

		public static Dictionary<string, string> FlattenDictionary(this Dictionary<string, string> dict, string prefix = "")
		{
			var flattenedDict = new Dictionary<string, string>();
			foreach(var kvp in dict)
			{
				var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}:{kvp.Key}";
				flattenedDict[key] = kvp.Value;
			}
			return flattenedDict;
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
					if(property.CanWrite)
						property.SetValue(item, nestedList);					
				}
				else if(typeof(Dictionary<string, string>).IsAssignableFrom(property.PropertyType))
				{
					var nestedList = UnflattenDictionary(dict, key);
					if(property.CanWrite)
						property.SetValue(item, nestedList);					
				}
				else if(dict.TryGetValue(key, out var value))
				{
					var convertedValue = Convert.ChangeType(value, property.PropertyType);
					if(property.CanWrite)
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

		public static Dictionary<string, string> UnflattenDictionary(this Dictionary<string, string> dict, string prefix = "")
		{
			var unflattenedDict = new Dictionary<string, string>();
			foreach(var kvp in dict)
			{
				var key = string.IsNullOrEmpty(prefix) ? kvp.Key : kvp.Key.Replace($"{prefix}:", "");
				unflattenedDict[key] = kvp.Value;
			}
			return unflattenedDict;
		}

		/// <summary>
		/// not every property type is supported by the flattener
		/// the EnsureCompatibility method can be used to check if the type is supported
		/// it will make an instance of the type and fill every property with a value (using Bogus). 
		/// nested properties are filled to a max depth of 2. 
		/// then the object is flattened and unflattened.
		/// the unflattened object is compared to the original object property by property
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool EnsureCompatibility<T>(int depth = 0) where T : class, new()
		{
			const int MAXDEPTH = 2;

			if(depth > MAXDEPTH)
				return true;

			T dut = new T();
			var faker = new Faker<T>();

			var properties = typeof(T).GetProperties();
			foreach(var property in properties)
			{
				var propertyType = property.PropertyType;

				if(propertyType == typeof(string))
				{
					faker.RuleForType(propertyType, f => f.Lorem.Word());
				}
				else if(propertyType == typeof(int))
				{
					faker.RuleForType(propertyType, f => f.Random.Int());
				}
				else if(propertyType == typeof(bool))
				{
					faker.RuleForType(propertyType, f => f.Random.Bool());
				}
				else if(propertyType == typeof(DateTime))
				{
					faker.RuleForType(propertyType, f => f.Date.Past());
				}
				else if(propertyType == typeof(double))
				{
					faker.RuleForType(propertyType, f => f.Random.Double());
				}
				else if(propertyType == typeof(decimal))
				{
					faker.RuleForType(propertyType, f => f.Random.Decimal());
				}
				else if(propertyType == typeof(float))
				{
					faker.RuleForType(propertyType, f => f.Random.Float());
				}
				else if(propertyType == typeof(Guid))
				{
					faker.RuleForType(propertyType, f => f.Random.Guid());
				}
				else if(propertyType == typeof(byte))
				{
					faker.RuleForType(propertyType, f => f.Random.Byte());
				}
				else if(propertyType == typeof(short))
				{
					faker.RuleForType(propertyType, f => f.Random.Short());
				}
				else if(propertyType == typeof(long))
				{
					faker.RuleForType(propertyType, f => f.Random.Long());
				}
				else if(propertyType == typeof(char))
				{
					faker.RuleForType(propertyType, f => f.Random.Char());
				}
				else if(propertyType == typeof(byte[]))
				{
					faker.RuleForType(propertyType, f => f.Random.Bytes(10));
				}
				else if(propertyType == typeof(TimeSpan))
				{
					faker.RuleForType(propertyType, f => f.Date.Timespan());
				}
				else if(propertyType == typeof(DateTimeOffset))
				{
					faker.RuleForType(propertyType, f => f.Date.RecentOffset());
				}
				else if(propertyType == typeof(Uri))
				{
					faker.RuleForType(propertyType, f => f.Internet.Url());
				}
				else if(propertyType == typeof(Version))
				{
					faker.RuleForType(propertyType, f => f.System.Version());
				}
				else if(propertyType == typeof(HashSet<int>))
				{
					faker.RuleForType(propertyType, f => new HashSet<int>(f.Random.Int(1, 10)));
				}
				else if(propertyType == typeof(Dictionary<string, string>))
				{
					faker.RuleForType(propertyType, f => new Dictionary<string, string> {{ f.Lorem.Word(), f.Lorem.Word() }});
				}
				//else if(propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
				//{
				//	var elementType = propertyType.GetGenericArguments()[0];
				//	var listFakerType = typeof(Faker<>).MakeGenericType(elementType);
				//	dynamic? listFaker = Activator.CreateInstance(listFakerType) as dynamic;

				//	if(propertyType.GetProperty("Capacity") != null)
				//	{
				//		listFaker.RuleFor("Capacity", (Func<Faker, int>)(f => f.Random.Int(1, 10)));
				//	}
				//	if(propertyType.GetProperty("Count") != null)
				//	{
				//		listFaker.RuleFor("Count", (Func<Faker, int>)(f => f.Random.Int(1, 10)));
				//	}
				//	faker.RuleFor(property.Name, f => listFaker.Generate());
				//}
				else if(propertyType.IsClass && propertyType != typeof(string))
				{
					var nestedInstance = Activator.CreateInstance(propertyType);
					if(nestedInstance != null)
					{
						var nestedMethod = typeof(ToDictionaryExtension).GetMethod(nameof(EnsureCompatibility), BindingFlags.Static | BindingFlags.Public);
						var genericMethod = nestedMethod?.MakeGenericMethod(propertyType);
						var result = (bool?)genericMethod?.Invoke(null, new object[] { depth + 1 });
						if(result == false)
							return false;
					}
				}

				// Add more type checks as needed
			}

			T? fake = faker.Generate();

			var flat = fake.Flatten();
			var unflat = flat.Unflatten<T>();

			foreach(var property in properties)
			{
				var originalValue = property.GetValue(fake);
				var newValue = property.GetValue(unflat);
				if(originalValue is null && newValue is null)
					continue;

				if(property.PropertyType.IsGenericType
					&& property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
					&& originalValue is null
					&& newValue is ICollection)
				{
					continue;
				}				

				if(property.PropertyType.IsGenericType
					&& property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
				{
					if(originalValue is Dictionary<string, string> originalDict 
						&& newValue is Dictionary<string, string> newDict)
					{
						return originalDict.Count() == newDict.Count() &&
						originalDict.All(kvp => newDict.TryGetValue(kvp.Key, out string? value) && kvp.Value == value);
					}
				}

				return originalValue?.Equals(newValue) ?? false;
			}
			return true;
		}
	}
}
