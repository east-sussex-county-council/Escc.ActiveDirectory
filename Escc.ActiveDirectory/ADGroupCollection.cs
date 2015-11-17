using System;
using System.Collections;

namespace Escc.ActiveDirectory
{
	/// <summary>
	/// Summary description for ADGroup.
	/// </summary>
	public class ADGroupCollection: CollectionBase
	{
		#region constructor
		/// <summary>
		/// Class constructor
		/// </summary>
		public ADGroupCollection()
		{
		}
		#endregion
		#region Interface implementations
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(ADGroupMember item)
		{
			return List.Add(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, ADGroupMember item)
		{
			List.Insert(index, item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		public void Remove(ADGroupMember item)
		{
			List.Remove(item);
		} 
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The position into which the new element was inserted.</returns>
		public bool Contains(ADGroupMember item)
		{
			return List.Contains(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The position into which the new element was inserted.</returns>
		public int IndexOf(ADGroupMember item)
		{
			return List.IndexOf(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(ADGroupMember[] array, int index)
		{
			List.CopyTo(array, index);
		}
		#endregion
		#region public properties
		/// <summary>
		/// Interface implementation
		/// </summary>
		public ADGroupMember this[int index]
		{
			get { return (ADGroupMember)List[index]; }
			set { List[index] = value; }
		}
		#endregion
	}
}
