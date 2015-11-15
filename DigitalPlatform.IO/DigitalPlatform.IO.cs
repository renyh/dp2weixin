using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.Text;

using DigitalPlatform;

namespace DigitalPlatform.IO
{
	// ��ʱ�ļ�
	public class TempFileItem 
	{
		public Stream m_stream;
		public string m_strFileName;
	}

	// ��ʱ�ļ�����
	public class TempFileCollection : ArrayList
	{
		public TempFileCollection() 
		{
		}

		~TempFileCollection() 
		{
			Clear();
		}

		public new void Clear() 
		{

			int l;
			for(l=0; l<this.Count; l++) 
			{

				TempFileItem item = (TempFileItem)this[l];
				if (item.m_stream != null) 
				{
					item.m_stream.Close();
					item.m_stream = null;
				}

				try 
				{
					File.Delete(item.m_strFileName);
				}
				catch
				{
				}

			}

			base.Clear();
		}
	}

	public delegate bool FlushOutput();
	public delegate bool ProgressOutput(long lCur);

	// �ڶ��ձ��к겻����
	public class MacroNotFoundException : Exception
	{

		public MacroNotFoundException (string s) : base(s)
		{
		}

	}

	// ������ʽ��
	public class MacroNameException : Exception
	{

		public MacroNameException (string s) : base(s)
		{
		}

	}

}
