using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using YourCommonTools;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * FileSystemController
	 * 
	 * It allows us navigate through the filesystem
	 * 
	 * @author Esteban Gallardo
	 */
	public class FileSystemController : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		public const int ITEM_BACK		= 0;
		public const int ITEM_DRIVE		= 1;
		public const int ITEM_FOLDER	= 2;
		public const int ITEM_FILE		= 3;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------
		private static FileSystemController _instance;

		public static FileSystemController Instance
		{
			get
			{
				_instance = GameObject.FindObjectOfType(typeof(FileSystemController)) as FileSystemController;
				if (!_instance)
				{
					GameObject container = new GameObject();
					container.name = "FileSystemController";
					_instance = container.AddComponent(typeof(FileSystemController)) as FileSystemController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private DirectoryInfo m_pathLastSearch = null;

		public DirectoryInfo PathLastSearch
		{
			get { return m_pathLastSearch; }
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			GameObject.Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * IsFileImage
		 */
		public static bool IsFileImage(string _filename)
		{
			return ((_filename.IndexOf(".png") != -1) || (_filename.IndexOf(".PNG") != -1)
					|| (_filename.IndexOf(".jpg") != -1) || (_filename.IndexOf(".JPG") != -1)
					|| (_filename.IndexOf(".jpeg") != -1) || (_filename.IndexOf(".JPEG") != -1));
		}

		// -------------------------------------------
		/* 
		 * Gets the list of items for the path
		 */
		public List<ItemMultiObjectEntry> GetFileList(DirectoryInfo _directoryInfo, string _searchPattern = "")
		{
			List<ItemMultiObjectEntry> output = new List<ItemMultiObjectEntry>();
			try
			{
				DirectoryInfo directoryInfo = _directoryInfo;
				if (directoryInfo == null)
				{
#if UNITY_EDITOR
					directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
#else
					directoryInfo = new DirectoryInfo(Application.persistentDataPath);					
#endif
				}

				// IF SHOW DRIVES
				if (directoryInfo.Parent == null)
				{
					string[] drives = System.IO.Directory.GetLogicalDrives();
					for (int i = 0; i < drives.Length; i++)
					{
						output.Add(new ItemMultiObjectEntry(ITEM_DRIVE, new DirectoryInfo(drives[i])));
					}
				}

				// DIRECTORIES
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				for (int i = 0; i < directories.Length; i++)
				{
					output.Add(new ItemMultiObjectEntry(ITEM_FOLDER, directories[i]));				
				}

				// FILES
				FileInfo[] files = Utilities.GetFiles(directoryInfo, _searchPattern, SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					output.Add(new ItemMultiObjectEntry(ITEM_FILE, files[i]));
				}
				
				// IF SHOW DRIVES
				if (directoryInfo.Parent != null)
				{
					output.Insert(0, new ItemMultiObjectEntry(ITEM_BACK, directoryInfo.Parent));
				}

				// SET LAST DIRECTORY VISITED
				m_pathLastSearch = directoryInfo;
			}
			catch (Exception err)
			{
				output.Clear();
				output.Add(new ItemMultiObjectEntry(ITEM_BACK, m_pathLastSearch));
			}				

			return output;
		}

	}
}