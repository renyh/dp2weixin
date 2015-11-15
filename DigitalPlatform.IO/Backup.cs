using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DigitalPlatform.IO
{
	/// <summary>
	/// ���� .dp2bak ��ʽ���ļ�
	/// </summary>
	public class Backup
	{

		// ����һ��Res��ȫ����Դ��ϳ�������Res����,д������
		// ���ñ�����ǰ��ע����ļ�ָ�����õ��ʵ�λ��(�����ļ���ĩβ������Ҫ����λ�õĿ�ʼ)��
		public static long WriteFirstResToBackupFile(Stream outputfile,
			string strMetaData,
			string strBody)
		{
			long lStart = outputfile.Position;

			outputfile.Seek(8, SeekOrigin.Current);

			// д��metadata�ĳ���, 8bytes
			byte[] data = Encoding.UTF8.GetBytes(strMetaData);
			long lMetaDataLength = data.Length;

			outputfile.Write(BitConverter.GetBytes(lMetaDataLength), 0, 8);

			// д��metadata����
			outputfile.Write(data, 0, data.Length);

			// ׼��Body����
			data = Encoding.UTF8.GetBytes(strBody);

			// д��body�ĳ���, 8bytes
			long lBodyLength = data.Length;
			outputfile.Write(BitConverter.GetBytes(lBodyLength), 0, 8);

			// д��body����
			outputfile.Write(data, 0 , data.Length);

			long lTotalLength = outputfile.Position - lStart - 8;	// ������

			// ���д��ʼ���ܳ���
            outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lStart, "");

            // outputfile.Seek(lStart, SeekOrigin.Begin);     // �ļ������Ժ���仰�����ܻ�ܲ�
			outputfile.Write(BitConverter.GetBytes(lTotalLength), 0, 8);

			// ��β,Ϊ�������д���ú��ļ�ָ��
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return lTotalLength + 8;	// ����ë����
		}

		// ����һ��Res�����Res����,д������
		// ���ñ�����ǰ��ע����ļ�ָ�����õ��ʵ�λ��(�����ļ���ĩβ������Ҫ����λ�õĿ�ʼ)��
		public static long WriteOtherResToBackupFile(Stream outputfile,
			string strMetaData,
			string strBodyFileName)
		{
			long lStart = outputfile.Position;

			outputfile.Seek(8, SeekOrigin.Current);

			// д��metadata�ĳ���, 8bytes
			byte[] data = Encoding.UTF8.GetBytes(strMetaData);
			long lMetaDataLength = data.Length;

			outputfile.Write(BitConverter.GetBytes(lMetaDataLength), 0, 8);

			// д��metadata����
			outputfile.Write(data, 0, data.Length);

			FileStream fileSource = File.Open(
				strBodyFileName,
				FileMode.Open,
				FileAccess.Read, 
				FileShare.ReadWrite);

			try 
			{

			// body�ĳ���, 8bytes
				long lBodyLength = fileSource.Length;
				outputfile.Write(BitConverter.GetBytes(lBodyLength), 0, 8);

				// body����
				StreamUtil.DumpStream(fileSource, outputfile);
			}
			finally 
			{
				fileSource.Close();
			}

			long lTotalLength = outputfile.Position - lStart - 8;	// ������

			// ���д��ʼ���ܳ���
            outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lStart, "");

			// outputfile.Seek(lStart, SeekOrigin.Begin);   // ����
			outputfile.Write(BitConverter.GetBytes(lTotalLength), 0, 8);

			// ��β,Ϊ�������д���ú��ļ�ָ��
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return lTotalLength + 8;	// ����ë����
		}

		// дres��ͷ��
		// �������Ԥ��ȷ֪����res�ĳ��ȣ����������һ��lTotalLengthֵ���ñ�������
		// ������Ҫ�����º��������ص�lStart��������EndWriteResToBackupFile()��
		// �����Ԥ��ȷ֪����res�ĳ��ȣ�����󲻱ص���EndWriteResToBackupFile()
		public static long BeginWriteResToBackupFile(Stream outputfile,
			long lTotalLength,
			out long lStart)
		{
			lStart = outputfile.Position;

			outputfile.Write(BitConverter.GetBytes(lTotalLength), 0, 8);

			return 0;
		}

		public static long EndWriteResToBackupFile(
			Stream outputfile,
			long lTotalLength,
			long lStart)
		{
			// ���д��ʼ���ܳ���
            outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lStart, "");

			// outputfile.Seek(lStart, SeekOrigin.Begin);   // ����
			outputfile.Write(BitConverter.GetBytes(lTotalLength), 0, 8);

			// ��β,Ϊ�������д���ú��ļ�ָ��
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return 0;
		}

		public static long WriteResMetadataToBackupFile(Stream outputfile,
			string strMetaData)
		{
			// д��metadata�ĳ���, 8bytes
			byte[] data = Encoding.UTF8.GetBytes(strMetaData);
			long lMetaDataLength = data.Length;

			outputfile.Write(BitConverter.GetBytes(lMetaDataLength), 0, 8);

			// д��metadata����
			outputfile.Write(data, 0, data.Length);

			return 0;
		}

		// дres body��ͷ��
		// �������Ԥ��ȷ֪body�ĳ��ȣ����������һ��lBodyLengthֵ���ñ�������
		// ������Ҫ�����º��������ص�lBodyStart��������EndWriteResBodyToBackupFile()��
		// �����Ԥ��ȷ֪body�ĳ��ȣ�����󲻱ص���EndWriteResBodyToBackupFile()
		// parameters:
		//		lBodyStart	����res body��δд���Ǽ���д��λ�ã�Ҳ������δд8byte�ߴ��λ��
		public static long BeginWriteResBodyToBackupFile(
			Stream outputfile,
			long lBodyLength,
			out long lBodyStart)
		{
			lBodyStart = outputfile.Position;

			outputfile.Write(BitConverter.GetBytes(lBodyLength), 0, 8);
			return 0;
		}

		// res body��β
		public static long EndWriteResBodyToBackupFile(
			Stream outputfile,
			long lBodyLength,
			long lBodyStart)
		{
			// ���д��ʼ���ܳ���
            outputfile.Seek(lBodyStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lBodyStart, "");

			// outputfile.Seek(lBodyStart, SeekOrigin.Begin);  // ����
			outputfile.Write(BitConverter.GetBytes(lBodyLength), 0, 8);

			// ��β,Ϊ�������д���ú��ļ�ָ��
			outputfile.Seek(lBodyLength, SeekOrigin.Current);

			return 0;
		}

		public Backup()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
