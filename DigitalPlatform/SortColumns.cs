using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.GUI;
using System.Diagnostics;

namespace DigitalPlatform
{
#if NO
    public enum ColumnSortStyle
    {
        LeftAlign = 0, // ������ַ���
        RightAlign = 1, // �Ҷ����ַ���
        RecPath = 2,    // ��¼·�������硰����ͼ��/1������'/'Ϊ�磬�ұ߲��ֵ�������ֵ���򡣻��ߡ�localhost/����ͼ��/ctlno/1��
        LongRecPath = 3,    // ��¼·�������硰����ͼ��/1 @���ط�������
        Extend = 4,    // ��չ������ʽ
    }
#endif

    // ��Ŀ����ʽ
    public class ColumnSortStyle
    {
        public string Name = "";
        public CompareEventHandler CompareFunc = null;

        public ColumnSortStyle(string strStyle)
        {
            this.Name = strStyle;
        }

        public static ColumnSortStyle None
        {
            get
            {
                return new ColumnSortStyle("");
            }
        }

        public static ColumnSortStyle LeftAlign
        {
            get
            {
                return new ColumnSortStyle("LeftAlign");
            }
        }

        public static ColumnSortStyle RightAlign
        {
            get
            {
                return new ColumnSortStyle("RightAlign");
            }
        }

        public static ColumnSortStyle RecPath
        {
            get
            {
                return new ColumnSortStyle("RecPath");
            }
        }

        public static ColumnSortStyle LongRecPath
        {
            get
            {
                return new ColumnSortStyle("LongRecPath");
            }
        }

        public static ColumnSortStyle IpAddress
        {
            get
            {
                return new ColumnSortStyle("IpAddress");
            }
        }

        public override bool Equals(System.Object obj)
        {
            ColumnSortStyle o = obj as ColumnSortStyle;
            if ((object)o == null)
                return false;

            if (this.Name == o.Name)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ColumnSortStyle a, ColumnSortStyle b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            return a.Name == b.Name;
        }

        public static bool operator !=(ColumnSortStyle a, ColumnSortStyle b)
        {
            return !(a == b);
        }
    }

    public class Column
    {
        public int No = -1;
        public bool Asc = true;
        public ColumnSortStyle SortStyle = ColumnSortStyle.None;   // ColumnSortStyle.LeftAlign;
    }

    public class SortColumns : List<Column>
    {
        // ��װ�汾��������ǰ�ĸ�ʽ
        // ������ͬһ�з������ô˺��������������toggle
        // ���ԣ������ñ��������趨�̶���������
        public void SetFirstColumn(int nFirstColumn,
            ListView.ColumnHeaderCollection columns)
        {
            SetFirstColumn(nFirstColumn,
                columns,
                true);
        }


        // parameters:
        //      bToggleDirection    ==true ��nFirstColumn�����Ѿ��ǵ�ǰ��һ�У��������������
        public void SetFirstColumn(int nFirstColumn,
            ListView.ColumnHeaderCollection columns,
            bool bToggleDirection)
        {
            int nIndex = -1;
            Column column = null;
            // �ҵ�����к�
            for (int i = 0; i < this.Count; i++)
            {
                column = this[i];
                if (column.No == nFirstColumn)
                {
                    nIndex = i;
                    break;
                }
            }

            ColumnSortStyle firstColumnStyle = ColumnSortStyle.None;   //  ColumnSortStyle.LeftAlign;

            // �Զ������Ҷ�����
            // 2008/8/30 changed
            if (columns[nFirstColumn].TextAlign == HorizontalAlignment.Right)
                firstColumnStyle = ColumnSortStyle.RightAlign;

            // �����Ѿ��ǵ�һ�У������������
            if (nIndex == 0 && bToggleDirection == true)
            {
                if (column.Asc == true)
                    column.Asc = false;
                else
                    column.Asc = true;

                // �޸���һ�е��Ӿ�
                ColumnHeader header = columns[column.No];

                SetHeaderText(header,
                    nIndex,
                    column);
                return;
            }

            if (nIndex != -1)
            {
                // �������������Ѿ����ڵ�ֵ
                this.RemoveAt(nIndex);
            }
            else
            {
                column = new Column();
                column.No = nFirstColumn;
                column.Asc = true;  // ��ʼʱΪ��������
                column.SortStyle = firstColumnStyle;    // 2007/12/20
            }

            // �ŵ��ײ�
            this.Insert(0, column);

            // �޸�ȫ���е��Ӿ�
            RefreshColumnDisplay(columns);
        }

        // �޸��������飬���õ�һ�У���ԭ�����к��ƺ�
        // parameters:
        //      bToggleDirection    ==true ��nFirstColumn�����Ѿ��ǵ�ǰ��һ�У��������������
        public void SetFirstColumn(int nFirstColumn,
            ColumnSortStyle firstColumnStyle,
            ListView.ColumnHeaderCollection columns,
            bool bToggleDirection)
        {
            int nIndex = -1;
            Column column = null;
            // �ҵ�����к�
            for (int i = 0; i < this.Count; i++)
            {
                column = this[i];
                if (column.No == nFirstColumn)
                {
                    nIndex = i;
                    break;
                }
            }

            // �����Ѿ��ǵ�һ�У������������
            if (nIndex == 0 && bToggleDirection == true)
            {
                if (column.Asc == true)
                    column.Asc = false;
                else
                    column.Asc = true;

                column.SortStyle = firstColumnStyle;    // 2008/11/30

                // �޸���һ�е��Ӿ�
                ColumnHeader header = columns[column.No];

                SetHeaderText(header,
                    nIndex,
                    column);
                return;
            }

            if (nIndex != -1)
            {
                // �������������Ѿ����ڵ�ֵ
                this.RemoveAt(nIndex);
            }
            else
            {
                column = new Column();
                column.No = nFirstColumn;
                column.Asc = true;  // ��ʼʱΪ��������
                column.SortStyle = firstColumnStyle;    // 2007/12/20
            }

            // �ŵ��ײ�
            this.Insert(0, column);

            // �޸�ȫ���е��Ӿ�
            RefreshColumnDisplay(columns);
        }

        void DisplayColumnsText(ListView.ColumnHeaderCollection columns)
        {
            Debug.WriteLine("***");
            foreach (ColumnHeader column0 in columns)
            {
                Debug.WriteLine(column0.Text);
            }
            Debug.WriteLine("***");
        }

        // �޸�ȫ���е��Ӿ�
        public void RefreshColumnDisplay(ListView.ColumnHeaderCollection columns)
        {
#if DEBUG
            DisplayColumnsText(columns);
#endif
            Column column = null;
            for (int i = 0; i < this.Count; i++)
            {
                column = this[i];

                ColumnHeader header = columns[column.No];

                SetHeaderText(header,
                    i,
                    column);
            }
#if DEBUG
            DisplayColumnsText(columns);
#endif
        }

        // �ָ�û���κ������־���б�����������
        public static void ClearColumnSortDisplay(ListView.ColumnHeaderCollection columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                ColumnHeader header = columns[i];

                ColumnProperty prop = (ColumnProperty)header.Tag;
                if (prop != null)
                {
                    header.Text = prop.Title;
                }
            }
        }

        // ����ColumnHeader����
        public static void SetHeaderText(ColumnHeader header,
            int nSortNo,
            Column column)
        {
            ColumnProperty prop = (ColumnProperty)header.Tag;

            //string strOldText = "";
            if (prop != null)
            {
                // strOldText = (string)header.Tag;
            }
            else
            {
                // strOldText = header.Text;
                // ��������
                prop = new ColumnProperty(header.Text);
                header.Tag = prop; 
            }

            string strNewText = 
                (column.Asc == true ? "��" : "��")
                + (nSortNo + 1).ToString()
                + " "
                + prop.Title;   //  strOldText;
            header.Text = strNewText;

            // 2008/11/30
            if (column.SortStyle == ColumnSortStyle.RightAlign)
            {
                if (header.TextAlign != HorizontalAlignment.Right)
                    header.TextAlign = HorizontalAlignment.Right;
            }
            else
            {
                if (header.TextAlign != HorizontalAlignment.Left)
                    header.TextAlign = HorizontalAlignment.Left;
            }
        }
    }

    // Implements the manual sorting of items by columns.
    public class SortColumnsComparer : IComparer
    {
        SortColumns SortColumns = new SortColumns();

        // ��һ�� SortStyle ����Ԥ֪�����͵�ʱ��ʹ����� handler ����
        public event CompareEventHandler EventCompare = null;

        public SortColumnsComparer()
        {
            Column column = new Column();
            column.No = 0;
            this.SortColumns.Add(column);
        }
        public SortColumnsComparer(SortColumns sortcolumns)
        {
            this.SortColumns = sortcolumns;
        }

        // ����¼·���и�Ϊ�������֣���߲��ֺ��ұ߲��֡�
        // ����ͼ��/1
        // �ұ߲����Ǵ��ҿ�ʼ�ҵ���һ��'/'�ұߵĲ��֣����Բ���·�����̣�һ�������ұߵ����ֲ���
        static void SplitRecPath(string strRecPath,
            out string strLeft,
            out string strRight)
        {
            int nRet = strRecPath.LastIndexOf("/");
            if (nRet == -1)
            {
                strLeft = strRecPath; // ���û��б�ܣ�������߲��֡���һ���к����廹��Ҫ��ϸ����
                strRight = "";
                return;
            }

            strLeft = strRecPath.Substring(0, nRet);
            strRight = strRecPath.Substring(nRet + 1);
        }

        static void SplitLongRecPath(string strRecPath,
            out string strLeft,
            out string strRight,
            out string strServerName)
        {
            int nRet = 0;

            nRet = strRecPath.IndexOf("@");
            if (nRet != -1)
            {
                strServerName = strRecPath.Substring(nRet + 1).Trim();
                strRecPath = strRecPath.Substring(0, nRet).Trim();
            }
            else
                strServerName = "";
            
            nRet = strRecPath.LastIndexOf("/");
            if (nRet == -1)
            {
                strLeft = strRecPath;
                strRight = "";
                return;
            }

            strLeft = strRecPath.Substring(0, nRet);
            strRight = strRecPath.Substring(nRet + 1);
        }

        // �Ҷ���Ƚ��ַ���
        // parameters:
        //      chFill  ����õ��ַ�
        public static int RightAlignCompare(string s1, string s2, char chFill = '0')
        {
            if (s1 == null)
                s1 = "";
            if (s2 == null)
                s2 = "";
            int nMaxLength = Math.Max(s1.Length, s2.Length);
            return string.CompareOrdinal(s1.PadLeft(nMaxLength, chFill),
                s2.PadLeft(nMaxLength, chFill));
        }

        // �Ƚ����� IP ��ַ
        public static int CompareIpAddress(string s1, string s2)
        {
            if (s1 == null)
                s1 = "";
            if (s2 == null)
                s2 = "";

            string[] parts1 = s1.Split(new char[] { '.', ':' });
            string[] parts2 = s2.Split(new char[] { '.', ':' });

            for (int i = 0; i < Math.Min(parts1.Length, parts2.Length); i++)
            {
                if (i >= parts1.Length)
                    break;
                if (i >= parts2.Length)
                    break;
                string n1 = parts1[i];
                string n2 = parts2[i];
                int nRet = RightAlignCompare(n1, n2);
                if (nRet != 0)
                    return nRet;
            }

            return (parts1.Length - parts2.Length);
        }

        public int Compare(object x, object y)
        {
            for (int i = 0; i < this.SortColumns.Count; i++)
            {
                Column column = this.SortColumns[i];

                string s1 = "";
                try
                {
                    s1 = ((ListViewItem)x).SubItems[column.No].Text;
                }
                catch
                {
                }
                string s2 = "";
                try
                {
                    s2 = ((ListViewItem)y).SubItems[column.No].Text;
                }
                catch
                {
                }

                int nRet = 0;

                if (column.SortStyle == null)
                {
                    nRet = String.Compare(s1, s2);
                }
                else if (column.SortStyle.CompareFunc != null)
                {
                    // �������������ֱ����������
                    CompareEventArgs e = new CompareEventArgs();
                    e.Column = column;
                    e.SortColumnIndex = i;
                    // e.ColumnIndex = column.No;
                    e.String1 = s1;
                    e.String2 = s2;
                    column.SortStyle.CompareFunc(this, e);
                    nRet = e.Result;
                }
                else if (column.SortStyle == ColumnSortStyle.None 
                    || column.SortStyle == ColumnSortStyle.LeftAlign)
                {
                    nRet = String.Compare(s1, s2);
                }
                else if (column.SortStyle == ColumnSortStyle.RightAlign)
                {
#if NO
                    int nMaxLength = s1.Length;
                    if (s2.Length > nMaxLength)
                        nMaxLength = s2.Length;

                    s1 = s1.PadLeft(nMaxLength, ' ');
                    s2 = s2.PadLeft(nMaxLength, ' ');

                    nRet = String.Compare(s1, s2);
#endif
                    nRet = RightAlignCompare(s1, s2, ' ');
                }
                else if (column.SortStyle == ColumnSortStyle.RecPath)
                {
                    string strLeft1;
                    string strRight1;
                    string strLeft2;
                    string strRight2;
                    SplitRecPath(s1, out strLeft1, out strRight1);
                    SplitRecPath(s2, out strLeft2, out strRight2);

                    nRet = String.Compare(strLeft1, strLeft2);
                    if (nRet != 0)
                        goto END1;

#if NO
                    // �Լ�¼�Ų��ֽ����Ҷ���ıȽ�
                    int nMaxLength = strRight1.Length;
                    if (strRight2.Length > nMaxLength)
                        nMaxLength = strRight2.Length;

                    strRight1 = strRight1.PadLeft(nMaxLength, ' ');
                    strRight2 = strRight2.PadLeft(nMaxLength, ' ');

                    nRet = String.Compare(strRight1, strRight2);
#endif
                    nRet = RightAlignCompare(strRight1, strRight2, ' ');
                }
                else if (column.SortStyle == ColumnSortStyle.LongRecPath)
                {
                    string strLeft1;
                    string strRight1;
                    string strServerName1;
                    string strLeft2;
                    string strRight2;
                    string strServerName2;

                    SplitLongRecPath(s1, out strLeft1, out strRight1, out strServerName1);
                    SplitLongRecPath(s2, out strLeft2, out strRight2, out strServerName2);

                    nRet = String.Compare(strServerName1, strServerName2);
                    if (nRet != 0)
                        goto END1;

                    nRet = String.Compare(strLeft1, strLeft2);
                    if (nRet != 0)
                        goto END1;

                    // �Լ�¼�Ų��ֽ����Ҷ���ıȽ�
                    int nMaxLength = strRight1.Length;
                    if (strRight2.Length > nMaxLength)
                        nMaxLength = strRight2.Length;

                    strRight1 = strRight1.PadLeft(nMaxLength, ' ');
                    strRight2 = strRight2.PadLeft(nMaxLength, ' ');

                    nRet = String.Compare(strRight1, strRight2);

                }
                else if (column.SortStyle == ColumnSortStyle.IpAddress)
                {
                    nRet = CompareIpAddress(s1, s2);
                }
                else if (this.EventCompare != null)
                {
                    CompareEventArgs e = new CompareEventArgs();
                    e.Column = column;
                    e.SortColumnIndex = i;
                    e.String1 = s1;
                    e.String2 = s2;
                    this.EventCompare(this, e);
                    nRet = e.Result;
                }
                else
                {
                    // ����ʶ��ķ�ʽ����������봦��
                    nRet = String.Compare(s1, s2);
                }

                END1:
                if (nRet != 0)
                {
                    if (column.Asc == true)
                        return nRet;
                    else
                        return -nRet;
                }
            }

            return 0;
        }
    }

    public delegate void CompareEventHandler(object sender,
        CompareEventArgs e);

    public class CompareEventArgs : EventArgs
    {
        public Column Column = null;    // ������
        public int SortColumnIndex = -1;    // ������ index���� Column �� SortColumns �����е��±�
        public string String1 = "";
        public string String2 = "";
        public int Result = 0;  // [out]
    }
}
