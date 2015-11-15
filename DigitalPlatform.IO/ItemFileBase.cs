using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace DigitalPlatform.IO
{

	// ��������
	public class Item : IComparable
	{
//		public int DataLength = 0;

		public virtual int Length
		{
			get 
			{
				return 0;
			}
			set 
			{
			}
		}

		public virtual void ReadData(Stream stream)
		{
			// ����Length��bytes������

		}


		public virtual void ReadCompareData(Stream stream)
		{
			// ����Length��bytes������

		}

		public virtual void WriteData(Stream stream)
		{
			// д��Length��bytes������

		}

        public virtual void BuildBuffer()
        {
            // ����m_buffer��׼��Lengthֵ

        }

		// ʵ��IComparable�ӿڵ�CompareTo()����,
		// ����ID�Ƚ���������Ĵ�С���Ա�����
		// ���Ҷ��뷽ʽ�Ƚ�
		// obj: An object to compare with this instance
		// ����ֵ A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
		// Less than zero: This instance is less than obj.
		// Zero: This instance is equal to obj.
		// Greater than zero: This instance is greater than obj.
		// �쳣: ArgumentException,obj is not the same type as this instance.
		public virtual int CompareTo(object obj)
		{
			Item item = (Item)obj;

			return (this.Length - item.Length);	// �Ƚ�˭���ݸ���
		}


	}


	// ö����
	public class ItemFileBaseEnumerator : IEnumerator
	{
		ItemFileBase m_file = null;
		long m_index = -1;

		public ItemFileBaseEnumerator(ItemFileBase file)
		{
			m_file = file;
		}

		public void Reset()
		{
			m_index = -1;
		}

		public bool MoveNext()
		{
			m_index ++;
			if (m_index >= m_file.Count)
				return false;

			return true;
		}

		public object Current
		{
			get
			{
				return (object)m_file[m_index];
			}
		}
	}


	// �Ľ����
    [Flags]
    public enum CompressStyle 
	{
		Index = 0x01,	// �Ľ������ļ�
		Data = 0x02,	// �Ľ������ļ�
	}

	public delegate Item Delegate_newItem();

	/// <summary>
	/// �ļ��е�����ϡ�ʵ��������������ɴ�ȡ�Ĵ��ļ����ܡ����ڴ�����С��
	/// </summary>
	public class ItemFileBase : IEnumerable, IDisposable
	{
        public bool ReadOnly = false;

		public ReaderWriterLock m_lock = new ReaderWriterLock();
		public static int m_nLockTimeout = 5000;	// 5000=5��


		bool disposed = false;
		// ���ļ�������
		public string m_strBigFileName = "";
		public Stream m_streamBig = null;

		// С�ļ���С��
		public string m_strSmallFileName = "";
		public Stream m_streamSmall = null;

		public long m_count = 0;

		bool bDirty = false; //��ʼֵfalse,��ʾ�ɾ�

		public Delegate_newItem procNewItem = null;
		/*
		 * ʹ��˵��
		���procNewItem�Ѿ��ҽ���delegate���򼯺�������ʵ�ʱ��ʹ������������Item����������Ķ���
		��������ĺô����ǲ�������������ItemFileBase
		���procNewItemΪ�գ���ʹ��this.NewItem()��������������ȱʡʵ�֣����Ƿ���Item���Ͷ���
		���������ȱ�㣬��Ҫ����������ItemFileBase
		����������ò�Ҫ���ã���Ϊ���׵��������ҡ�������ã�procNewItem���ȡ�
		*
		*/


		public ItemFileBase()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method 
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~ItemFileBase()      
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}


		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!this.disposed)
			{
				// If disposing equals true, dispose all managed 
				// and unmanaged resources.
				if(disposing)
				{
					// Dispose managed resources.

                    // ������һ�����⣺������������������Close()
					// this.Close();
				}

                this.Close();   // 2007/6/8 �ƶ��������

             
				/*
				// Call the appropriate methods to clean up 
				// unmanaged resources here.
				// If disposing is false, 
				// only the following code is executed.
				CloseHandle(handle);
				handle = IntPtr.Zero;            
				*/
			}
			disposed = true;         
		}

		// ������item����
		// ���������item�ಢ��ϣ�����ڱ������й��������������ಢ���ر�������
		// Ҳ��ʹ��procNewItem�ӿڣ������Ͳ�������������
		// �μ�procNewItem���崦˵��
		public virtual Item NewItem()
		{
			return new Item();
		}

		// ����������
		public Item this[Int64 nIndex]
		{
			get
			{
				return GetItem(nIndex, false);
			}
		}

		// ��¼��
		public Int64 Count
		{
			get
			{
				// �Ӷ���
				this.m_lock.AcquireReaderLock(m_nLockTimeout);
				try 
				{
					return m_count;
				}
				finally
				{
					this.m_lock.ReleaseReaderLock();
				}

			}
		}

		// ������ݣ�����Ȼ�ڿ���״̬
		public void Clear()
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				if (m_streamBig != null)
					m_streamBig.SetLength(0);

				if (m_streamSmall != null)
					m_streamSmall.SetLength(0);

				m_count=0;
				bDirty = false;
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}

		}

        // ��
        // bInitialIndex	��ʼ��index�ļ������򿪡�
        public void Open(bool bInitialIndex)
        {
            // ��д��
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                if (m_streamBig == null)
                {
                    if (m_strBigFileName == "")
                        m_strBigFileName = this.GetTempFileName();	// ������ʱ�ļ�������delegate����

                    m_streamBig = File.Open(m_strBigFileName,
                        FileMode.OpenOrCreate);


                    /*
                    m_streamBig = File.Open(m_strBigFileName,
                        FileMode.OpenOrCreate, 
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite);  // 2007/12/26
                     * */
                }

                if (bInitialIndex == true)
                {
                    string strSaveSmallFileName = "";
                    if (this.m_strSmallFileName != "")
                        strSaveSmallFileName = m_strSmallFileName;

                    RemoveIndexFile();

                    if (strSaveSmallFileName != "")
                        m_strSmallFileName = strSaveSmallFileName;

                    OpenIndexFile();
                }

            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

		// ��������һ�����϶���
		public void Copy(ItemFileBase file)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				this.Clear();

				foreach(Item item in file)
				{
					this.Add(item);
				}
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		// ѹ������Щ�ѱ��ɾ������ռ�ݵĴ��̿ռ�
		// ����index�ļ����������û�а������ɾ�����������ʹ�ó˷��������ٶȿ졣
		// һ��Ҫ������ǰ��Compressһ�£����������ٶȺ�����
		public void CompressDeletedItem(CompressStyle style)
		{
			if ((style & CompressStyle.Data) == CompressStyle.Data)
			{
				// ɾ����־��index�ļ���

				throw(new Exception("��ʱ��֧�ִ˹���"));
				/*
				// �����index
				RemoveIndexFile();
				OpenIndexFile();
				*/
			}

			if ((style & CompressStyle.Index) == CompressStyle.Index)
			{
				if (m_streamSmall == null)
					return;
				CompressIndex(m_streamSmall);
				bDirty = false;
			}

		}

		// ��β��׷��һ������
		public virtual void Add(Item item)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				// �������ļ�ָ������β��
				m_streamBig.Seek(0,
					SeekOrigin.End);

				// �����������ļ�
				if (m_streamSmall != null)
				{
					// д��һ����index��Ŀ

					m_streamSmall.Seek (0,SeekOrigin.End);
					long nPosition = m_streamBig.Position ;

					byte[] bufferPosition = new byte[8];
					bufferPosition = System.BitConverter.GetBytes((long)nPosition); // ԭ��ȱ��(long), ��һ��bug. 2006/10/1 �޸�
                    Debug.Assert(bufferPosition.Length == 8, "");
                    m_streamSmall.Write(bufferPosition, 0, 8);
				}

                // 2007/7/3
                item.BuildBuffer();

				byte[] bufferLength = System.BitConverter.GetBytes((Int32)item.Length);

                Debug.Assert(bufferLength.Length == 4, "");
				m_streamBig.Write(bufferLength,0,4);

				item.WriteData(m_streamBig);

				m_count++;
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// ���ɾ��һ����¼
		public void RemoveAt(int nIndex)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				if (nIndex < 0 || nIndex >= m_count)
				{
					throw(new Exception("�±� " + Convert.ToString(nIndex) + " Խ��(Count=" + Convert.ToString(m_count) + ")"));
				}
				int nRet = RemoveAtAuto(nIndex);
				if (nRet == -1)
				{
					throw(new Exception ("RemoveAtAuto fail"));
				}

				m_count --;
				// bDirty = true;	// ��ʾ�Ѿ��б��ɾ����������

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		//���ɾ��������¼
		public void RemoveAt(int nIndex,
			int nCount)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				if (nIndex < 0 || nIndex + nCount > m_count)
				{
					throw(new Exception("�±� " + Convert.ToString(nIndex) + " Խ��(Count=" + Convert.ToString(m_count) + ")"));
				}

				int nRet = 0;
				if (m_streamSmall != null) // �������ļ�ʱ
				{
					// nRet = RemoveAtIndex(nIndex);
					nRet = CompressRemoveAtIndex(nIndex, nCount);
				}
				else 
				{
					throw(new Exception ("��ʱ��û�б�д"));

				}


				if (nRet == -1)
				{
					throw(new Exception ("RemoveAtAuto fail"));
				}

				m_count -= nCount;
				// bDirty = true;	// ��ʾ�Ѿ��б��ɾ����������

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// ����һ������
		public virtual void Insert(int nIndex,
			Item item)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
                if (m_streamSmall == null
                    && m_streamBig == null)
                    throw (new Exception("�ڲ��ļ���δ�򿪻����Ѿ����ر�"));

				// �������������ļ�
				if (m_streamSmall == null)
					throw(new Exception("�ݲ�֧���������ļ���ʽ�µĲ������"));


				// �������ļ�ָ������β��
				m_streamBig.Seek(0,
					SeekOrigin.End);

				// �����������ļ�
				if (m_streamSmall != null)
				{
					// ����һ����index��Ŀ
					long lStart = (long)nIndex * 8;
					StreamUtil.Move(m_streamSmall,
						lStart, 
						m_streamSmall.Length - lStart,
						lStart + 8);

					m_streamSmall.Seek (lStart,SeekOrigin.Begin);
					long nPosition = m_streamBig.Position;

					byte[] bufferPosition = new byte[8];
                    bufferPosition = System.BitConverter.GetBytes((long)nPosition); // ԭ��ȱ��(long), ��һ��bug. 2006/10/1 �޸�
                    Debug.Assert(bufferPosition.Length == 8, "");
					m_streamSmall.Write (bufferPosition,0,8);
				}

				byte[] bufferLength = System.BitConverter.GetBytes((Int32)item.Length);
                Debug.Assert(bufferLength.Length == 4, "");
				m_streamBig.Write(bufferLength,0,4);

				item.WriteData(m_streamBig);

				m_count++;

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		// �رա�ɾ�������ļ���
		public void Close()
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

			RemoveDataFile();

			RemoveIndexFile();

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// ���ⲿ�ļ��󶨵�������
		public void Attach(string strFileName,
			string strIndexFileName)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				RemoveIndexFile();
				RemoveDataFile();	// �����ǰ����ʹ�õ��ڲ������ļ�

				m_strBigFileName = strFileName;
				m_streamBig = File.Open (m_strBigFileName,
					FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);   // 2007/12/26


				bool bCountSeted = false;
				if (strIndexFileName != null)
				{
					if (File.Exists(strIndexFileName) == true)
					{
						this.m_strSmallFileName = strIndexFileName;
						this.OpenIndexFile();
					}
					else
					{
						this.CreateIndexFile(strIndexFileName);
					}

					m_count = GetCountFromIndexFile();
					bCountSeted = true;
				}

				if (bCountSeted == false)
					m_count = GetCountFromDataFile();

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		// �������ļ���������
		void Resequence(string strOutputFileName)
		{
			this.m_streamSmall.Seek(0, SeekOrigin.Begin);

			for(;;)
			{

			}

			// return;
		}


		// �������ļ��Ͷ����ѹ�
		// parameters:
		//		bResequence	�Ƿ���������?
		// return:
		//	�����ļ���
		public void Detach(out string strDataFileName,
			out string strIndexFileName)
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				strDataFileName = m_strBigFileName;
				CloseDataFile();

				m_strBigFileName = "";	// ������������ȥɾ��

				strIndexFileName = this.m_strSmallFileName;
				CloseIndexFile();

				this.m_strSmallFileName = "";	// ������������ȥɾ��

				return;
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// ����
		public void Sort()
		{
			// ��д��
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				if (this.m_strSmallFileName != "")
					CreateIndexFile(this.m_strSmallFileName);
				else 
					CreateIndexFile(null);
				QuickSort();
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

        // ȷ������������
        public void EnsureCreateIndex()
        {
            if (this.m_streamSmall == null)
            {
                this.CreateIndexFile(null);
            }
            else
            {
                // Debugʱ����У��һ��index�ļ��ߴ��Count�Ĺ�ϵ�Ƿ���ȷ
            }
        }

		// ����Index
		// parameters:
		//	strIndexFileName	���==null����ʾ�Զ�������ʱ�ļ�
		public void CreateIndexFile(string strIndexFileName)
		{
			int nLength;

			RemoveIndexFile();

			if (strIndexFileName != null)
				this.m_strSmallFileName = strIndexFileName;

			OpenIndexFile();

			m_streamBig.Seek(0, SeekOrigin.Begin);
			m_streamSmall.Seek(0, SeekOrigin.End);

			int i=0;
			long lPosition = 0;
			for(i=0;;i++)
			{
				//�����ֽ�����
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n<4)   //��ʾ�ļ���β
					break;

				nLength = System.BitConverter.ToInt32(bufferLength,0);
				if (nLength<0)  //ɾ����
				{
					nLength = (int)GetRealValue(nLength);
					goto CONTINUE;
				}

				byte[] bufferOffset = new byte[8];
				bufferOffset = System.BitConverter.GetBytes((long)lPosition);
                Debug.Assert(bufferOffset.Length == 8, "");
				m_streamSmall.Write (bufferOffset,0,8);

			CONTINUE:

				m_streamBig.Seek (nLength,SeekOrigin.Current);
				lPosition += (4+nLength);
			}
		}


		// ��������
		// �޸ģ���������delegate����̽���Ƿ���Ҫ�ж�ѭ��
		// һ���û���Ҫ�����������������Sort()����
		// �����ʾ�������? ͷ�۵����顣�ɷ��ö�ջ��ȱ�ʾ����?
		// ��Ҫ�����ȫ����Ĳ����У�item������������Щ���ִ���item
		// ������ȥ�������ǽ���ָʾ�����ݡ�
		// return:
		//  0 succeed
		//  1 interrupted
		public int QuickSort()
		{
            if (this.m_streamSmall == null)
            {
                throw new Exception("����QuickSortǰ����Ҫ�ȴ�������");
            }

			ArrayList stack = new ArrayList (); // ��ջ
			int   nStackTop = 0;
			long   nMaxRow = m_streamSmall.Length /8;  //m_count;
			long   k = 0;
			long j = 0;
			long i = 0;

			if (nMaxRow == 0)
				return 0;

			/*
			if (nMaxRow >= 10) // ����
			 nMaxRow = 10;
			*/

			Push(stack, 0, nMaxRow - 1, ref nStackTop);
			while(nStackTop>0) 
			{
				Pop(stack, ref k, ref j, ref nStackTop);
				while(k<j) 
				{
					Split(k,j,ref i);
					Push(stack, i+1, j, ref nStackTop);
					j = i - 1;
				}
			}

			return 0;
		}

		#region ��������

		// ɾ��Index�ļ�
		private void RemoveIndexFile()
		{
			// �����������ڣ��رգ��ÿ�
			if (m_streamSmall != null)
			{
				m_streamSmall.Close();
				m_streamSmall = null;
			}

            if (this.ReadOnly == false)
            {
                // ����ļ������ڣ�ɾ���ļ����ñ���ֵΪ��
                if (m_strSmallFileName != "" && m_strSmallFileName != null)
                {
                    File.Delete(m_strSmallFileName);
                    m_strSmallFileName = "";
                }
            }

			bDirty = false;
		}

		// ɾ��data�ļ�
		private void RemoveDataFile()
		{
			// �����������ڣ��رգ��ÿ�
			if (m_streamBig != null)
			{
				m_streamBig.Close();
				m_streamBig = null;
			}

            if (this.ReadOnly == false)
            {
                // ����ļ������ڣ�ɾ���ļ����ñ���ֵΪ��
                if (m_strBigFileName != "" && m_strBigFileName != null)
                {
                    File.Delete(m_strBigFileName);
                    m_strBigFileName = "";
                }
            }
		}

		public void CloseDataFile()
		{
			// �����������ڣ��رգ��ÿ�
			if (m_streamBig != null)
			{
				m_streamBig.Close();
				m_streamBig = null;
			}
		}


		// �Ƿ���д򿪵������ļ�
		public bool HasIndexed
		{
			get 
			{
				if (m_streamSmall == null)
					return false;
				return true;
			}
		}

        public virtual string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

		// ��Index�ļ�
		private void OpenIndexFile()
		{
			if (m_streamSmall == null)
			{
				if (m_strSmallFileName == "")
					m_strSmallFileName = this.GetTempFileName();
				// ����index�ļ���ʱ�򣬿��Ը�����ɫ��ʾ���Ա�ȡ����ɫ�������ļ���

				m_streamSmall = File.Open(m_strSmallFileName,
					FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);  // 2007/12/26

			}

			bDirty = false;
			// m_streamSmall�ķǿվͱ���������Open״̬
		}


		public void CloseIndexFile()
		{
			// �����������ڣ��رգ��ÿ�
			if (m_streamSmall != null)
			{
				m_streamSmall.Close();
				m_streamSmall = null;
			}
		}


		private Int64 GetCountFromDataFile()
		{
			Debug.Assert(m_streamBig != null, "data�������ȴ�");

			m_streamBig.Seek(0, SeekOrigin.Begin);

			Int64 i=0;
			Int32 nLength;

			while(true)
			{
				//�����ֽ�����
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n < 4)   //��ʾ�ļ���β
					break;

				nLength = System.BitConverter.ToInt32(bufferLength,0);
				if (nLength<0)  //ɾ����
				{
					nLength = (int)GetRealValue(nLength);
					goto CONTINUE;
				}
				i++;

			CONTINUE:
				m_streamBig.Seek(nLength, SeekOrigin.Current);
			}
			return i;
		}

		// �������ļ��õ�Ԫ�ظ���
		// Ҫ��bDirty == false
		private Int64 GetCountFromIndexFile()
		{
			Debug.Assert(m_streamSmall != null, "index�������ȴ�");

			return m_streamSmall.Length / 8;

			/* �ʵ�ʱ�����ӱ��������������, �ų���Щ����ƫ��ֵ
			m_streamBig.Seek(0, SeekOrigin.Begin);

			Int64 i=0;
			Int32 nLength;

			while(true)
			{
				//�����ֽ�����
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n < 4)   //��ʾ�ļ���β
					break;

				nLength = System.BitConverter.ToInt32(bufferLength,0);
				if (nLength<0)  //ɾ����
				{
					nLength = (int)GetRealValue(nLength);
					goto CONTINUE;
				}
				i++;

			CONTINUE:
				m_streamBig.Seek(nLength, SeekOrigin.Current);
			}
			return i;
			*/
		}

		// �õ�������λ�û򳤶�
		Int64 GetRealValue(Int64 lPositionOrLength)
		{
			if (lPositionOrLength<0)
			{
				lPositionOrLength = -lPositionOrLength;
				lPositionOrLength--;
			}
			return lPositionOrLength;
		}

		// �õ�ɾ�����ʹ�õ�λ�û򳤶�
		// ������ʾ��ɾ��������
		Int64 GetDeletedValue(Int64 lPositionOrLength)
		{
			if (lPositionOrLength >= 0)
			{
				lPositionOrLength ++;
				lPositionOrLength = -lPositionOrLength;
			}

			return 	lPositionOrLength;
		}


		// bContainDeleted	�Ƿ������ɾ��������?
		public Item GetItem(Int64 nIndex,
			bool bContainDeleted)
		{
			Item item = null;
			long lBigOffset;

			//�Զ����ش��ļ��ı�����,С�ļ�����ʱ����С�ļ��õ���������ʱ���Ӵ��ļ��õ�
			//bContainDeleted����false��������ɾ���ļ�¼��Ϊtrue,������
			//����ֵ
			//>=0:����
			//-1:��bContainDeletedΪfalseʱ:��ʾ����������trueʱ��ʾ�����ĸ�ֵ
			lBigOffset = GetDataOffsetAuto(nIndex, bContainDeleted);

			//��bContainDeletedΪfalseʱ����������ɾ����¼ʱ������ֵ-1����ʾû�ҵ�
			if (bContainDeleted == false)
			{
				if (lBigOffset == -1)
					return null;
			}
			item = GetItemByOffset(lBigOffset);

			return item;
		}

		// bContainDeleted	�Ƿ������ɾ��������?
		public Item GetCompareItem(Int64 nIndex,
			bool bContainDeleted)
		{
			Item item = null;
			long lBigOffset;

			//�Զ����ش��ļ��ı�����,С�ļ�����ʱ����С�ļ��õ���������ʱ���Ӵ��ļ��õ�
			//bContainDeleted����false��������ɾ���ļ�¼��Ϊtrue,������
			//����ֵ
			//>=0:����
			//-1:��bContainDeletedΪfalseʱ:��ʾ����������trueʱ��ʾ�����ĸ�ֵ
			lBigOffset = GetDataOffsetAuto(nIndex, bContainDeleted);

			//��bContainDeletedΪfalseʱ����������ɾ����¼ʱ������ֵ-1����ʾû�ҵ�
			if (bContainDeleted == false)
			{
				if (lBigOffset == -1)
					return null;
			}
			item = GetCompareItemByOffset(lBigOffset);

			return item;
		}


		// �Զ����������������ļ��ı���������ν�Զ�����˼��, С�ļ�����ʱ����С�ļ���õ�(��)��������ʱ���Ӵ��ļ�˳��õ�(��)
		// parameters:
		//		bContainDeleted����false��������ɾ���ļ�¼��Ϊtrue,������
		// ����ֵ
		//		>=0	����
		//		-1	��bContainDeletedΪfalseʱ:��ʾ����������trueʱ��ʾ�����ĸ�ֵ
		long GetDataOffsetAuto(long nIndex,bool bContainDeleted)
		{
			if (m_streamSmall != null)
			{
				return GetDataOffsetFromIndexFile(nIndex, bContainDeleted);
			}
			else
			{
				return GetDataOffsetFromDataFile(nIndex, bContainDeleted);
			}
		}

		// �������ļ���ֱ��������������ƫ������
		//	��Ȼ�������ٶȺ���
		// return:
		//		-1	��bContainDeletedΪfalseʱ-1��ʾ����������bContainDeletedΪtrueʱ��ʾ�����ĸ�ֵ
		long GetDataOffsetFromDataFile(long nIndex,bool bContainDeleted)
		{
			m_streamBig.Seek(0, SeekOrigin.Begin);
			long lBigOffset = 0;

			int nLength;
			int i = 0;
			while(true)
			{
				//��4���ֽڣ��õ�����
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n<4)   //��ʾ�ļ���β
					break;
				nLength = System.BitConverter.ToInt32(bufferLength,0);

				if (bContainDeleted == false)
				{
					if (nLength<0)
					{
						//ת��Ϊʵ�ʵĳ��ȣ���seek
						long lTemp = GetRealValue(nLength);
						m_streamBig.Seek (lTemp,SeekOrigin.Current);

						lBigOffset += (4+lTemp);
						continue;
					}
				}

				if (i == nIndex)
				{
					return  lBigOffset;
				}
				else
				{
					m_streamBig.Seek (nLength,SeekOrigin.Current);
				}

				lBigOffset += (4+nLength);

				i++;
			}

			return -1;
		}

		// �������ļ��м��㷵�������������ļ��е�ƫ����
		// return:
		//		-1	��bContainDeletedΪfalseʱ-1��ʾ����������bContainDeletedΪtrueʱ��ʾ�����ĸ�ֵ
		long GetDataOffsetFromIndexFile(
			long nIndex,
			bool bContainDeleted)
		{
			if (m_streamSmall == null)
			{
				Debug.Assert(true, "�����ļ���Ϊnull, GetDataOffsetFromIndexFile()�����޷�ִ��");
				throw(new Exception ("�����ļ���Ϊnull, GetDataOffsetFromIndexFile()�����޷�ִ��"));
			}

			// �ɾ���Ҳ����������ȷ����ɾ������ڵ�����£������ó˷�����õ�λ�ã��ٶȿ�
			if (bDirty == false)
			{
				if (nIndex*8>=m_streamSmall.Length || nIndex<0)
				{
					throw(new Exception("���� " + Convert.ToString(nIndex) + "�����㳬�������ļ���ǰ���Χ"));
				}
				return GetIndexItemValue(nIndex);
			}
			else
			{
				long lBigOffset = 0;

				m_streamSmall.Seek(0, SeekOrigin.Begin);
				int i = 0;
				while(true)
				{
					//��8���ֽڣ��õ�λ��
					byte[] bufferBigOffset = new byte[8];
					int n = m_streamSmall.Read(bufferBigOffset,0,8);
					if (n < 8)   //��ʾ�ļ���β
						break;
					lBigOffset = System.BitConverter.ToInt32(bufferBigOffset, 0);
					
					if (bContainDeleted == false)
					{
						//Ϊ����ʱ����
						if (lBigOffset<0)
						{
							continue;
						}
					}
					//��ʾ������ҵ�
					if (i == nIndex)
					{
						return lBigOffset;
					}
					i++;
				}
			}
			return -1;
		}

		// ��*8�ķ��������������������ļ���λ�ã�ȡ����ֵ
		// ��������ɾ���ļ�¼����ȡ��������������������������ļ��ı�����
		long GetIndexItemValue(long nIndex)
		{
			if( m_streamSmall == null)
			{
				throw(new Exception("m_streamSmall����Ϊ��, GetIndexItemValue()�޷�����"));
			}

			if (nIndex*8 >= m_streamSmall.Length || nIndex<0)
			{
				throw(new Exception("���� " + Convert.ToString(nIndex) + "�����㳬�������ļ���ǰ���Χ"));
			}

			m_streamSmall.Seek(nIndex*8, 
				SeekOrigin.Begin);

			byte[] bufferOffset = new byte[8];
			int n = m_streamSmall.Read(bufferOffset, 0, 8);
			if (n <= 0)
			{
				throw(new Exception("GetIndexItemValue()�쳣��ʵ�����ĳ���"+Convert.ToString (m_streamSmall.Length )+"\r\n"
					+"ϣ��Seek����λ��"+Convert.ToString (nIndex*8)+"\r\n"
					+"ʵ�ʶ��ĳ���"+Convert.ToString (n)));
			}
			long lOffset = System.BitConverter.ToInt64(bufferOffset,0);

			return lOffset;
		}

		//	lOffset�����������������ҵ���¼������ʱ��ע�⣬�������Ҫ�õ���ɾ���ļ�¼���������ж�
		Item GetItemByOffset(long lOffset)
		{
			Item item = null;

			if (lOffset <0)
			{
				lOffset = GetRealValue(lOffset);
			}

			if (lOffset >= m_streamBig.Length )
			{
				throw(new Exception ("�ڲ�����λ�ô����ܳ���"));
				//return null;
			}

			m_streamBig.Seek(lOffset, SeekOrigin.Begin);

			//�����ֽ�����
			byte[] bufferLength = new byte[4];
			int n = m_streamBig.Read(bufferLength,0,4);
			if (n<4)   //��ʾ�ļ���β
			{
				throw(new Exception ("�ڲ�����:Read error"));
				//return null;
			}

			// ���procNewItem�Ѿ��ҽ���delegate����ʹ������������Item����������Ķ���
			// ��������ĺô����ǲ�������������ItemFileBase
			if (this.procNewItem != null)
				item = procNewItem();
			else	// procNewItemΪ�գ���ʹ��this.NewItem()��������������ȱʡʵ�֣����Ƿ���Item���Ͷ���
				item = this.NewItem(); // ���������ȱ�㣬��Ҫ����������ItemFileBase

			item.Length = System.BitConverter.ToInt32(bufferLength, 0);
			item.ReadData(m_streamBig);

			return item;
		}


		//	lOffset�����������������ҵ���¼������ʱ��ע�⣬�������Ҫ�õ���ɾ���ļ�¼���������ж�
		public Item GetCompareItemByOffset(long lOffset)
		{
			Item item = null;

			if (lOffset <0)
			{
				lOffset = GetRealValue(lOffset);
			}

			if (lOffset >= m_streamBig.Length )
			{
				throw(new Exception ("�ڲ�����λ�ô����ܳ���"));
				//return null;
			}

			m_streamBig.Seek(lOffset, SeekOrigin.Begin);

			//�����ֽ�����
			byte[] bufferLength = new byte[4];
			int n = m_streamBig.Read(bufferLength,0,4);
			if (n<4)   //��ʾ�ļ���β
			{
				throw(new Exception ("�ڲ�����:Read error"));
				//return null;
			}

			// ���procNewItem�Ѿ��ҽ���delegate����ʹ������������Item����������Ķ���
			// ��������ĺô����ǲ�������������ItemFileBase
			if (this.procNewItem != null)
				item = procNewItem();
			else	// procNewItemΪ�գ���ʹ��this.NewItem()��������������ȱʡʵ�֣����Ƿ���Item���Ͷ���
				item = this.NewItem(); // ���������ȱ�㣬��Ҫ����������ItemFileBase

			item.Length = System.BitConverter.ToInt32(bufferLength, 0);
			item.ReadCompareData(m_streamBig);

			return item;
		}

	
        /*
		// ����ѹ�����ɾ���˵���Щ����
		private static int CompressIndex(Stream oStream)
		{
			if (oStream == null)
			{
				return -1;
			}

			long lDeletedStart = 0;  //ɾ�������ʼλ��
			long lDeletedEnd = 0;    //ɾ����Ľ���λ��
			long lDeletedLength = 0;
			bool bDeleted = false;   //�Ƿ��ѳ���ɾ����

			long lUseablePartLength = 0;    //����������ĳ���
			bool bUserablePart = false;    //�Ƿ��ѳ���������

			bool bEnd = false;
			long lValue = 0;

			oStream.Seek (0,SeekOrigin.Begin );
			while(true)
			{
				int nRet;
				byte[] bufferValue = new byte[8];
				nRet = oStream.Read(bufferValue,0,8);
				if (nRet != 8 && nRet != 0)  
				{
					throw(new Exception ("�ڲ�����:�����ĳ��Ȳ�����8"));
					//break;
				}
				if (nRet == 0)//��ʾ����
				{
					if(bUserablePart == false)
						break;

					lValue = -1;
					bEnd = true;
					//break;
				}

				if (bEnd != true)
				{
					lValue = BitConverter.ToInt64(bufferValue,0);
				}
				if (lValue<0)
				{
					if (bDeleted == true && bUserablePart == true)
					{
						lDeletedEnd = lDeletedStart + lDeletedLength;
						//��MovePart(lDeletedStart,lDeletedEnd,lUseablePartLength)

						StreamUtil.Move(oStream,
							lDeletedEnd,
							lUseablePartLength,
							lDeletedStart);

						//���¶�λdeleted����ʼλ��
						lDeletedStart = lUseablePartLength-lDeletedLength+lDeletedEnd;
						lDeletedEnd = lDeletedStart+lDeletedLength;

						oStream.Seek (lDeletedEnd+lUseablePartLength,SeekOrigin.Begin);

					}

					bDeleted = true;
					bUserablePart = false;
					lDeletedLength += 8;  //����λ�ü�8
				}
				else if (lValue>=0)
				{
					//�����ֹ�ɾ����ʱ���ֽ����µ����ÿ�ʱ��ǰ�������ÿ鲻�ƣ����¼��㳤��
					//|  userable  | ........ |  userable |
					//|  ........  | userable |
					if (bDeleted == true && bUserablePart == false)
					{
						lUseablePartLength = 0;
					}

					bUserablePart = true;
					lUseablePartLength += 8;
					
					if (bDeleted == false)
					{
						lDeletedStart += 8;  //��������ɾ����ʱ��ɾ����ʼλ�ü�8
					}
				}

				if (bEnd == true)
				{
					break;
				}
			}

			//ֻʣβ���ı�ɾ����¼
			if (bDeleted == true && bUserablePart == false)
			{
				//lDeletedEnd = lDeletedStart + lDeletedLength;
				oStream.SetLength(lDeletedStart);
			}

			// bDirty = false;
			return 0;
		}
        */


        // ����ѹ�����ɾ���˵���Щ����
        // ѹ������
        private static int CompressIndex(Stream oStream)
        {
            if (oStream == null)
            {
                return -1;
            }

            int nRet;
            long lRestLength = 0;
            long lDeleted = 0;
            long lCount = 0;

            oStream.Seek(0, SeekOrigin.Begin);
            lCount = oStream.Length / 8;
            for (long i = 0; i < lCount; i++)
            {
                byte[] bufferValue = new byte[8];
                nRet = oStream.Read(bufferValue, 0, 8);
                if (nRet != 8 && nRet != 0)
                {
                    throw (new Exception("�ڲ�����:�����ĳ��Ȳ�����8"));
                }

                long lValue = BitConverter.ToInt64(bufferValue, 0);

                if (nRet == 0)//��ʾ����
                {
                    break;
                }

                if (lValue < 0)
                {
                    // ��ʾ��Ҫɾ������Ŀ
                    lRestLength = oStream.Length - oStream.Position;

                    Debug.Assert(oStream.Position - 8 >= 0, "");


                    long lSavePosition = oStream.Position;

                    StreamUtil.Move(oStream,
                        oStream.Position,
                        lRestLength,
                        oStream.Position - 8);

                    oStream.Seek(lSavePosition - 8, SeekOrigin.Begin);

                    lDeleted++;
                }
            }

            if (lDeleted > 0)
            {
                oStream.SetLength((lCount - lDeleted) * 8);
            }

            return 0;
        }


		#endregion

	

		#region �����йصĻ�������

		void Push(ArrayList stack,
			long lStart,
			long lEnd,
			ref int nStackTop)
		{
			if (nStackTop < 0)
			{
				throw(new Exception ("nStackTop����С��0"));
			}
			if (lStart < 0)
			{
				throw(new Exception ("nStart����С��0"));
			}

			if (nStackTop*2 != stack.Count )
			{
				throw(new Exception ("nStackTop*2������stack.m_count"));
			}


			stack.Add (lStart);
			stack.Add (lEnd);

			nStackTop ++;
		}


		void Pop(ArrayList stack,
			ref long lStart,
			ref long lEnd,
			ref int nStackTop)
		{
			if (nStackTop <= 0)
			{
				throw(new Exception ("pop��ǰ,nStackTop����С�ڵ���0"));
			}

			if (nStackTop*2 != stack.Count )
			{
				throw(new Exception ("nStackTop*2������stack.m_count"));
			}

			lStart = (long)stack[(nStackTop-1) * 2];
			lEnd = (long)stack[(nStackTop-1) * 2+1];

			stack.RemoveRange((nStackTop-1) * 2,2);

			nStackTop --;
		}


		void Split(long nStart,
			long nEnd,
			ref long nSplitPos)
		{
			// ȡ������
			long pStart = 0;
			long pEnd = 0;
			long pMiddle = 0;
			long pSplit = 0;
			long nMiddle;
			long m,n,i,j,k;
			long T = 0;
			int nRet;
			long nSplit;


			nMiddle = (nStart + nEnd) / 2;

			pStart = GetIndexItemValue(nStart);  
			pEnd = GetIndexItemValue(nEnd);   

			// �������յ��Ƿ��������
			if (nStart + 1 == nEnd) 
			{
				nRet = Compare(pStart, pEnd);
				if (nRet > 0) 
				{ // ����
					T = pStart;
					SetRowPtr(nStart, pEnd);
					SetRowPtr(nEnd, T);
				}
				nSplitPos = nStart;
				return;
			}


			pMiddle = GetIndexItemValue(nMiddle);   //GetRowPtr(nMiddle);

			nRet = Compare(pStart, pEnd);
			if (nRet <= 0) 
			{
				nRet = Compare(pStart, pMiddle);
				if (nRet <= 0) 
				{
					pSplit = pMiddle;
					nSplit = nMiddle;
				}
				else 
				{
					pSplit = pStart;
					nSplit = nStart;
				}
			}
			else 
			{
				nRet = Compare(pEnd, pMiddle);
				if (nRet <= 0) 
				{
					pSplit = pMiddle;
					nSplit = nMiddle;
				}
				else 
				{
					pSplit = pEnd;
					nSplit = nEnd;
				}

			}

			// 
			k = nSplit;
			m = nStart;
			n = nEnd;

			T = GetIndexItemValue(k);
			// (m)-->(k)
			SetRowPtr(k, GetIndexItemValue(m));
			i = m;
			j = n;
			while(i!=j) 
			{
				while(true) 
				{
					nRet = Compare(GetIndexItemValue(j), T);
					if (nRet >= 0 && i<j)
						j = j - 1;
					else 
						break;
				}
				if (i<j) 
				{
					// (j)-->(i)
					SetRowPtr(i, GetIndexItemValue(j) /*GetRowPtr(j)*/);
					i = i + 1;
					while(true) 
					{
						nRet = Compare(/*GetRowPtr(i)*/ GetIndexItemValue(i), T);
						if (nRet <=0 && i<j)
							i = i + 1;
						else 
							break;
					}
					if (i<j) 
					{
						// (i)--(j)
						SetRowPtr(j, GetIndexItemValue(i) /*GetRowPtr(i)*/);
						j = j - 1;
					}
				}
			}
			SetRowPtr(i, T);
			nSplitPos = i;
		}


		public void SetRowPtr(long nIndex, long lPtr)
		{
			byte[] bufferOffset ;

			//�õ�ֵ
			bufferOffset = new byte[8];
			bufferOffset = BitConverter.GetBytes((long)lPtr);
			

			//����ֵ
			m_streamSmall.Seek (nIndex*8,SeekOrigin.Begin);
            Debug.Assert(bufferOffset.Length == 8, "");
			m_streamSmall.Write (bufferOffset,0,8);

		}

		// ���������ڲ�ʹ�á���Ҫ�ı�������Ϊ��������Item��CompareTo()����
		public virtual int Compare(long lPtr1,long lPtr2)
		{
			if (lPtr1<0 && lPtr2<0)
				return 0;
			else if (lPtr1>=0 && lPtr2<0)
				return 1;
			else if (lPtr1<0 && lPtr2>=0)
				return -1;

			Item item1 = GetCompareItemByOffset(lPtr1);
			Item item2 = GetCompareItemByOffset(lPtr2);

			return item1.CompareTo(item2);
		}

		#endregion



		#region ��ɾ���йصĻ�������

		// �Զ�ѡ��Ӻδ�ɾ��
		int RemoveAtAuto(int nIndex)
		{
			int nRet = -1;
			if (m_streamSmall != null) // �������ļ�ʱ
			{
				// nRet = RemoveAtIndex(nIndex);
				nRet = CompressRemoveAtIndex(nIndex, 1);
			}
			else  // �����ļ�������ʱ�� �������ļ���ɾ��
			{
				nRet = RemoveAtData(nIndex);
			}
			return nRet;
		}

		// ���������ж�λ����
		public long LocateIndexItem(int nIndex)
		{
			long lPositionS = 0;
			if (bDirty == false)
			{
				lPositionS = nIndex*8;
				if (lPositionS>=m_streamSmall.Length || nIndex<0)
				{
					throw(new Exception("�±�Խ��..."));
				}

				m_streamSmall.Seek(lPositionS, SeekOrigin.Begin);
				return lPositionS;
			}
			else
			{
				m_streamSmall.Seek (0,SeekOrigin.Begin);
				long lBigOffset;
				int i = 0;
				while(true)
				{
					//��8���ֽڣ��õ�λ��
					byte[] bufferBigOffset = new byte[8];
					int n = m_streamSmall.Read(bufferBigOffset,0,8);
					if (n<8)   //��ʾ�ļ���β
						break;
					lBigOffset = System.BitConverter.ToInt64(bufferBigOffset,0);
					
					//Ϊ����ʱ����
					if (lBigOffset<0)
					{
						goto CONTINUE;
					}

					//��ʾ������ҵ�
					if (i == nIndex)
					{
						m_streamSmall.Seek (lPositionS,SeekOrigin.Begin );
						return lPositionS;
					}
					i++;

				CONTINUE:
					lPositionS += 8;
				}
			}
			return -1;
		}

		// �������ļ��б��ɾ��һ������
		public int MaskRemoveAtIndex(int nIndex)
		{
			int nRet;

			// lBigOffset��ʾ���ļ��ı�������-1��ʾ����
			long lBigOffset = GetDataOffsetFromIndexFile(nIndex,false);
			if (lBigOffset == -1)
				return -1;

			lBigOffset = GetDeletedValue(lBigOffset);

			byte[] bufferBigOffset = new byte[8];
			bufferBigOffset = BitConverter.GetBytes((long)lBigOffset);

			nRet = (int)LocateIndexItem(nIndex);
			if (nRet == -1)
				return -1;
            Debug.Assert(bufferBigOffset.Length == 8, "");
			m_streamSmall.Write(bufferBigOffset,0,8);

			return 0;
		}


		// �������ļ��м�ѹʽɾ��һ������
		public int CompressRemoveAtIndex(int nIndex,
			int nCount)
		{
			if (m_streamSmall == null)
				throw new Exception("�����ļ���δ��ʼ��");

			long lStart = (long)nIndex * 8;
			StreamUtil.Move(m_streamSmall,
					lStart + 8*nCount, 
					m_streamSmall.Length - lStart - 8*nCount,
					lStart);

			m_streamSmall.SetLength(m_streamSmall.Length - 8*nCount);

			return 0;
		}


		//�Ӵ��ļ���ɾ��
		public int RemoveAtData(int nIndex)
		{
			//�õ����ļ�ƫ����
			long lBigOffset = GetDataOffsetFromDataFile(nIndex, false);
			if (lBigOffset == -1)
				return -1;

			if (lBigOffset >= m_streamBig.Length )
			{
				throw(new Exception ("�ڲ�����λ�ô����ܳ���"));
				//return null;
			}

			m_streamBig.Seek(lBigOffset,SeekOrigin.Begin);
			//�����ֽ�����
			byte[] bufferLength = new byte[4];
			int n = m_streamBig.Read(bufferLength,0,4);
			if (n<4)   //��ʾ�ļ���β
			{
				throw(new Exception ("�ڲ�����:Read error"));
				//return null;
			}

			int nLength = System.BitConverter.ToInt32(bufferLength,0);
			nLength = (int)GetDeletedValue(nLength);

			bufferLength = BitConverter.GetBytes((Int32)nLength);
			m_streamBig.Seek (-4,SeekOrigin.Current);
            Debug.Assert(bufferLength.Length == 4);
			m_streamBig.Write (bufferLength,0,4);

			return 0;
		}


		#endregion

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ItemFileBaseEnumerator(this);
		}

	}
}
