using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace DigitalPlatform
{
    /*
     * 1) stop = new Stop() 
     * 2) ����load�׶� Register()
     * 3) ����closed�׶� Unregister()
     * 4) ÿ��ѭ����ʼ Initial() ����delegate
     *    ѭ����ʼʱ��BeginLoop() ѭ��������EndLoop() Initial()�����delegate�Ĺ���
     * 5) ��ѭ��ִ���У�����û�����stop button��delegate��Ȼ�ᱻ���á�����
     *    ��ѭ���������۲�stop��State״̬��Ҳ���Ե�֪��ť�Ƿ��Ѿ���������
     * 
     * 
     * 
     * 
     * 
     * 
     */

    // Stop�¼�
	public delegate void StopEventHandler(object sender,
	    StopEventArgs e);

    public class StopEventArgs : EventArgs
    {

    }

    // BeginLoop�¼�
    public delegate void BeginLoopEventHandler(object sender,
        BeginLoopEventArgs e);

    public class BeginLoopEventArgs : EventArgs
    {
        public bool IsActive = false;
    }

    // EndLoop�¼�
    public delegate void EndLoopEventHandler(object sender,
        EndLoopEventArgs e);

    public class EndLoopEventArgs : EventArgs
    {
        public bool IsActive = false;
    }

    // ����һ��Delegate_doStop()
    // public delegate void Delegate_doStop();

    //���Ӵ����ж���
    public class Stop
    {
        public long ProgressMin = -1;
        public long ProgressMax = -1;
        public long ProgressValue = -1;

        public StopStyle Style = StopStyle.None;
        public ReaderWriterLock m_stoplock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5��

        public event StopEventHandler OnStop = null;

        public event BeginLoopEventHandler OnBeginLoop = null;
        public event EndLoopEventHandler OnEndLoop = null;

        int nStop = -1;	// -1: ��δʹ�� 0:���ڴ��� 1:ϣ��ֹͣ 2:�Ѿ�ֹͣ��EndLoop()�Ѿ�����
        StopManager m_manager = null;

        string m_strMessage = "";

        // bool m_bCancel = false;

        public string Name = "";

        public object Tag = null;   // ��������������

        public Stop()
        {
        }

        // parameters:
        //      strMessage  Ҫ��ʾ����Ϣ�����Ϊ null����ʾ����ʾ��Ϣ
        public void Initial(// Delegate_doStop doStopDelegate,
            string strMessage)
        {
            // m_doStopDelegate = doStopDelegate;
            if (strMessage != null)
            {
                m_strMessage = strMessage;
                if (m_manager != null)
                {
                    // TODO: ִ�ж�θ��£����ܻٵ�ԭ���洢��״̬
                    m_manager.ChangeState(this,
                        StateParts.All,
                        true);
                }
            }
        }

        // ע�ᣬ�͹����������ϵ
        // parameters:
        //      bActive �Ƿ���Ҫ��������
        public void Register(StopManager manager,
            bool bActive)
        {
            m_manager = manager;
            manager.Add(this);

            if (bActive == true)
                manager.Active(this);
        }

        public void Unregister(bool bActive = true)
        {
            m_manager.Remove(this, false);
            m_manager = null;
        }

        //׼��������,��ѭ������ʱ���˵���Stopmanager��Enable()�������޸ĸ����ڵİ�ť״̬
        public void BeginLoop()
        {
            nStop = 0;	// ���ڴ���

            if (m_manager != null)
            {
                bool bIsActive = m_manager.IsActive(this);

                if (this.OnBeginLoop != null)
                {
                    BeginLoopEventArgs e = new BeginLoopEventArgs();
                    e.IsActive = bIsActive;
                    this.OnBeginLoop(this, e);
                }

                if (bIsActive == true)
                {
                    m_manager.ChangeState(this,
                        StateParts.All | StateParts.SaveEnabledState,
                        true);
                }
                else
                {
                    // ���ڼ���λ�õ�stop����Ҫ����ԭ�е�reversebutton״̬����Ϊ��������䵽���˵�״̬
                    m_manager.ChangeState(this,
                        StateParts.All ,
                        true);
                }
            }
        }

        public void SetMessage(string strMessage)
        {
            m_strMessage = strMessage;

            if (m_manager != null)
            {
                // TODO: ֻӦ���ı��ı���״̬����Ӧ������ť��״̬
                m_manager.ChangeState(this,
                    StateParts.Message,
                    true);
            }

        }

        public void SetProgressRange(long lStart, long lEnd)
        {
            this.ProgressMin = lStart;
            this.ProgressMax = lEnd;
            this.ProgressValue = lStart;

            if (m_manager != null)
            {
                m_manager.ChangeState(this,
                    StateParts.ProgressRange,
                    true);
            }
        }

        public void SetProgressValue(long lValue)
        {
            this.ProgressValue = lValue;

            if (m_manager != null)
            {
                m_manager.ChangeState(this,
                    StateParts.ProgressValue,
                    true);
            }

            // 2014/1/7
            if (lValue > this.ProgressMax)
            {
                this.ProgressMax = lValue;
                m_manager.ChangeState(this,
    StateParts.ProgressRange,
    true);
            }
        }

        public void HideProgress()
        {
            this.ProgressValue = -1;

            if (m_manager != null)
            {
                m_manager.ChangeState(this,
                    StateParts.ProgressValue,
                    true);
            }
        }

        //���������ˣ���ѭ�������������StopManager��Enable()�������޸İ�ťΪ����״̬
        public void EndLoop()
        {
            nStop = 2;	// �Ѿ�ֹͣ

            if (m_manager != null)
            {
                bool bIsActive = m_manager.IsActive(this);

                this.m_strMessage = "";

                if (this.OnEndLoop != null)
                {
                    EndLoopEventArgs e = new EndLoopEventArgs();
                    e.IsActive = bIsActive;
                    this.OnEndLoop(this, e);
                }

                if (bIsActive == true)
                {
                    m_manager.ChangeState(this,
                        StateParts.All | StateParts.RestoreEnabledState,
                        true);
                }
                else
                {
                    // ���ڼ���λ�ã���Ҫ�ָ���ν��״̬
                    m_manager.ChangeState(this,
                        StateParts.All,
                        true);
                }
            }
        }

        //�鿴�Ƿ����,��StopManager��
        public virtual int State
        {
            get
            {
                return nStop;
            }
        }

        public virtual void Continue()
        {
            nStop = 0;
        }

        public virtual void SetState(int nState)
        {
            this.nStop = nState;
        }

        // ֹͣ,��StopManager��
        // locks: д����
        // parameters:
        //      bHalfStop   �Ƿ�Ϊһ���жϡ���νһ���жϣ����ǲ�����Stop�¼�����ֻ�޸�Stop״̬��
        public void DoStop(object sender = null)
        {
            this.m_stoplock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                bool bHalfStop = false;

                if ((this.Style & StopStyle.EnableHalfStop) != 0
                    && this.nStop == 0)
                {
                    bHalfStop = true;
                }

                if (bHalfStop == false)
                {
                    if (this.OnStop != null)
                    {
                        // OnStop()�����Ѿ�����������µ��õ�
                        StopEventArgs e = new StopEventArgs();
                        this.OnStop(sender == null ? this : sender, e);
                    }
                }

                nStop = 1;
            }
            finally
            {
                this.m_stoplock.ReleaseWriterLock();
            }
        }

        public string Message
        {
            get
            {
                return m_strMessage;
            }
        }


    }

    //�ڸ������ж���,��ʼ����ť
    public class StopManager
    {
        public event DisplayMessageEventHandler OnDisplayMessage = null;
        public event AskReverseButtonStateEventHandle OnAskReverseButtonState;

        public bool bDebug = false;
        public string DebugFileName = "";

        public ReaderWriterLock m_collectionlock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5��

        // ToolBarButton m_stopToolButton = null;	// ����ֹͣ�Ĺ�������ť
        // Button	m_stopButton = null;	// ����ֹͣ�İ�ť
        object m_stopButton = null;	// ����ֹͣ�Ĺ�������ť ������ToolBarButton ToolStripButton Button


        /*
		StatusBar m_messageStatusBar = null;	// ״̬��
		Label m_messageLabel = null;	// ��ʾ״̬��Label
        */
        object m_messageBar = null;  // StatusBar StatusStrip Label TextBox

        object m_progressBar = null;    // ProgressBar

        double progress_ratio = 1.0;

        public List<Stop> stops = new List<Stop>();
        bool bMultiple = false;

        // string m_strTipsSave = "";

        List<object> m_reverseButtons = null;	// ��ֹͣ��ťEnabled״̬�仯ʱ����Ҫ����״̬�෴�İ�ť��������
        List<int> m_reverseButtonEnableStates = null;   // 0: diabled; 1: eanbled; -1: unknown

        void WriteDebugInfo(string strText)
        {
            if (bDebug == false)
                return;

            WriteText(this.DebugFileName, strText);
        }

        // д���ı��ļ���
        // ����ļ�������, ���Զ��������ļ�
        // ����ļ��Ѿ����ڣ���׷����β����
        public static void WriteText(string strFileName,
            string strText)
        {
            StreamWriter sw = new StreamWriter(strFileName,
                true,	// append
                System.Text.Encoding.UTF8);
            sw.Write(strText);
            sw.Close();
        }

        // 2007/8/2
        public void LinkReverseButtons(List<object> buttons)
        {
            // ���
            for (int i = 0; i < buttons.Count; i++)
            {
                object button = buttons[i];

                if ((button is ToolBarButton)
                    || (button is Button)
                    || (button is ToolStripButton))
                {
                }
                else
                {
                    throw new Exception("button����ֻ����ToolBarButton ToolStripButton��Button����");
                }
            }

            if (m_reverseButtons == null
                || m_reverseButtons.Count == 0)
                m_reverseButtons = buttons;
            else
            {
                m_reverseButtons.AddRange(buttons);
            }
        }

        // 2007/8/2
        public void UnlinkReverseButtons(List<object> buttons)
        {
            // ���
            for (int i = 0; i < buttons.Count; i++)
            {
                object button = buttons[i];

                if ((button is ToolBarButton)
                    || (button is Button)
                    || (button is ToolStripButton))
                {
                }
                else
                {
                    throw new Exception("button����ֻ����ToolBarButton ToolStripButton��Button����");
                }
            }

            if (m_reverseButtons == null 
                || m_reverseButtons.Count == 0)
                return;

            for (int i = 0; i < buttons.Count; i++)
            {
                object button = buttons[i];

                m_reverseButtons.Remove(button);
            }
        }


        public void LinkReverseButton(object button)
        {
            if ((button is ToolBarButton)
                || (button is Button)
                || (button is ToolStripButton))
            {
            }
            else
            {
                throw new Exception("button����ֻ����ToolBarButton ToolStripButton��Button����");
            }

            if (m_reverseButtons == null)
                m_reverseButtons = new List<object>();

            m_reverseButtons.Add(button);
        }

        public void UnlinkReverseButton(object button)
        {
            if ((button is ToolBarButton)
        || (button is Button)
        || (button is ToolStripButton))
            {
            }
            else
            {
                throw new Exception("button����ֻ����ToolBarButton ToolStripButton��Button����");
            }

            if (m_reverseButtons == null)
                return;

            m_reverseButtons.Remove(button);
        }

        void InternalSetMessage(string strMessage)
        {
            if (m_messageBar is StatusStrip)
            {
                // ((StatusStrip)m_messageBar).Text = strMessage;
                Safe_SetStatusStripText(((StatusStrip)m_messageBar), strMessage);
            }
            else if (m_messageBar is StatusBar)
            {
                StatusBar statusbar = ((StatusBar)m_messageBar);

                Safe_SetStatusBarText(statusbar, strMessage);
            }
            else if (m_messageBar is Label)
            {
                // ((Label)m_messageBar).Text = strMessage;
                Safe_SetLabelText(((Label)m_messageBar), strMessage);
            }
            else if (m_messageBar is TextBox)
            {
                // ((TextBox)m_messageBar).Text = strMessage;
                Safe_SetTextBoxText(((TextBox)m_messageBar), strMessage);
            }
            else if (m_messageBar is ToolStripStatusLabel)
            {
                // ((ToolStripStatusLabel)m_messageBar).Text = strMessage;
                Safe_SetToolStripStatusLabelText((ToolStripStatusLabel)m_messageBar,
                    strMessage);
            }

            if (this.OnDisplayMessage != null)
            {
                DisplayMessageEventArgs e = new DisplayMessageEventArgs();
                e.Message = strMessage;
                this.OnDisplayMessage(this, e);
            }
        }

        void InternalSetProgressBar(long lStart, long lEnd, long lValue)
        {
            if (m_progressBar is ProgressBar)
            {
                ProgressBar progressbar = ((ProgressBar)m_progressBar);

                Safe_SetProgressBar(progressbar,
                    lStart, lEnd, lValue);
            }

            if (m_progressBar is ToolStripProgressBar)
            {
                ToolStripProgressBar progressbar = ((ToolStripProgressBar)m_progressBar);

                Safe_SetProgressBar(progressbar,
                    lStart, lEnd, lValue);
            }
        }

        #region StatusStrip

        // �̰߳�ȫ�汾
        string Safe_SetStatusStripText(StatusStrip status_strip,
            string strText)
        {
            if (status_strip.Parent != null && status_strip.Parent.InvokeRequired)
            {
                Delegate_SetStatusStripText d = new Delegate_SetStatusStripText(SetStatusStripText);
                return (string)status_strip.Parent.Invoke(d, new object[] { status_strip, strText });
            }
            else
            {
                string strOldText = status_strip.Text;

                status_strip.Text = strText;

                status_strip.Update();

                return strOldText;
            }
        }

        delegate string Delegate_SetStatusStripText(StatusStrip status_strip,
            string strText);

        string SetStatusStripText(StatusStrip status_strip,
            string strText)
        {
            string strOldText = status_strip.Text;

            status_strip.Text = strText;

            status_strip.Update();

            return strOldText;
        }

        #endregion

        #region ToolStripStatusLabel

        // �̰߳�ȫ�汾
        string Safe_SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText)
        {
            if (label.Owner == null)
                return "";

            if (label.Owner.InvokeRequired)
            {
                Delegate_SetToolStripStatusLabelText d = new Delegate_SetToolStripStatusLabelText(SetToolStripStatusLabelText);
                return (string)label.Owner.Invoke(d, new object[] { label, strText });
            }
            else
            {
                string strOldText = label.Text;

                label.Text = strText;

                // label.Owner.Update();    // �Ż�

                return strOldText;
            }
        }

        delegate string Delegate_SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText);

        string SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText)
        {
            string strOldText = label.Text;

            label.Text = strText;

            label.Owner.Update();

            return strOldText;
        }

        #endregion

        #region TextBox

        // �̰߳�ȫ�汾
        string Safe_SetTextBoxText(TextBox textbox,
            string strText)
        {
            if (textbox.Parent != null && textbox.Parent.InvokeRequired)
            {
                Delegate_SetTextBoxText d = new Delegate_SetTextBoxText(SetTextBoxText);
                return (string)textbox.Parent.Invoke(d, new object[] { textbox, strText });
            }
            else
            {
                string strOldText = textbox.Text;

                textbox.Text = strText;
                textbox.Update();


                return strOldText;
            }
        }

        delegate string Delegate_SetTextBoxText(TextBox textbox,
            string strText);

        string SetTextBoxText(TextBox textbox,
            string strText)
        {
            string strOldText = textbox.Text;

            textbox.Text = strText;
            textbox.Update();

            return strOldText;
        }

        #endregion

        #region Label

        // �̰߳�ȫ�汾
        string Safe_SetLabelText(Label label,
            string strText)
        {
            if (label.Parent != null && label.Parent.InvokeRequired)
            {
                Delegate_SetLabelText d = new Delegate_SetLabelText(SetLabelText);
                return (string)label.Parent.Invoke(d, new object[] { label, strText });
            }
            else
            {
                string strOldText = label.Text;

                label.Text = strText;
                label.Update();

                return strOldText;
            }
        }

        delegate string Delegate_SetLabelText(Label label,
            string strText);

        string SetLabelText(Label label,
            string strText)
        {
            string strOldText = label.Text;

            label.Text = strText;
            label.Update();

            return strOldText;
        }

        #endregion

        #region ProgressBar

        // �̰߳�ȫ�汾
        void Safe_SetProgressBar(ProgressBar progressbar,
            long lStart,
            long lEnd,
            long lValue)
        {
            if (progressbar.Parent != null && progressbar.Parent.InvokeRequired)
            {
                Delegate_SetProgressBar d = new Delegate_SetProgressBar(SetProgressBar);
                progressbar.Parent.Invoke(d, new object[] { progressbar, lStart, lEnd, lValue });
            }
            else
            {
                if (lEnd == -1 && lStart == -1 && lValue == -1)
                {
                    if (progressbar.Visible != false)   // 2008/3/17
                        progressbar.Visible = false;
                    if (lValue == -1)
                        return;
                }
                else
                {
                    if (progressbar.Visible == false)
                        progressbar.Visible = true;
                }

                if (lEnd >= 0)
                {
                    SetRatio(lEnd);
                    progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
                }
                if (lStart >= 0)
                    progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

                if (lValue >=0 )
                    progressbar.Value = (int)(this.progress_ratio * (double)lValue); 
            }
        }

        void SetRatio(long lEnd)
        {
            this.progress_ratio =  (double)64000 / (double)lEnd;
            if (this.progress_ratio > 1.0)
                this.progress_ratio = 1.0;
        }

        delegate void Delegate_SetProgressBar(ProgressBar progressbar,
            long lStart, long lEnd, long lValue);

        void SetProgressBar(ProgressBar progressbar,
            long lStart, long lEnd, long lValue)
        {
            if (lEnd == -1 && lStart == -1 && lValue == -1)
            {
                if (progressbar.Visible != false)   // 2008/3/17
                    progressbar.Visible = false;
                if (lValue == -1)
                    return;
            }
            else
            {
                if (progressbar.Visible == false)
                    progressbar.Visible = true;
            }

            if (lEnd >= 0)
            {
                SetRatio(lEnd);
                progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
            }
            if (lStart >= 0)
                progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

            if (lValue >= 0)
                progressbar.Value = (int)(this.progress_ratio * (double)lValue);
        }


        #endregion

        #region ToolStripProgressBar

        // �̰߳�ȫ�汾
        void Safe_SetProgressBar(ToolStripProgressBar progressbar,
            long lStart,
            long lEnd,
            long lValue)
        {
            if (progressbar.Owner == null)
                return;

            if (progressbar.Owner.InvokeRequired)
            {
                Delegate_SetToolStrupProgressBar d = new Delegate_SetToolStrupProgressBar(SetProgressBar);
                progressbar.Owner.Invoke(d, new object[] { progressbar, lStart, lEnd, lValue });
            }
            else
            {
                if (lEnd == -1 && lStart == -1 && lValue == -1)
                {
                    if (progressbar.Visible != false)   // 2008/3/17
                        progressbar.Visible = false;

                    if (lValue == -1)
                        return;
                }
                else
                {
                    if (progressbar.Visible == false)
                        progressbar.Visible = true;
                }

                if (lEnd >= 0)
                {
                    SetRatio(lEnd);
                    progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
                }
                if (lStart >= 0)
                    progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

                if (lValue >= 0)
                    progressbar.Value = (int)(this.progress_ratio * (double)lValue);
            }
        }

        delegate void Delegate_SetToolStrupProgressBar(ToolStripProgressBar progressbar,
            long lStart, long lEnd, long lValue);

        void SetProgressBar(ToolStripProgressBar progressbar,
            long lStart, long lEnd, long lValue)
        {
            if (lEnd == -1 && lStart == -1 && lValue == -1)
            {
                if (progressbar.Visible != false)   // 2008/3/17
                    progressbar.Visible = false;
                if (lValue == -1)
                    return;
            }
            else
            {
                if (progressbar.Visible == false)
                    progressbar.Visible = true;
            }

            if (lEnd >= 0)
            {
                SetRatio(lEnd);
                progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
            }
            if (lStart >= 0)
                progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

            if (lValue >= 0)
                progressbar.Value = (int)(this.progress_ratio * (double)lValue);
        }


        #endregion

        #region StatusBar

        // �̰߳�ȫ�汾
        string Safe_SetStatusBarText(StatusBar statusbar,
            string strText)
        {
            if (statusbar.Parent != null && statusbar.Parent.InvokeRequired)
            {
                Delegate_SetStatusBarText d = new Delegate_SetStatusBarText(SetStatusBarText);
                return (string)statusbar.Parent.Invoke(d, new object[] { statusbar, strText });
            }
            else
            {
                string strOldText = statusbar.Text;

                statusbar.Text = strText;
                statusbar.Update();

                return strOldText;
            }
        }

        delegate string Delegate_SetStatusBarText(StatusBar statusbar,
            string strText);

        string SetStatusBarText(StatusBar statusbar,
            string strText)
        {
            string strOldText = statusbar.Text;

            statusbar.Text = strText;
            statusbar.Update();

            return strOldText;
        }

        #endregion

        // �ı�stop��ťEnabled״̬����������ǰ��״̬
        void EnableStopButtons(bool bEnabled)
        {
            if (this.m_stopButton == null)
                return;

            if (this.m_stopButton is Button)
            {
                // ((Button)this.m_stopButton).Enabled = bEnabled;

                Button button = ((Button)this.m_stopButton);

                Safe_EnableButton(button, bEnabled);

            }
            else if (this.m_stopButton is ToolBarButton)
            {
                // ((ToolBarButton)this.m_stopButton).Enabled = bEnabled;

                ToolBarButton button = ((ToolBarButton)this.m_stopButton);

                Safe_EnableToolBarButton(button, bEnabled);
                /*
                if (button.Parent.InvokeRequired)
                {
                    Delegate_SetToolBarButtonEnable d = new Delegate_SetToolBarButtonEnable(SetToolBarButtonEnable);
                    button.Parent.Invoke(d, new object[] { button, bEnabled });
                }
                else
                {
                    button.Enabled = bEnabled;
                }
                 * */


            }
            else if (this.m_stopButton is ToolStripButton)
            {
                // ((ToolStripButton)this.m_stopButton).Enabled = bEnabled;

                ToolStripButton button = (ToolStripButton)this.m_stopButton;

                Safe_EnableToolStripButton(button, bEnabled);
                /*
                if (button.Owner.InvokeRequired)
                {
                    Delegate_SetToolStripButtonEnable d = new Delegate_SetToolStripButtonEnable(SetToolStripButtonEnable);
                    button.Owner.Invoke(d, new object[] { button, bEnabled });
                }
                else
                {
                    button.Enabled = bEnabled;
                }
                 * */
            }

        }

        #region Button

        // �̰߳�ȫ����
        static bool Safe_EnableButton(Button button,
            bool bEnabled)
        {
            if (button.Parent != null && button.Parent.InvokeRequired)
            {
                Delegate_SetButtonEnable d = new Delegate_SetButtonEnable(SetButtonEnable);
                return (bool)button.Parent.Invoke(d, new object[] { button, bEnabled });
            }
            else
            {
                bool bOldState = button.Enabled;

                button.Enabled = bEnabled;

                return bOldState;
            }
        }

        delegate bool Delegate_SetButtonEnable(Button button,
            bool bEnabled);

        static bool SetButtonEnable(Button button,
            bool bEnabled)
        {
            bool bOldState = button.Enabled;

            button.Enabled = bEnabled;

            button.Parent.Update();

            return bOldState;
        }

        #endregion

        #region ToolBarButton

        // �̰߳�ȫ����
        static bool Safe_EnableToolBarButton(ToolBarButton button,
            bool bEnabled)
        {
            if (button.Parent != null && button.Parent.InvokeRequired)
            {
                Delegate_SetToolBarButtonEnable d = new Delegate_SetToolBarButtonEnable(SetToolBarButtonEnable);
                return (bool)button.Parent.Invoke(d, new object[] { button, bEnabled });
            }
            else
            {
                bool bOldState = button.Enabled;

                button.Enabled = bEnabled;

                return bOldState;
            }
        }

        delegate bool Delegate_SetToolBarButtonEnable(ToolBarButton button,
            bool bEnabled);

        static bool SetToolBarButtonEnable(ToolBarButton button,
            bool bEnabled)
        {
            bool bOldState = button.Enabled;

            button.Enabled = bEnabled;

            button.Parent.Update();

            return bOldState;
        }

        #endregion

        #region ToolStripButton

        // �̰߳�ȫ����
        static bool Safe_EnableToolStripButton(ToolStripButton button,
            bool bEnabled)
        {
            // 2014/12/26
            if (button.Owner == null)
                return false;

            if (button.Owner.InvokeRequired)
            {
                Delegate_SetToolStripButtonEnable d = new Delegate_SetToolStripButtonEnable(SetToolStripButtonEnable);
                return (bool)button.Owner.Invoke(d, new object[] { button, bEnabled });
            }
            else
            {
                bool bOldState = button.Enabled;
                button.Enabled = bEnabled;
                return bOldState;
            }
        }

        delegate bool Delegate_SetToolStripButtonEnable(ToolStripButton button,
            bool bEnabled);

        static bool SetToolStripButtonEnable(ToolStripButton button,
            bool bEnabled)
        {
            bool bOldState = button.Enabled;

            button.Enabled = bEnabled;

            button.Owner.Update();

            return bOldState;
        }

        #endregion

        // ����ı�reverse_buttons��Enabled״̬��
        // ע�⣬��falseʱ����Ҫ�ı�Ϊdisabled״̬����trueʱ������Ҫ�ָ�ԭ������(disableǰ)��״̬
        // parameters:
        //      bEnabled    true��ʾϣ���ָ���ťԭ��״̬��falseϣ��disable��ť
        //      parts   SaveEnabledStateϣ���ȱ��水ťԭ����ֵ��RestoreEnabledState��ʾҪ�ָ���ťԭ����ֵ
        void EnableReverseButtons(bool bEnabled,
            StateParts parts)
        {
            if (m_reverseButtons == null)
                return;

            if (m_reverseButtonEnableStates == null)
                m_reverseButtonEnableStates = new List<int>();

            /*
            if ((parts & StateParts.All) != 0)
                throw new Exception("StatePartsö���г���SaveEnabledState��RestoreEnabledStateֵ�⣬����ֵ���ڱ�����û������");
             * */

            bool bSave = false;
            if ((parts & StateParts.SaveEnabledState) != 0)
                bSave = true;

            bool bRestore = false;
            if ((parts & StateParts.RestoreEnabledState) != 0)
                bRestore = true;


            // ��֤��������Ĵ�Сһ��
            while (m_reverseButtonEnableStates.Count < m_reverseButtons.Count)
            {
                m_reverseButtonEnableStates.Add(-1);
            }

            for (int i = 0; i < m_reverseButtons.Count; i++)
            {
                int nOldState = m_reverseButtonEnableStates[i];

                bool bEnableResult = bEnabled;
                // TODO: ״̬������ʱ�򣬿���ͨ���¼����ⲿ��ȡ��Ϣ
                // ���bEnabledҪ��true������״̬�������Ǿ�ѯ��
                if (nOldState == -1 && bEnabled == true)
                {
                    if (this.OnAskReverseButtonState != null)
                    {
                        AskReverseButtonStateEventArgs e = new AskReverseButtonStateEventArgs();
                        e.EnableQuestion = true;
                        e.Button = m_reverseButtons[i];
                        this.OnAskReverseButtonState(this, e);
                        bEnableResult = e.EnableResult;
                    }
                }


                bool bOldState = (nOldState == 1 ? true : false);

                object button = m_reverseButtons[i];
                if (button is Button)
                {
                    bOldState = ((Button)button).Enabled;
                    // ((Button)button).Enabled = (bRestore == true ? bOldState : bEnabled);
                    Safe_EnableButton((Button)button,
                        (bRestore == true ? bOldState : bEnableResult));

                }
                else if (button is ToolBarButton)
                {
                    // ((ToolBarButton)button).Enabled = bEnabled;

                    bOldState = Safe_EnableToolBarButton(((ToolBarButton)button),
                        bRestore == true ? bOldState : bEnableResult);
                    /*
                    if (button.Parent.InvokeRequired)
                    {
                        Delegate_SetToolBarButtonEnable d = new Delegate_SetToolBarButtonEnable(SetToolBarButtonEnable);
                        button.Parent.Invoke(d, new object[] { button, bEnabled });
                    }
                    else
                    {
                        button.Enabled = bEnabled;
                    }
                     * */
                }
                else if (button is ToolStripButton)
                {
                    // ((ToolStripButton)button).Enabled = bEnabled;

                    bOldState = Safe_EnableToolStripButton(((ToolStripButton)button),
                        bRestore == true ? bOldState : bEnableResult);
                }

                // ����disableǰ��������ǰ��״̬
                if (bEnabled == false
                    && bSave == true)
                    m_reverseButtonEnableStates[i] = (bOldState == true ? 1 : 0);

            }
        }


        // ��ʼ��һ����ť,�ڸ�����loadʱ��
        // locks: ����д��
        public void Initial(object button,
            object statusBar,
            object progressBar)
        {

            WriteDebugInfo("collection write lock 1\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {

                m_stopButton = button;

                /*
				if (m_stopButton != null)
					m_strTipsSave = m_stopToolButton.ToolTipText;
				else
					m_strTipsSave = "";
                 */

                m_messageBar = statusBar;
                m_progressBar = progressBar;
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 1\r\n");
            }

        }


        // ����һ��Stop���󡣼����ڷǻλ��
        // locks: ����д��
        public void Add(Stop stop)
        {
            WriteDebugInfo("collection write lock 2\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                // stops.Add(stop);
                stops.Insert(0, stop);  // �޸ĺ��Ч�����Ͳ���ı伤���stop������
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 2\r\n");
            }

            //SetToolTipText();
        }

        /*
		void SetToolTipText()
		{
			if (m_stopToolButton != null) 
			{
				string strText = DebugInfo();
				if (strText != "0")
					m_stopToolButton.ToolTipText = m_strTipsSave + "\r\n[" + DebugInfo() + "]";
				else
					m_stopToolButton.ToolTipText = m_strTipsSave;
			}
		}
         */


        string DebugInfo()
        {
            /*
            string strText = "";

            this.m_lock.AcquireReaderLock(Stop.m_nLockTimeout);
            try 
            {
                for(int i=0;i<stops.Count;i++) 
                {
                    Stop temp = (Stop)stops[i];
                    strText += Convert.ToString(i) + "-" 
                        + temp.Name + "-["
                        + Convert.ToString(temp.State())
                        +"]\r\n";
                }
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

            return strText;
            */
            return Convert.ToString(stops.Count);
        }

        // ����һ��Stop����
        // locks: ����д��
        public void Remove(Stop stop, bool bChangeState = true)
        {
            WriteDebugInfo("collection write lock 3\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                stops.Remove(stop);

                if (bChangeState == true)
                ChangeState(null,
                    StateParts.All,
                    false); // false��ʾ���Ӽ�����
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 3\r\n");

            }

            //SetToolTipText();

        }

        // ���ť
        // ��ΪStopManager��Ͻ�ܶ�Stop����, ��һ��Stop������Ҫ���ڽ���״̬,
        // ��Ҫ��StopManager�ڲ������а����Stop����Ų������β��, Ҳ�����൱�ڶ�ջ����
        // ��Ч����Ȼ��, ��StopManager��������button����Ϊ��Stop����ǰ״̬��Ӧ��
        // Enabled����Disabled״̬, ��Ϊֻ�������û����ܴ�����ť��
        // StopManager�����˺ܶ�Stop״̬��Active()�����൱�ڰ�ĳ��Stop״̬�����ɼ��Ķ�����
        // locks: ����д��
        public bool Active(Stop stop)
        {
            if (stop == null)
            {
                // 2007/8/1
                EnableStopButtons(false);
                EnableReverseButtons(true, StateParts.None);
                InternalSetMessage("");
                InternalSetProgressBar(-1, -1, -1);

                return false;
            }

            bool bFound = false;

            WriteDebugInfo("collection write lock 4\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {

                for (int i = 0; i < stops.Count; i++)
                {
                    if (stops[i] == stop)
                    {
                        bFound = true;
                        stops.RemoveAt(i);
                        stops.Add(stop);
                        break;
                    }

                }
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 4\r\n");

            }


            if (bFound == false)
            {
                // 2007/8/1
                EnableStopButtons(false);
                EnableReverseButtons(true,
                    StateParts.None);
                InternalSetMessage("");
                InternalSetProgressBar(-1, -1, -1);

                return false;
            }

            // Debug.Assert(stop.State != -1, "");

            //if (m_stopToolButton != null) //??
            //{
            if (stop.State == 0)
            {
                EnableStopButtons(true);
                EnableReverseButtons(false,
                    StateParts.None);
            }
            else
            {
                EnableStopButtons(false);
                EnableReverseButtons(true,
                    StateParts.None);
            }
            //}

            // ����ҲҪ�仯
            //if (m_messageStatusBar != null) 
            //{
            InternalSetMessage(stop.Message);
            //}

            InternalSetProgressBar(stop.ProgressMin, stop.ProgressMax, stop.ProgressValue);


            //SetToolTipText();


            return true;
        }

        // �Ƿ��ڵ�ǰ����λ��?
        public bool IsActive(Stop stop)
        {
            int index = this.stops.IndexOf(stop);

            if (index == -1)
                return false;

            if (index == this.stops.Count - 1)
                return true;

            return false;
        }

        // ��ǰ�����˵�Stop����
        public Stop ActiveStop
        {
            get
            {
                if (stops.Count > 0)
                {
                    return (Stop)stops[stops.Count - 1];
                }

                return null;
            }
        }

        // ֹͣ��ǰ�����һ��Stop���󡣱�����ͨ��������ֹͣ��ť��
        public void DoStopActive()
        {
            if (stops.Count > 0)
            {
                Stop temp = (Stop)stops[stops.Count - 1];
                temp.DoStop();
            }

            //SetToolTipText();

        }

#if NOOOOOOOOOOO
        // һ��ֹͣ��ǰ�����һ��Stop���󡣱�����ͨ��������ֹͣ��ť��
        public void DoHalfStopActive()
        {
            if (stops.Count > 0)
            {
                Stop temp = (Stop)stops[stops.Count - 1];
                temp.DoStop(true);
            }
        }
#endif

        // ֹͣ����Stop���󣬵��ǲ�ֹͣ��ǰ������Ǹ�Stop��ť��
        // locks: ����д��
        public void DoStopAllButActive()
        {
            WriteDebugInfo("collection write lock 5\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                for (int i = 0; i < stops.Count - 1; i++)
                {
                    Stop temp = (Stop)stops[i];
                    temp.DoStop();
                }
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 5\r\n");
            }

            //SetToolTipText();
        }

        // ֹͣ����Stop���󣬰�����ǰ�����Stop��������ָstopExclude����==null
        // ��stopExclude�������Ըı亯����Ϊ����ֹͣĳ��ָ���Ķ���
        // locks: ����д��
        public void DoStopAll(Stop stopExclude)
        {
            WriteDebugInfo("collection write lock 6\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                for (int i = 0; i < stops.Count; i++)
                {
                    Stop temp = (Stop)stops[i];
                    if (stopExclude != temp)
                        temp.DoStop();
                }
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 6\r\n");

            }

            //SetToolTipText();
        }

        // ��״̬, ��Stop��
        // locks: ���϶���(���bLock == true)
        public void ChangeState(Stop stop,
            StateParts parts,
            bool bLock)
        {
            if (stops.Count == 0)
                return;

            // TODO: ����������߼�������: �����Ӿ��ϲ����֣��ڴ�ҲӦ�������޸ģ��Ա������л�ʱ��ʾ��������̬����ô��ʱ�����ǿ��Եģ����ǹؼ�״̬�仯��������ʾ�����صȶ�����һ��Ҫ���ֵ��ڴ�
            if (stop != null && stop.ProgressValue == -1)
            {
                if ((parts & StateParts.ProgressValue) != 0)
                {
                    // ���Ϊ����progress����ͼ��
                    // ��max��min������Ϊ-1���Ա㽫��ˢ�µ�ʱ������������
                    // 2011/10/12
                    stop.ProgressMax = -1;
                    stop.ProgressMin = -1;
                }
            }

            if (bLock == true)
            {
                WriteDebugInfo("collection read lock 7\r\n");
                this.m_collectionlock.AcquireReaderLock(Stop.m_nLockTimeout);
            }
            bool bLoop = false;
            Stop active = null;

            try
            {

                if (bMultiple == false)
                {
                    if (stop == null)
                    {
                        stop = (Stop)stops[stops.Count - 1];
                    }
                    else
                    {
                        if (stop != stops[stops.Count - 1])
                            return;
                    }
                    bLoop = stop.State == 0 ? true : false;
                    active = stop;
                }
                else
                {
                    bool bFound = false;
                    for (int i = 0; i < stops.Count; i++)
                    {
                        Stop temp = (Stop)stops[i];
                        if (bLoop == false)
                        {
                            if (temp.State == 0)
                                bLoop = true;
                        }

                        if (stop == temp)
                        {
                            bFound = true;
                            active = temp;
                            break;
                        }
                    }
                    if (bFound == false)
                        return;

                }
                // ����


            }
            finally
            {
                if (bLock == true)
                {
                    this.m_collectionlock.ReleaseReaderLock();
                    WriteDebugInfo("collection read unlock 7\r\n");
                }
            }

            //if (m_stopToolButton != null) //???
            //{
            if ((parts & StateParts.StopButton) != 0)
                EnableStopButtons(bLoop);

            if ((parts & StateParts.ReverseButtons) != 0)
                EnableReverseButtons(!bLoop,
                    parts);
            //}

            //if (m_messageStatusBar != null) 
            //{
            if ((parts & StateParts.Message) != 0)
                InternalSetMessage(active.Message);
            //}

            if ((parts & StateParts.ProgressRange) != 0)
            {
                // active.ProgressValue = 0;   // ��ʼ��
                active.ProgressValue = active.ProgressMin;   // ��ʼ�� 2008/5/16 changed
                InternalSetProgressBar(active.ProgressMin, active.ProgressMax, -1);
            }

            if ((parts & StateParts.ProgressValue) != 0)
            {
                /*
                if (active.ProgressValue == -1)
                {
                    // ���Ϊ����progress����ͼ��
                    // ��max��min������Ϊ-1���Ա㽫��ˢ�µ�ʱ������������
                    // 2008/3/10
                    active.ProgressMax = -1;
                    active.ProgressMin = -1;
                }
                 * */

                InternalSetProgressBar(-1, -1, active.ProgressValue);
            }

        }

    }

    // ��Щ����
    [Flags]
    public enum StateParts
    {
        None = 0x00,
        All = 0xff, // ȫ�� (����StoreEnabledState����)
        StopButton = 0x01,  // Stop��ť
        ReverseButtons = 0x02,  // Stop�����������ť
        Message = 0x10, // ��Ϣ�ı�
        ProgressRange = 0x20,   // ��������Χ
        ProgressValue = 0x40,   // ������ֵ

        SaveEnabledState = 0x0100,   // �洢Enabled״̬
        RestoreEnabledState = 0x0200,   // �ָ���ǰ�洢Enabled״̬
    }

    // �����˵�
    public delegate void AskReverseButtonStateEventHandle(object sender,
    AskReverseButtonStateEventArgs e);

    public class AskReverseButtonStateEventArgs : EventArgs
    {
        public object Button = null;
        public bool EnableQuestion = true; // ���⣺ϣ��Enabled? == trueָϣ��ʹ�ܡ�һ�㲻��ѯ��ʹ���ܣ���Ϊ�������ײ�������о���
        public bool EnableResult = true;   // �𰸣��Ƿ�ͬ��Enabled.
    }

    [Flags]
    public enum StopStyle
    {
        None = 0x00,
        EnableHalfStop = 0x01,  // �����һ�Ρ����жϡ�
    }

    public delegate void DisplayMessageEventHandler(object sender,
DisplayMessageEventArgs e);

    public class DisplayMessageEventArgs : EventArgs
    {
        public string Message = "";
    }
}
