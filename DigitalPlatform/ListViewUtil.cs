using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DigitalPlatform.GUI
{
    public class ListViewUtil
    {
        public static int GetColumnHeaderHeight(ListView list)
        {
            RECT rc = new RECT();
            IntPtr hwnd = API.SendMessage(list.Handle, API.LVM_GETHEADER, 0, 0);
            if (hwnd == null)
                return -1;

            if (API.GetWindowRect(new HandleRef(null, hwnd), out rc))
            {
                return rc.bottom - rc.top;
            }

            return -1;
        }

        // 2012/5/9
        // �����������б�
        public static string GetItemNameList(ListView.SelectedListViewItemCollection items,
            string strSep = ",")
        {
            StringBuilder strItemNameList = new StringBuilder(4096);
            foreach (ListViewItem item in items)
            {
                if (strItemNameList.Length > 0)
                    strItemNameList.Append(strSep);
                strItemNameList.Append(item.Text);
            }

            return strItemNameList.ToString();
        }

        // 2012/5/9
        // �����������б�
        public static string GetItemNameList(ListView list,
            string strSep = ",")
        {
            StringBuilder strItemNameList = new StringBuilder(4096);
            foreach (ListViewItem item in list.SelectedItems)
            {
                if (strItemNameList.Length > 0)
                    strItemNameList.Append(strSep);
                strItemNameList.Append(item.Text);
            }

            return strItemNameList.ToString();
        }

        // �����ƶ�����Ĳ˵��Ƿ�Ӧ��ʹ��
        public static bool MoveItemEnabled(
            ListView list,
            bool bUp)
        {
            if (list.SelectedItems.Count == 0)
                return false;
            int index = list.SelectedIndices[0];
            if (bUp == true)
            {
                if (index == 0)
                    return false;
                return true;
            }
            else
            {
                if (index >= list.Items.Count - 1)
                    return false;
                return true;
            }
        }

        // parameters:
        //      indices �����ƶ��漰�����±�λ�á���һ��Ԫ�����ƶ�ǰ��λ�ã��ڶ���Ԫ�����ƶ����λ��
        public static int MoveItemUpDown(
            ListView list,
            bool bUp,
            out List<int> indices,
            out string strError)
        {
            strError = "";
            indices = new List<int>();
            // int nRet = 0;

            if (list.SelectedItems.Count == 0)
            {
                strError = "��δѡ��Ҫ���������ƶ�������";
                return -1;
            }

            // ListViewItem item = list.SelectedItems[0];
            // int index = list.Items.IndexOf(item);
            // Debug.Assert(index >= 0 && index <= list.Items.Count - 1, "");
            int index = list.SelectedIndices[0];
            ListViewItem item = list.Items[index];

            indices.Add(index);

            bool bChanged = false;

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "��ͷ";
                    return -1;
                }

                list.Items.RemoveAt(index);
                index--;
                list.Items.Insert(index, item);
                indices.Add(index);
                list.FocusedItem = item;

                bChanged = true;
            }

            if (bUp == false)
            {
                if (index >= list.Items.Count - 1)
                {
                    strError = "��β";
                    return -1;
                }
                list.Items.RemoveAt(index);
                index++;
                list.Items.Insert(index, item);
                indices.Add(index);
                list.FocusedItem = item;

                bChanged = true;
            }

            if (bChanged == true)
                return 1;
            return 0;
        }

        public static void DeleteSelectedItems(ListView list)
        {
            int[] indices = new int[list.SelectedItems.Count];
            list.SelectedIndices.CopyTo(indices, 0);

            list.BeginUpdate();

            for (int i = indices.Length - 1; i >= 0; i--)
            {
                list.Items.RemoveAt(indices[i]);
            }

            list.EndUpdate();

#if NO
            for (int i = list.SelectedIndices.Count - 1;
    i >= 0;
    i--)
            {
                int index = list.SelectedIndices[i];
                list.Items.RemoveAt(index);
            }
#endif
#if NO
            foreach (ListViewItem item in list.SelectedItems)
            {
                list.Items.Remove(item);
            }
#endif
        }

        public static void SelectAllLines(ListView list)
        {
            list.BeginUpdate();
            for (int i = 0; i < list.Items.Count; i++)
            {
                list.Items[i].Selected = true;
            }
            list.EndUpdate();
        }

        // ����б������ַ���
        public static string GetColumnWidthListString(ListView list)
        {
            string strResult = "";
            for (int i = 0; i < list.Columns.Count; i++)
            {
                ColumnHeader header = list.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += header.Width.ToString();
            }

            return strResult;
        }

        // ����б������ַ���
        // ��չ���ܰ汾���������ұ�������û�б������ֵ���
        public static string GetColumnWidthListStringExt(ListView list)
        {
            string strResult = "";
            int nEndIndex = list.Columns.Count;
            for (int i = list.Columns.Count-1; i >= 0; i--)
            {
                ColumnHeader header = list.Columns[i];
                if (String.IsNullOrEmpty(header.Text) == false)
                    break;
                nEndIndex = i;
            }
            for (int i = 0; i < nEndIndex; i++)
            {
                ColumnHeader header = list.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += header.Width.ToString();
            }

            return strResult;
        }

        // �����б���Ŀ��
        // parameters:
        //      bExpandColumnCount  �Ƿ�Ҫ��չ�б��⵽�㹻��Ŀ��
        public static void SetColumnHeaderWidth(ListView list,
            string strWidthList,
            bool bExpandColumnCount)
        {
            string[] parts = strWidthList.Split(new char[] {','});

            if (bExpandColumnCount == true)
                EnsureColumns(list, parts.Length, 100);

            for (int i = 0; i < parts.Length; i++)
            {
                if (i >= list.Columns.Count)
                    break;

                string strValue = parts[i].Trim();
                int nWidth = -1;
                try
                {
                    nWidth = Convert.ToInt32(strValue);
                }
                catch
                {
                    break;
                }

                if (nWidth != -1)
                    list.Columns[i].Width = nWidth;
            }
        }



        // ��Ӧѡ���Ƿ����仯�Ķ������޸���Ŀ��������
        // parameters:
        //      protect_column_numbers  ��Ҫ�������е��к����顣�кŴ�0��ʼ���㡣��ν�������ǲ��ƻ��������еı��⣬���ñ��������������п�ʼ����nRecPathColumn��ʾ���кŲ������뱾���飬Ҳ���Զ��ܵ��������������Ҫ��������������null
        public static void OnSeletedIndexChanged(ListView list,
            int nRecPathColumn,
            List<int> protect_column_numbers)
        {
            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
            {
                throw new Exception("ListView��Tagû�а���ListViewProperty����");
            }

            if (list.SelectedItems.Count == 0)
            {
                // ���������Ŀ����Ϊ1,2,3...�����߱�����ǰ�Ĳ���ֵ?
                return;
            }

            ListViewItem item = list.SelectedItems[0];
            // ���·�����ٶ����ڵ�һ�У�
            string strRecPath = GetItemText(item, nRecPathColumn);


            ColumnPropertyCollection props = null;
            string strDbName = "";

            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                strDbName = "<blank>";  // ��������ݿ�������ʾ��һ�пյ����
                props = prop.GetColumnName(strDbName);
                goto DO_REFRESH;
            }

            // ȡ�����ݿ���
            strDbName = prop.ParseDbName(strRecPath);   //  GetDbName(strRecPath);

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                return;
            }

            if (strDbName == prop.CurrentDbName)
                return; // û�б�Ҫˢ��

            props = prop.GetColumnName(strDbName);

            DO_REFRESH:

            if (props == null)
            {
                // not found

                // ���������Ŀ����Ϊ1,2,3...�����߱�����ǰ�Ĳ���ֵ?
                props = new ColumnPropertyCollection();
            }


            // �޸�����
            int index = 0;
            for (int i = 0; i < list.Columns.Count; i++)
            {
                ColumnHeader header = list.Columns[i];

                if (i == nRecPathColumn)
                    continue;

                // Խ����Ҫ��������
                if (protect_column_numbers != null)
                {
                    if (protect_column_numbers.IndexOf(i) != -1)
                        continue;
                }

#if NO
                if (index < props.Count)
                {
                    if (header.Tag != null)
                        header.Tag = props[index];
                    else
                        header.Text = props[index].Title;
                }
                else 
                {
                    ColumnProperty temp = (ColumnProperty)header.Tag;

                    if (temp == null)
                        header.Text = i.ToString();
                    else
                        header.Text = temp.Title;
                }
#endif

                ColumnProperty temp = (ColumnProperty)header.Tag;

                if (index < props.Count)
                {
                    if (temp != props[index])
                    {
                        header.Tag = props[index];
                        temp = props[index];
                    }
                }
                else
                    temp = null;    // 2013/10/5 ������Ҳ���������У���Ҫ��ʾΪ����

                if (temp == null)
                {
                    // ��� header ��ǰ�����־����ã�û��ʱ��ʹ�ñ����� 2014/9/6 ���� BUG
                    if (string.IsNullOrEmpty(header.Text) == true)
                        header.Text = i.ToString();
                }
                else
                    header.Text = temp.Title;

                index++;
            }

            // ˢ�������е���ʾ��Ҳ����˵ˢ����Щ����������ĸ����е���ʾ
            prop.SortColumns.RefreshColumnDisplay(list.Columns);

            prop.CurrentDbName = strDbName; // ����
        }

        // ��Ӧ�����Ŀ����Ķ�������������
        // parameters:
        //      bClearSorter    �Ƿ����������� sorter ����
        public static void OnColumnClick(ListView list,
            ColumnClickEventArgs e,
            bool bClearSorter = true)
        {
            int nClickColumn = e.Column;

            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
            {
                throw new Exception("ListView��Tagû�а���ListViewProperty����");
            }

            // 2013/3/31
            // ���������û�г�ʼ��������Ҫ�ȳ�ʼ��
            if (list.SelectedItems.Count == 0 && list.Items.Count > 0)
            {
                list.Items[0].Selected = true;
                OnSeletedIndexChanged(list,
                    0,
                    null);
                list.Items[0].Selected = false;
            }

            ColumnSortStyle sortStyle = prop.GetSortStyle(list, nClickColumn);

            prop.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                list.Columns,
                true);

            // ����
            SortColumnsComparer sorter = new SortColumnsComparer(prop.SortColumns);
            if (prop.HasCompareColumnEvent() == true)
            {
                sorter.EventCompare += (sender1, e1) =>
                {
                    prop.OnCompareColumn(sender1, e1);
                };
            }
            list.ListViewItemSorter = sorter;

            if (bClearSorter == true)
                list.ListViewItemSorter = null;
        }

        class SetSortStyleParam
        {
            public ColumnSortStyle Style;
            public ListViewProperty prop = null;
            public int ColumnIndex = -1;
        }

        // ��Ӧ����Ҽ������Ŀ����Ķ��������������Ĳ˵�
        public static void OnColumnContextMenuClick(ListView list,
            ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
            {
                throw new Exception("ListView��Tagû�а���ListViewProperty����");
            }

#if NO
            ColumnSortStyle sortStyle = prop.GetSortStyle(nClickColumn);
            prop.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                list.Columns,
                true);
#endif
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;

// list.Columns[nClickColumn].Text
            menuItem = new ToolStripMenuItem("��������ʽ");
            contextMenu.Items.Add(menuItem);

            ColumnSortStyle sortStyle = prop.GetSortStyle(list, nClickColumn);
            if (sortStyle == null)
                sortStyle = ColumnSortStyle.None;

            List<ColumnSortStyle> all_styles = prop.GetAllSortStyle(list, nClickColumn);

            foreach (ColumnSortStyle style in all_styles)
            {
                subMenuItem = new ToolStripMenuItem();
                subMenuItem.Text = GetSortStyleCaption(style);
                SetSortStyleParam param = new SetSortStyleParam();
                param.ColumnIndex = nClickColumn;
                param.prop = prop;
                param.Style = style;
                subMenuItem.Tag = param;
                subMenuItem.Click += new EventHandler(menu_setSortStyle_Click);
                if (style == sortStyle)
                    subMenuItem.Checked = true;
                menuItem.DropDown.Items.Add(subMenuItem);
            }

            Point p = list.PointToClient(Control.MousePosition);
            contextMenu.Show(list, p);
        }

        static string GetSortStyleCaption(ColumnSortStyle style)
        {
            string strName = style.Name;
            if (string.IsNullOrEmpty(strName) == true)
                return "[None]";

            // �� call_number ��̬ת��Ϊ CallNumber ��̬
            string[] parts = strName.Split(new char[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder text = new StringBuilder(4096);
            foreach (string s in parts)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;

                text.Append(char.ToUpper(s[0]));
                if (s.Length > 1)
                    text.Append(s.Substring(1));
            }

            return text.ToString();
        }

        static void menu_setSortStyle_Click(object sender, EventArgs e)
        {
            var menu = sender as ToolStripMenuItem;
            var param = menu.Tag as SetSortStyleParam;
            param.prop.SetSortStyle(param.ColumnIndex, param.Style);
        }

        // ������������������Ϣ��ˢ��list�ı������ϵĳ¾ɵ������־
        public static void ClearSortColumns(ListView list)
        {
            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
                return;

            prop.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(list.Columns);

            prop.CurrentDbName = "";    // �������
        }

        // ���ListViewProperty����
        public static ListViewProperty GetListViewProperty(ListView list)
        {
            if (list.Tag == null)
                return null;
            if (!(list.Tag is ListViewProperty))
                return null;

            return (ListViewProperty)list.Tag;
        }

        // ����һ������
        public static ListViewItem FindItem(ListView listview,
            string strText,
            int nColumn)
        {
            for (int i = 0; i < listview.Items.Count; i++)
            {
                ListViewItem item = listview.Items[i];
                string strThisText = GetItemText(item, nColumn);
                if (strThisText == strText)
                    return item;
            }

            return null;
        }

        // ���һ��xλ���ں����ϡ�
        // return:
        //		-1	û������
        //		���� �к�
        public static int ColumnHitTest(ListView listview,
            int x)
        {
            int nStart = 0;
            for (int i = 0; i < listview.Columns.Count; i++)
            {
                ColumnHeader header = listview.Columns[i];
                if (x >= nStart && x < nStart + header.Width)
                    return i;
                nStart += header.Width;
            }

            return -1;
        }

        // ȷ���б��������㹻
        public static void EnsureColumns(ListView listview,
            int nCount,
            int nInitialWidth = 200)
        {
            if (listview.Columns.Count >= nCount)
                return;

            for (int i = listview.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                // strText = Convert.ToString(i);

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = nInitialWidth;
                listview.Columns.Add(col);
            }
        }

        // ���һ����Ԫ��ֵ
        public static string GetItemText(ListViewItem item,
            int col)
        {
            if (col == 0)
                return item.Text;

            // 2008/5/14��������׳��쳣
            if (col >= item.SubItems.Count)
                return "";

            return item.SubItems[col].Text;
        }

        // �޸�һ����Ԫ��ֵ
        public static void ChangeItemText(ListViewItem item,
            int col,
            string strText)
        {
            // ȷ���̰߳�ȫ 2014/9/3
            if (item.ListView != null && item.ListView.InvokeRequired)
            {
                item.ListView.BeginInvoke(new Action<ListViewItem, int, string>(ChangeItemText), item, col, strText);
                return;
            }

            if (col == 0)
            {
                item.Text = strText;
                return;
            }

            // ����
            while (item.SubItems.Count < col + 1)   // ԭ��Ϊ<=, ����ɶ��һ�еĺ�� 2006/10/9 changed
            {
                item.SubItems.Add("");
            }

#if NO
            item.SubItems.RemoveAt(col);
            item.SubItems.Insert(col, new ListViewItem.ListViewSubItem(item, strText));
#endif
            item.SubItems[col].Text = strText;
        }

        // 2009/10/21
        // ���һ���е�ֵ�����Ѹ�����Ԫ��ֵ��\t�ַ���������
        public static string GetLineText(ListViewItem item)
        {
            string strResult = "";
            for (int i = 0; i < item.SubItems.Count; i++)
            {
                if (i > 0)
                    strResult += "\t";

                strResult += item.SubItems[i].Text;
            }

            return strResult;
        }

        // ���ȫ��ѡ��״̬
        public static void ClearSelection(ListView list)
        {
            list.SelectedItems.Clear();
        }

        // ���ȫ�� Checked ״̬
        public static void ClearChecked(ListView list)
        {
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.CheckedItems)
            {
                items.Add(item);
            }

            foreach (ListViewItem item in items)
            {
                item.Checked = false;
            }
        }

        // ѡ��һ��
        // parameters:
        //		nIndex	Ҫ����ѡ���ǵ��С����==-1����ʾ���ȫ��ѡ���ǵ���ѡ��
        //		bMoveFocus	�Ƿ�ͬʱ�ƶ�focus��־����ѡ����
        public static void SelectLine(ListView list,
            int nIndex,
            bool bMoveFocus)
        {
            list.SelectedItems.Clear();

            if (nIndex != -1)
            {
                list.Items[nIndex].Selected = true;

                if (bMoveFocus == true)
                {
                    list.Items[nIndex].Focused = true;
                }
            }
        }

        // ѡ��һ��
        // 2008/9/9
        // parameters:
        //		bMoveFocus	�Ƿ�ͬʱ�ƶ�focus��־����ѡ����
        public static void SelectLine(ListViewItem item,
            bool bMoveFocus)
        {
            Debug.Assert(item != null, "");

            item.ListView.SelectedItems.Clear();

            item.Selected = true;

            if (bMoveFocus == true)
            {
                item.Focused = true;
            }

        }

    }

    public class ListViewProperty
    {
        public string CurrentDbName = ""; // ��ǰ�Ѿ���ʾ�ı�������Ӧ�����ݿ�����Ϊ�˼ӿ��ٶ�

        public event GetColumnTitlesEventHandler GetColumnTitles = null;
        public event ParsePathEventHandler ParsePath = null;
        public event CompareEventHandler CompareColumn = null;

        // ����������к�����
        public SortColumns SortColumns = new SortColumns();

        public List<ColumnSortStyle> SortStyles = new List<ColumnSortStyle>();

        public Hashtable UsedColumnTitles = new Hashtable();   // keyΪ���ݿ�����valueΪList<string>

        public void ClearCache()
        {
            this.UsedColumnTitles.Clear();
            this.CurrentDbName = "";
        }

        public void OnCompareColumn(object sender, CompareEventArgs e)
        {
            if (this.CompareColumn != null)
                this.CompareColumn(sender, e);
        }

        public bool HasCompareColumnEvent()
        {
            if (this.CompareColumn != null)
                return true;
            return false;
        }

        // ���һ���п��õ�ȫ�� sort style
        public List<ColumnSortStyle> GetAllSortStyle(ListView list, int nColumn)
        {
            List<ColumnSortStyle> styles = new List<ColumnSortStyle>();
            styles.Add(ColumnSortStyle.None); // û��
            styles.Add(ColumnSortStyle.LeftAlign); // ������ַ���
            styles.Add(ColumnSortStyle.RightAlign);// �Ҷ����ַ���
            styles.Add(ColumnSortStyle.RecPath);    // ��¼·�������硰����ͼ��/1������'/'Ϊ�磬�ұ߲��ֵ�������ֵ���򡣻��ߡ�localhost/����ͼ��/ctlno/1��
            styles.Add(ColumnSortStyle.LongRecPath);  // ��¼·�������硰����ͼ��/1 @���ط�������

            // Ѱ�ұ��� .Tag �еĶ���
            if (nColumn < list.Columns.Count)
            {
                ColumnHeader header = list.Columns[nColumn];
                ColumnProperty prop = (ColumnProperty)header.Tag;
                if (prop != null)
                {
                    if (string.IsNullOrEmpty(prop.Type) == false)
                    {
                        ColumnSortStyle default_style = new ColumnSortStyle(prop.Type);
                        if (styles.IndexOf(default_style) == -1)
                            styles.Add(default_style);
                    }
                }
            }
            return styles;
        }


        public ColumnSortStyle GetSortStyle(ListView list, int nColumn)
        {
            ColumnSortStyle result = null;
            if (this.SortStyles.Count <= nColumn)
            {
            }
            else 
                result = SortStyles[nColumn];

            if (result == null || result == ColumnSortStyle.None)
            {
                // Ѱ�ұ��� .Tag �еĶ���
                if (nColumn < list.Columns.Count)
                {
                    ColumnHeader header = list.Columns[nColumn];
                    ColumnProperty prop = (ColumnProperty)header.Tag;
                    if (prop != null)
                    {
                        if (string.IsNullOrEmpty(prop.Type) == false)
                            return new ColumnSortStyle(prop.Type);
                    }
                }
            }
            return result;
        }

        public void SetSortStyle(int nColumn, ColumnSortStyle style)
        {
            // ȷ��Ԫ���㹻
            while (this.SortStyles.Count < nColumn + 1)
            {
                this.SortStyles.Add(null); // ���� .None // ȱʡ�� ColumnSortStyle.LeftAlign
            }

            this.SortStyles[nColumn] = style;

            // 2013/3/27
            // ˢ�� SortColumns
            foreach (Column column in this.SortColumns)
            {
                if (column.No == nColumn)
                    column.SortStyle = style;
            }
        }

        public ColumnPropertyCollection GetColumnName(string strDbName)
        {
            // �ȴ�Hashtable��Ѱ��
            if (this.UsedColumnTitles.Contains(strDbName) == true)
                return (ColumnPropertyCollection)this.UsedColumnTitles[strDbName];

            if (this.GetColumnTitles != null)
            {
                GetColumnTitlesEventArgs e = new GetColumnTitlesEventArgs();
                e.DbName = strDbName;
                e.ListViewProperty = this;
                this.GetColumnTitles(this, e);
                if (e.ColumnTitles != null)
                {
                    this.UsedColumnTitles[strDbName] = e.ColumnTitles;
                }
                return e.ColumnTitles;
            }

            return null;    // not found
        }

        public string ParseDbName(string strPath)
        {
            if (this.ParsePath != null)
            {
                ParsePathEventArgs e = new ParsePathEventArgs();
                e.Path = strPath;
                this.ParsePath(this, e);
                return e.DbName;
            }

            // ����� "����ͼ��/3" �򷵻����ݿ����������"����ͼ��/1@���ط�����"�򷵻�ȫ·��
            return GetDbName(strPath);
        }

        // ��·����ȡ����������
        // parammeters:
        //      strPath ·��������"����ͼ��/3"
        public static string GetDbName(string strPath)
        {
            // �����Ƿ��з����������� 2015/8/12
            int nRet = strPath.IndexOf("@");
            if (nRet != -1)
            {
                return strPath; // ����ȫ·��
            }

            nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }
#if NO
        // ��·����ȡ����������
        // parammeters:
        //      strPath ·��������"����ͼ��/3"
        public static string GetDbName(string strPath)
        {
            // �����Ƿ��з����������� 2015/8/11
            string strServerName = "";
            int nRet = strPath.IndexOf("@");
            if (nRet != -1)
            {
                strServerName = strPath.Substring(nRet).Trim(); // �����ַ� '@'
                strPath = strPath.Substring(0, nRet).Trim();
            }

            nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath + strServerName;

            return strPath.Substring(0, nRet).Trim() + strServerName;
        }
#endif
    }

    /// <summary>
    /// �����Ŀ����
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void GetColumnTitlesEventHandler(object sender,
    GetColumnTitlesEventArgs e);

    // 2013/3/31
    /// <summary>
    /// һ����Ŀ������
    /// </summary>
    public class ColumnProperty
    {
        /// <summary>
        /// ��Ŀ����
        /// </summary>
        public string Title = "";   // ��Ŀ����

        /// <summary>
        /// ��ֵ����
        /// </summary>
        public string Type = "";    // ��ֵ���͡�����ʱ����

        /// <summary>
        /// XPath
        /// </summary>
        public string XPath = "";   // XPath �ַ��� 2015/8/27

        /// <summary>
        /// �ַ���ת������
        /// </summary>
        public string Convert = ""; // �ַ���ת������ 2015/8/27

        public ColumnProperty(string strTitle, 
            string strType = "",
            string strXPath = "",
            string strConvert = "")
        {
            this.Title = strTitle;
            this.Type = strType;
            this.XPath = strXPath;
            this.Convert = strConvert;
        }
    }

    /// <summary>
    /// ��Ŀ���Լ���
    /// </summary>
    public class ColumnPropertyCollection : List<ColumnProperty>
    {
        /// <summary>
        /// ׷��һ����Ŀ���Զ���
        /// </summary>
        /// <param name="strTitle">����</param>
        /// <param name="strType">����</param>
        public void Add(string strTitle, 
            string strType = "", 
            string strXPath = "",
            string strConvert = "")
        {
            ColumnProperty prop = new ColumnProperty(strTitle, strType, strXPath, strConvert);
            base.Add(prop);
        }

        /// <summary>
        /// ����һ����Ŀ���Զ���
        /// </summary>
        /// <param name="nIndex">����λ���±�</param>
        /// <param name="strTitle">����</param>
        /// <param name="strType">����</param>
        public void Insert(int nIndex, string strTitle, string strType = "")
        {
            ColumnProperty prop = new ColumnProperty(strTitle, strType);
            base.Insert(nIndex, prop);
        }

        /// <summary>
        /// ���� type ֵ�����к�
        /// </summary>
        /// <returns>-1: û���ҵ�; ����: �к�</returns>
        public int FindColumnByType(string strType)
        {
            int index = 0;
            foreach (ColumnProperty col in this)
            {
                if (col.Type == strType)
                    return index;
                index ++;
            }
            return -1;
        }
    }

    /// <summary>
    /// �����Ŀ����Ĳ���
    /// </summary>
    public class GetColumnTitlesEventArgs : EventArgs
    {
        public string DbName = "";  // [in] ���ֵΪ"<blank>"����ʾ��һ��Ϊ�յ����������keys������
        public ListViewProperty ListViewProperty = null;    // [in][out]

        // public List<string> ColumnTitles = null;  // [out] null��ʾnot found����.Count == 0��ʾ��Ŀ����Ϊ�գ����Ҳ���not found
        public ColumnPropertyCollection ColumnTitles = null;  // [out] null��ʾnot found����.Count == 0��ʾ��Ŀ����Ϊ�գ����Ҳ���not found

        // public string ErrorInfo = "";    // [out]
    }

    /// <summary>
    /// ����·��
    /// </summary>
    /// <param name="sender">������</param>
    /// <param name="e">�¼�����</param>
    public delegate void ParsePathEventHandler(object sender,
    ParsePathEventArgs e);

    /// <summary>
    /// ����·���Ĳ���
    /// </summary>
    public class ParsePathEventArgs : EventArgs
    {
        public string Path = "";    // [in]
        public string DbName = "";    // [out]  // ���ݿ������֡����ܰ������������Ʋ���
    }
}
