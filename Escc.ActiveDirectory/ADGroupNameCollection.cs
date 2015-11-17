using System;
using System.Collections;

namespace Escc.ActiveDirectory
{
	/// <summary>
	/// Summary description for ADGroup.
	/// </summary>
	public class ADGroupNameCollection: CollectionBase
	{
		#region constructor
		/// <summary>
		/// Class constructor
		/// </summary>
		public ADGroupNameCollection()
		{
		}
		#endregion
		#region Interface implementations
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int Add(string item)
		{
			return List.Add(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, string item)
		{
			List.Insert(index, item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public void Remove(string item)
		{
			List.Remove(item);
		} 
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(string item)
		{
			return List.Contains(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(string item)
		{
			return List.IndexOf(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(string[] array, int index)
		{
			List.CopyTo(array, index);
		}
		#endregion
		#region public properties
		/// <summary>
		/// Interface implementation
		/// </summary>
		public string this[int index]
		{
			get { return (string)List[index]; }
			set { List[index] = value; }
		}
		#endregion
	}
}
