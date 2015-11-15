using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Diagnostics;

using System.Windows.Forms;
using System.Drawing;


namespace DigitalPlatform
{

	// byte[] �����ʵ�ú�����
	public class ByteArray
	{
        /*
        // ����һ��byte����
        public static byte[] Dup(byte [] source)
        {
            if (source == null)
                return null;

            byte [] result = null;
            result = EnsureSize(result, source.Length);

            Array.Copy(source, 0, result, 0, source.Length);

            return result;
        }*/

		// ��¡һ���ַ�����
		public static byte[] GetCopy(byte[] baContent)
		{
			if (baContent == null)
				return null;
			byte [] baResult = new byte[baContent.Length];
			Array.Copy(baContent, 0, baResult, 0, baContent.Length);
			return baResult;
		}

		// ��byte[]ת��Ϊ�ַ������Զ�̽����뷽ʽ
		public static string ToString(byte [] baContent)
		{
			ArrayList encodings = new ArrayList();

			encodings.Add(Encoding.UTF8);
			encodings.Add(Encoding.Unicode);

			for(int i=0;i<encodings.Count;i++)
			{
				Encoding encoding = (Encoding)encodings[i];

				byte [] Preamble = encoding.GetPreamble();

				if (baContent.Length < Preamble.Length)
					continue;

				if (ByteArray.Compare(baContent, Preamble, Preamble.Length) == 0)
					return encoding.GetString(baContent,
						Preamble.Length,
						baContent.Length - Preamble.Length);
			}

			// ȱʡ����UTF8
			return Encoding.UTF8.GetString(baContent);
		}

		// byte[] �� �ַ���
		public static string ToString(byte[] bytes,
			Encoding encoding)
		{
			int nIndex = 0;
			int nCount = bytes.Length;
			byte[] baPreamble = encoding.GetPreamble();
			if (baPreamble != null
				&& baPreamble.Length != 0
				&& bytes.Length >= baPreamble.Length)
			{
				byte[] temp = new byte[baPreamble.Length];
				Array.Copy(bytes,
					0,
					temp,
					0,
					temp.Length);

				bool bEqual = true;
				for(int i=0;i<temp.Length;i++)
				{
					if (temp[i] != baPreamble[i])
					{
						bEqual = false;
						break;
					}
				}

				if (bEqual == true)
				{
					nIndex = temp.Length;
					nCount = bytes.Length - temp.Length;
				}
			}

			return encoding.GetString(bytes,
				nIndex,
				nCount);
		}


		// �Ƚ�����byte[]�����Ƿ���ȡ�
		// parameter:
		//		timestamp1: ��һ��byte[]����
		//		timestamp2: �ڶ���byte[]����
		// return:
		//		0   ���
		//		���ڻ���С��0   ���ȡ��ȱȽϳ��ȡ�������ȣ�������ַ������
		public static int Compare(
			byte[] bytes1,
			byte[] bytes2)
		{
			if (bytes1 == null	&& bytes2 == null)
				return 0;
			if (bytes1 == null)
				return -1;
			if (bytes2 == null)
				return 1;

			int nDelta = bytes1.Length - bytes2.Length;
			if (nDelta != 0)
				return nDelta;

			for(int i=0;i<bytes1.Length;i++)
			{
				nDelta = bytes1[i] - bytes2[i];
				if (nDelta != 0)
					return nDelta;
			}

			return 0;
		}

		// �Ƚ�����byte����ľֲ�
		public static int Compare(
			byte[] bytes1,
			byte[] bytes2, 
			int nLength)
		{
			if (bytes1.Length < nLength || bytes2.Length < nLength)
				return Compare(bytes1, bytes2, Math.Min(bytes1.Length, bytes2.Length));

			for(int i=0;i<nLength;i++)
			{
				int nDelta = bytes1[i] - bytes2[i];
				if (nDelta != 0)
					return nDelta;
			}

			return 0;
		}


		public static int IndexOf(byte [] source,
			byte v,
			int nStartPos)
		{
			for(int i=nStartPos;i<source.Length;i++)
			{
				if (source[i] == v)
					return i;
			}
			return -1;
		}
		// ȷ������ߴ��㹻
		public static byte [] EnsureSize(byte [] source,
			int nSize)
		{
			if (source == null) 
			{
				return new byte[nSize];
			}

			if (source.Length < nSize) 
			{
				byte [] temp = new byte [nSize];
				Array.Copy(source, 
					0,
					temp,
					0,
					source.Length);
				return temp;	// �ߴ粻�����Ѿ����·��䣬���Ҽ̳���ԭ������
			}

			return source;	// �ߴ��㹻
		}


		// �ڻ�����β��׷��һ���ֽ�
		public static byte[] Add(byte[] source,
			byte v)
		{
			int nIndex = -1;
			if (source != null) 
			{
				nIndex = source.Length;
				source = EnsureSize(source, source.Length + 1);
			}
			else 
			{
				nIndex = 0;
				source = EnsureSize(source, 1);
			}

			source[nIndex] = v;

			return source;
		}

		// �ڻ�����β��׷�������ֽ�
		public static byte[] Add(byte[] source,
			byte[] v)
		{
			int nIndex = -1;
			if (source != null) 
			{
				nIndex = source.Length;
				source = EnsureSize(source, source.Length + v.Length);
			}
			else 
			{
                // 2011/1/22
                if (v == null)
                    return null;
				nIndex = 0;
				source = EnsureSize(source, v.Length);
			}

			Array.Copy(v,0,source, nIndex, v.Length);

			return source;
		}

        // 2011/9/12
        // �ڻ�����β��׷�������ֽ�
        public static byte[] Add(byte[] source,
            byte[] v,
            int nLength)
        {
            Debug.Assert(v.Length >= nLength, "");

            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + nLength);
            }
            else
            {
                if (v == null)
                    return null;
                nIndex = 0;
                source = EnsureSize(source, nLength);
            }

            Array.Copy(v, 0, source, nIndex, nLength);

            return source;
        }

		// �õ���16���Ʊ�ʾ��ʱ����ַ���
		public static string GetHexTimeStampString(byte [] baTimeStamp)
		{
			if (baTimeStamp == null)
				return "";
			string strText = "";
			for(int i=0;i<baTimeStamp.Length;i++) 
			{
				//string strHex = String.Format("{0,2:X}",baTimeStamp[i]);
				string strHex = Convert.ToString(baTimeStamp[i], 16);
				strText +=  strHex.PadLeft(2, '0');
			}

			return strText;
		}

		// �õ�byte[]���͵�ʱ���
		public static byte[] GetTimeStampByteArray(string strHexTimeStamp)
		{
			if (string.IsNullOrEmpty(strHexTimeStamp) == true)
				return null;

			byte [] result = new byte[strHexTimeStamp.Length / 2];

			for(int i=0;i<strHexTimeStamp.Length / 2;i++)
			{
				string strHex = strHexTimeStamp.Substring(i*2, 2);
				result[i] = Convert.ToByte(strHex, 16);

			}

			return result;
		}
	}

	/*
	/// <summary>
	/// һ���ԡ�ȫ�ֺ���
	/// </summary>
	public class General
	{
		public static long min(long a, long b)
		{
			return a < b ? a : b;
		}

		public static int min(int a, int b)
		{
			return a < b ? a : b;
		}

		public static long max(long a, long b)
		{
			return a > b ? a : b;
		}

		public static int max(int a, int b)
		{
			return a > b ? a : b;
		}
	}
	*/

	// ��д��: ���ӻ�
	public class ConvertUtil
	{

        // ��CopyTo()����
		// �Ѱ���string�����ArrayListת��Ϊstring[]����
        // parameters:
        //      nStartCol   ��ʼ���кš�һ��Ϊ0
        public static string[] GetStringArray(
            int nStartCol,
            ArrayList aText)
		{
			string[] result = new string[aText.Count+nStartCol];
			for(int i=0;i<aText.Count;i++)
			{
				result[i+nStartCol] = (string)aText[i];
			}
			return result;
		}

		//�ַ�����int32
		public static int S2Int32(string strText)
		{
			int nTemp = 0;
			try
			{
				nTemp = Convert.ToInt32(strText);
			}
			catch(Exception ex)
			{
				throw(new Exception ("�����ļ������ʵ�ֵ:"+strText+"\r\n"+ex.Message ));
			}
			return nTemp;
		}

		//�ַ�����int32��ָ�����ư汾
		public static int S2Int32(string strText,int nBase)
		{
			int nTemp = 0;
			try
			{
				nTemp = Convert.ToInt32(strText,nBase);
			}
			catch(Exception ex)
			{
				throw(new Exception ("�����ļ������ʵ�ֵ:"+strText+"\r\n"+ex.Message ));
			}
			return nTemp;
		}

        // ������Χ�Ƿ�Ϸ�,�����������ܹ�ȡ�ĳ���
        // parameter:
        //		nStart          ��ʼλ�� ����С��0
        //		nNeedLength     ��Ҫ�ĳ���	����С��-1��-1��ʾ��nStart-(nTotalLength-1)
        //		lTotalLength    ����ʵ���ܳ��� ����С��0
        //		nMaxLength      ���Ƶ���󳤶�	����-1����ʾ������
        //		lOutputLength   out���������صĿ����õĳ��� 2012/8/26 �޸�Ϊlong����
        //		strError        out���������س�����Ϣ
        // return:
        //		-1  ����
        //		0   �ɹ�
        public static int GetRealLength(long lStart,
            int nNeedLength,
            long lTotalLength,
            int nMaxLength,
            out long lOutputLength,
            out string strError)
        {
            lOutputLength = 0;
            strError = "";

            // ��ʼֵ,�����ܳ��Ȳ��Ϸ�
            if (lStart < 0
                || lTotalLength < 0)
            {
                strError = "��Χ����:nStart < 0 �� nTotalLength <0 \r\n";
                return -1;
            }
            if (lStart != 0
                && lStart >= lTotalLength)
            {
                strError = "��Χ����: ��ʼֵ "+lStart.ToString()+" �����ܳ��� "+lTotalLength.ToString()+"\r\n";
                return -1;
            }

            lOutputLength = nNeedLength;
            if (lOutputLength == 0)
            {
                return 0;
            }

            // ��Ϊ�м������ʱ��lOutoutLength��ֵ����һ�Ⱥܴ���������long���͡�����󾭹�����֮�󣬲��ᳬ��int�ķ�Χ

            if (lOutputLength == -1)  // �ӿ�ʼ��ȫ��
                lOutputLength = lTotalLength - lStart;

            if (lStart + lOutputLength > lTotalLength)
                lOutputLength = lTotalLength - lStart;

            // ��������󳤶�
            if (nMaxLength != -1 && nMaxLength >= 0)
            {
                if (lOutputLength > nMaxLength)
                    lOutputLength = nMaxLength;

                Debug.Assert(lOutputLength < Int32.MaxValue && lOutputLength > Int32.MinValue, "");
            }

            return 0;
        }
	}

#if NO


	public class ArrayListUtil
	{
		// ����: �ϲ������ַ�������
		// parameter:
		//		sourceLeft: Դ�������
		//		sourceRight: Դ�ұ�����
		//		targetLeft: Ŀ���������
		//		targetMiddle: Ŀ���м�����
		//		targetRight: Ŀ���ұ�����
		// �����׳��쳣
		public static void MergeStringArray(ArrayList sourceLeft,
			ArrayList sourceRight,
			List<string> targetLeft,
			List<string> targetMiddle,
			List<string> targetRight)
		{
			int i = 0;   
			int j = 0;
			string strLeft;
			string strRight;
			int ret;
			while (true)
			{
				strLeft = null;
				strRight = null;
				if (i >= sourceLeft.Count)
				{
					i = -1;
				}
				else if (i != -1)
				{
					try
					{
						strLeft = (string)sourceLeft[i];
					}
					catch
					{
						Exception ex = new Exception("i="+Convert.ToString(i)+"----Count="+Convert.ToString(sourceLeft.Count)+"<br/>");
						throw(ex);
					}
				}
				if (j >= sourceRight.Count)
				{
					j = -1;
				}
				else if (j != -1)
				{
					try
					{
						strRight = (string)sourceRight[j];
					}
					catch
					{
						Exception ex = new Exception("j="+Convert.ToString(j)+"----Count="+Convert.ToString(sourceLeft.Count)+sourceRight.GetHashCode()+"<br/>");
						throw(ex);
					}
				}
				if (i == -1 && j == -1)
				{
					break;
				}

				if (strLeft == null)
				{
					ret = 1;
				}
				else if (strRight == null)
				{
					ret = -1;
				}
				else
				{
					ret = strLeft.CompareTo(strRight);  //MyCompareTo(oldOneKey); //��CompareTO
				}

				if (ret == 0 && targetMiddle != null) 
				{
					targetMiddle.Add(strLeft);
					i++;
					j++;
				}

				if (ret<0) 
				{
					if (targetLeft != null && strLeft != null)
						targetLeft.Add(strLeft);
					i++;
				}

				if (ret>0 )
				{
					if (targetRight != null && strRight != null)
						targetRight.Add(strRight);
					j++;
				}
			}
		}
	}
#endif

}



