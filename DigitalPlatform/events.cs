using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

// �����¼�����

namespace DigitalPlatform
{
    /// <summary>
    /// �����¼�
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void IdleEventHandler(object sender,
    IdleEventArgs e);

    /// <summary>
    /// �����¼��Ĳ���
    /// </summary>
    public class IdleEventArgs : EventArgs
    {
        public bool bDoEvents = true;
    }

    /// <summary>
    /// ���ݷ����ı�
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void ContentChangedEventHandler(object sender,
    ContentChangedEventArgs e);

    /// <summary>
    /// ���ֵ�б�Ĳ���
    /// </summary>
    public class ContentChangedEventArgs : EventArgs
    {
        public bool OldChanged = false;
        public bool CurrentChanged = false;
    }

    /// <summary>
    /// ���ֵ�б�
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void GetValueTableEventHandler(object sender,
    GetValueTableEventArgs e);

    /// <summary>
    /// ���ֵ�б�Ĳ���
    /// </summary>
    public class GetValueTableEventArgs : EventArgs
    {
        public string TableName = "";

        public string DbName = "";


        /// <summary>
        /// ֵ�б�
        /// </summary>
        public string[] values = null;

    }

    ///
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void ControlKeyPressEventHandler(object sender,
        ControlKeyPressEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public class ControlKeyPressEventArgs : EventArgs
    {
        public KeyPressEventArgs e = null;

        // ��������������
        public string Name = "";
    }



    ///
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void ControlKeyEventHandler(object sender,
        ControlKeyEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public class ControlKeyEventArgs : EventArgs
    {
        public KeyEventArgs e = null;

        // ��������������
        public string Name = "";

        /*
        // �������ڵ��ӿؼ�
        // 2009/2/24
        public object SenderControl = null;
         * */
    }

    public delegate void ApendMenuEventHandler(object sender,
    AppendMenuEventArgs e);

    public class AppendMenuEventArgs : EventArgs
    {
        public ContextMenu ContextMenu = null;  // [in]
    }

    public class LockException : Exception
    {
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public LockException(string strText)
            : base(strText)
        {
        }
    }
}
