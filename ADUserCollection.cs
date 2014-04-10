using System;
using System.Collections;

namespace EsccWebTeam.Data.ActiveDirectory
{
	/// <summary>
	/// Summary description for ADUserCollection.
	/// </summary>
	public class ADUserCollection: CollectionBase
	{
		#region constructor
		/// <summary>
		/// Class constructor
		/// </summary>
		public ADUserCollection()
		{
		}
		#endregion
		#region Interface implementations
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int Add(ADUser item)
		{
			return List.Add(item);
		}
		/// <summary>
		/// Interface implementation
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, ADUser item)
		{
			List.Insert(index, item);
		}
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="item"></param>
		public void Remove(ADUser item)
		{
			List.Remove(item);
		} 
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(ADUser item)
		{
			return List.Contains(item);
		}
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(ADUser item)
		{
			return List.IndexOf(item);
		}
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(ADUser[] array, int index)
		{
			List.CopyTo(array, index);
		}
		#endregion
		#region public properties
		/// <summary>
		/// Interface imoplementation
		/// </summary>
		public ADUser this[int index]
		{
			get { return (ADUser)List[index]; }
			set { List[index] = value; }
		}
		#endregion
	}
}
