using System;
using System.Collections;

namespace Escc.ActiveDirectory
{
	/// <summary>
	/// Summary description for ADGroupCollection.
	/// </summary>
	public class ADGroupsCollection: CollectionBase
	{
		#region constructor
		/// <summary>
		/// Class constructor
		/// </summary>
		public ADGroupsCollection()
		{
		}
		#endregion
		#region Interface implementations
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(ADGroupCollection item)
		{
			return List.Add(item);
		}
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, ADGroupCollection item)
		{
			List.Insert(index, item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="item"></param>
		public void Remove(ADGroupCollection item)
		{
			List.Remove(item);
		} 
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The position into which the new element was inserted.</returns>
		public bool Contains(ADGroupCollection item)
		{
			return List.Contains(item);
		}
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(ADGroupCollection item)
		{
			return List.IndexOf(item);
		}
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(ADGroupCollection[] array, int index)
		{
			List.CopyTo(array, index);
		}
		#endregion
		#region public properties
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		public ADGroupCollection this[int index]
		{
			get { return (ADGroupCollection)List[index]; }
			set { List[index] = value; }
		}
		#endregion
	}
}
