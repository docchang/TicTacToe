using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;


public static class Utility
{
	public static string RandomString(int length)
	{
		var random = new System.Random();
		return new string( Enumerable.Range(0, length)
			.Select(s => (char)random.Next((int)'!', (int)'~'))
			.ToArray());
	}

	public static bool RandomBoolean()
	{
		return UnityEngine.Random.value > 0.5f;
	}

	public static bool ByteArraysEqual(byte[] b1, byte[] b2)
	{
		if (b1 == b2) return true;
		if (b1 == null || b2 == null) return false;
		if (b1.Length != b2.Length) return false;
		for (int i=0; i < b1.Length; i++)
		{
			if (b1[i] != b2[i]) return false;
		}
		return true;
	}

	public static object CreateInstance(string strFullyQualifiedName)
	{
		System.Type t = System.Type.GetType(strFullyQualifiedName);
		return  System.Activator.CreateInstance(t);
	}
		
	public static IEnumerable<T> EnumEnumerable<T>()
	{
		if (!typeof(T).IsEnum)
		{
			return null;
		}
		return System.Enum.GetValues(typeof(T)).Cast<T>();
	}

	public static T RandomEnum<T>()
	{
		var l = EnumEnumerable<T>();
		if (l == null)
		{
			return default(T);
		}
		return l.RandomOne<T>();
	}

	public static int EnumCount<T>()
	{
		if (!typeof(T).IsEnum)
		{
			return 0;
		}
		return System.Enum.GetNames(typeof(T)).Length;
	}
}

public static class MethodExtensionForMonoBehaviourTransform
{
	/// <summary>
	/// Gets or add a component. Usage example:
	/// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
	/// </summary>
	static public T GetOrAddComponent<T> (this Component child) where T: Component
	{
		T result = child.GetComponent<T>();
		if (result == null) {
			result = child.gameObject.AddComponent<T>();
		}
		return result;
	}
}

public static class DictionaryMethods
{
	public static TValue ValueAtOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		TValue value;
		return dictionary.TryGetValue(key, out value) ? value : default(TValue);
	}
	public static TValue ValueAtOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
	{
		TValue value;
		return dictionary.TryGetValue(key, out value) ? value : defaultValue;
	}
}

public static class ExtensionMethods
{
	// Deep clone
	public static T DeepClone<T>(this T a)
	{
		using (MemoryStream stream = new MemoryStream())
		{
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, a);
			stream.Position = 0;
			return (T) formatter.Deserialize(stream);
		}
	}

	// usage: CustomDebug.Log(source.ToString<T>());
	public static string ToString<T>(this IEnumerable<T> source, string delimiter = ",")
	{
		if (source.IsNullOrEmpty()) { return null; }
		return string.Join(delimiter, source.Select(s => s.ToString()).ToArray());
	}

	// converting string Enum value to the corrosponding Enum type object
	public static T ToEnum<T>(this string obj)
	{
		// error check enum type
		if (!obj.IsEnumType<T>()) { return default(T); }

		T enumVal= (T)System.Enum.Parse(typeof(T), obj.ToString());
		return enumVal;
	}

	public static IEnumerable<Vector2> ToVector2(this IEnumerable<int> source)
	{
		// return ToVector2(source.Select(v => (float)v));
		if (!source.IsNullOrEmpty())
		{
			float i = 1f;
			foreach(var item in source)
			{
				yield return new Vector2(i++, (float)item);
			}
		}
	}

	public static IEnumerable<T> ToEnumerable<T>(this T item)
	{
		yield return item;
	}

	public static IEnumerable<Vector2> ToVector2(this IEnumerable<float> source)
	{
		if (!source.IsNullOrEmpty())
		{
			float i = 1f;
			foreach(var item in source)
			{
				yield return new Vector2(i++, item);
			}
		}
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
	{
		return source == null || !source.Any();
	}

	public static T RandomOne<T>(this IEnumerable<T> source)
	{
		return source.Skip(Random.Range(0,source.Count())).FirstOrDefault();
	}

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
	{
		if (source.IsNullOrEmpty()) return Enumerable.Empty<T>();
		return source.ShuffleIterator();
	}

	private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source)
	{
		var buffer = source.ToList();

		for (int i = 0; i < buffer.Count; i++)
		{
			int j = Random.Range(i, buffer.Count);
			yield return buffer[j];
			buffer[j] = buffer[i];
		}
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		for (int n = 0; n < list.Count; n++)
		{
			int i = Random.Range(n, list.Count);
			T temp = list[i];
			list[i] = list[n];
			list[n] = temp;
		}
	}

	public static IEnumerator SyncList<T>(this List<T> list, List<T> masterList, float wait = 0.05f)
	{
		if (list == null) { list = new List<T>(); }
		if (masterList.IsNullOrEmpty()) { list.Clear(); yield return null; }

		int index = 0;
		for (; index < list.Count && index < masterList.Count; index++)
		{
			list[index] = masterList[index];
			yield return new WaitForSeconds(wait);
		}
		// list is longer than masterList
		if (index < list.Count) {
			// remove index -> n
			list.RemoveRange(index, list.Count-index);
		}
		// masterList is longer than list
		else if (index < masterList.Count) {
			// add
			for (; index < masterList.Count; index++) {
				list.Add(masterList[index]);
				yield return new WaitForSeconds(wait);
			}
		}
	}

	// ValidateCount
	// list
	public static bool ValidateCount<T>(this IList<T> source, int count)
	{
		return source != null && source.Count == count;
	}
	// array
	public static bool ValidateCount<T>(this T[] source, int count)
	{
		return source != null && source.Length == count;
	}
	// IEnumerable
	public static bool ValidateCount<T>(this IEnumerable<T> source, int count)
	{
		return source != null && source.Count() == count;
	}

	public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
	{
		if (source == null) return;
		if (action == null) return;
		foreach (T item in source) {
			action(item);
		}
	}

	public static bool TypeCheck<T>(this T a, object obj, System.Func<T, bool> func = null) where T : class
	{
		if (obj == null || a.GetType() != obj.GetType()) {
			return false;
		}
		T objT = obj as T;
		if ((System.Object)objT == null) {
			return false;
		}
		if (func == null) { return true; }
		return func(objT);
	}

	public static bool IsEnumType<T>(this string a)
	{
		System.Type enumType = typeof(T);

		// make sure T is a enum type
		if (!enumType.IsEnum) { return false; }

		// enum string check
		if (!System.Enum.IsDefined(enumType, a)) { return false; }

		return true;
	}
}

public static class BoundsExtension
{
	public static float RandomX(this UnityEngine.Bounds bounds, float radius = 0f)
	{
		return Random.Range(bounds.min.x + radius, bounds.max.x - radius);
	}
	public static float RandomY(this UnityEngine.Bounds bounds, float radius = 0f)
	{
		return Random.Range(bounds.min.y + radius, bounds.max.y - radius);
	}
	public static float RandomZ(this UnityEngine.Bounds bounds, float radius = 0f)
	{
		return Random.Range(bounds.min.z + radius, bounds.max.z - radius);
	}
	// generate 2D point
	public static Vector3 RandomPoint(this UnityEngine.Bounds bounds, float radius = 0f)
	{
		return new Vector3(bounds.RandomX(radius),bounds.RandomY(radius));
	}
	// generate 3D position
	public static Vector3 RandomPosition(this UnityEngine.Bounds bounds, float radius = 0f)
	{
		return new Vector3(bounds.RandomX(radius),bounds.RandomY(radius),bounds.RandomZ(radius));
	}
}

public static class DateTimeExtension
{
	public static readonly System.DateTime DateTime1970 = new System.DateTime(1970, 1, 1);

	public static double TimeIntervalSince1970(this System.DateTime dateTime)
	{
		System.TimeSpan unixTime = dateTime - DateTime1970;
		return unixTime.TotalSeconds;
	}

	public static bool IsFuture(this System.DateTime dateTime)
	{
		return dateTime.IsFutureOf(System.DateTime.Now);
	}

	public static bool IsPast(this System.DateTime dateTime)
	{
		return dateTime.IsPastOf(System.DateTime.Now);
	}

	public static bool IsPresent(this System.DateTime dateTime)
	{
		return dateTime.IsPresentOf(System.DateTime.Now);
	}

	public static bool IsFutureOf(this System.DateTime dateTime1, System.DateTime dateTime2)
	{
		return dateTime1.ToUniversalTime() > dateTime2.ToUniversalTime();
	}

	public static bool IsPastOf(this System.DateTime dateTime1, System.DateTime dateTime2)
	{
		return dateTime1.ToUniversalTime() < dateTime2.ToUniversalTime();
	}

	public static bool IsPresentOf(this System.DateTime dateTime1, System.DateTime dateTime2)
	{
		return dateTime1.ToUniversalTime() == dateTime2.ToUniversalTime();
	}

	public static System.DateTime RandomDateTime(this System.DateTime dateTime)
	{
		return DateTime1970.AddSeconds(Random.Range(0f, (float)dateTime.TimeIntervalSince1970()));
	}
}

public static class FloatingEqualsExtension
{
	public static bool AlmostEquals(this double value1, double value2, double precision = 0.00000001)
	{
		return (System.Math.Abs(value1 - value2) <= precision);
	}
}

public static class StringExtension
{
	public static bool IsNullOrEmpty(this string str)
	{
		return System.String.IsNullOrEmpty(str);
	}

	public static bool IsNullOrWhiteSpace(this string str)
	{
		return System.String.IsNullOrEmpty(str) || str.Trim().Length == 0;
	}
}

