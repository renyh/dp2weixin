using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// DateTime������չ����
    /// </summary>
    public class DateTimeUtil
    {

        public static string GetDisplayTimePeriodString(string strText)
        {
            strText = strText.Replace("day", "��");

            return strText.Replace("hour", "Сʱ");
        }

        public static string ToLocalTime(string strRfc1123, string strFormat)
        {
            try
            {
                return Rfc1123DateTimeStringToLocal(strRfc1123, strFormat);
            }
            catch (Exception ex)
            {
                return "ʱ���ַ��� '" + strRfc1123 + "' ��ʽ����ȷ: " + ex.Message;
            }
        }
        public static string ToDateString(DateTime day)
        {
            return day.Year.ToString() + "/" + day.Month.ToString() + "/" + day.Day.ToString();
        }

        public static string ToMonthString(DateTime day)
        {
            return day.Year.ToString() + "/" + day.Month.ToString();
        }

        public static string ToYearString(DateTime day)
        {
            return day.Year.ToString();
        }


        // �������ַ�������Ϊ��ֹ��Χ����
        // throw:
        //      Exception
        public static void ParseDateRange(string strText,
            out string strStartDate,
            out string strEndDate)
        {
            strStartDate = "";
            strEndDate = "";

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                // û��'-'

                if (strText.Length == 4)
                {
                    strStartDate = strText + "0101";
                    strEndDate = strText + "1231";
                    return;
                }

                if (strText.Length == 6)
                {
                    strStartDate = strText + "01";
                    DateTime start = DateTimeUtil.Long8ToDateTime(strStartDate);
                    DateTime end = start.AddMonths(1);
                    end = new DateTime(end.Year, end.Month, 1); // ����1��
                    end = end.AddDays(-1);  // �������һ��

                    strEndDate = strText + end.Day;
                    return;
                }

                if (strText.Length == 8)
                {
                    // ����
                    strStartDate = strText;
                    strEndDate = "";
                    return;
                }

                // text-level: �û���ʾ
                throw new Exception(
                    string.Format("�����ַ��� '{0}' ��ʽ����ȷ��Ӧ��Ϊ4/6/8�ַ�",
                    strText)
                    );
            }
            else
            {
                string strLeft = "";
                string strRight = "";

                strLeft = strText.Substring(0, nRet).Trim();
                strRight = strText.Substring(nRet + 1).Trim();

                if (strLeft.Length != strRight.Length)
                {
                    // text-level: �û���ʾ
                    throw new Exception(
                        string.Format("�����ַ��� '{0}' ��ʽ����ȷ�������ߵĲ��� '{1}' ���ұߵĲ��� '{2}' �ַ���Ӧ��ȡ�",
                        strText,
                        strLeft,
                        strRight)
                        );
                }

                if (strLeft.Length == 4)
                {
                    strStartDate = strLeft + "0101";
                    strEndDate = strRight + "1231";
                    return;
                }

                if (strLeft.Length == 6)
                {
                    strStartDate = strLeft + "01";

                    DateTime start = DateTimeUtil.Long8ToDateTime(strRight + "01");
                    DateTime end = start.AddMonths(1);
                    end = new DateTime(end.Year, end.Month, 1); // ����1��
                    end = end.AddDays(-1);  // �������һ��

                    strEndDate = strRight + end.Day;
                    return;
                }

                if (strLeft.Length == 8)
                {
                    // ����
                    strStartDate = strLeft;
                    strEndDate = strRight;
                    return;
                }

                // text-level: �û���ʾ
                throw new Exception(
                    string.Format("�����ַ��� '{0}' ��ʽ����ȷ�������߻����ұߵĲ��֣�Ӧ��Ϊ4/6/8�ַ�",
                    strText)
                    );
            }
        }

        public static DateTime ParseFreeTimeString(string strTime)
        {
            DateTime parsedBack;
            string[] formats = {
                "yyyy",
                "yyyy.M",
                "yyyy.MM",
                "yyyy.MMM",

                "yyyy.M.d",
                "yyyy.MM.dd",
                "yyyy.MMM.ddd",

                "yyyy/M",
                "yyyy/MM",
                "yyyy/MMM",


                "yyyy/M/d",
                "yyyy/MM/dd",
                "yyyy/MMM/ddd",
                                };

            bool bRet = DateTime.TryParseExact(strTime,
                formats,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out parsedBack);
            if (bRet == false)
            {
                bRet = DateTime.TryParse(strTime,
                out parsedBack);
                if (bRet == false)
                {
                    string strError = "ʱ���ַ��� '" + strTime + "' �޷�����";
                    throw new Exception(strError);
                }
            }
            return parsedBack;
        }

        public static string ForcePublishTime8(string strPublishTime)
        {
            strPublishTime = CanonicalizePublishTimeString(strPublishTime);
            if (strPublishTime.Length > 8)
                strPublishTime = strPublishTime.Substring(0, 8);

            return strPublishTime;
        }

        // �淶��8�ַ��������ַ���
        public static string CanonicalizePublishTimeString(string strText)
        {
            if (strText.Length == 4)
            {
                strText = strText + "0101";
                goto END1;
            }

            if (strText.Length == 6)
            {
                strText = strText + "01";
                goto END1;
            }
            if (strText.Length == 8)
                goto END1;
            if (strText.Length == 10)
                goto END1;

            throw new Exception("���������ַ��� '" + strText + "' ��ʽ����ȷ");

        END1:
            // ���һ��ʱ���ַ����Ƿ����ڴ��ڵ�ʱ��
            string strTest = strText.Substring(0, 8);

            try
            {
                DateTimeUtil.Long8ToDateTime(strTest);
            }
            catch (System.ArgumentOutOfRangeException /*ex*/)
            {
                throw new Exception("�����ַ��� '" + strText + "' ����ȷ: �����˲����ܵ��ꡢ�¡���ֵ");
            }
            catch (Exception ex)
            {
                throw new Exception("�����ַ��� '" + strText + "' ����ȷ: " + ex.Message);
            }

            return strText;
        }

        // �Ƚ�HTTP header�ֶ������ʱ�䡣���Ƚ�millisecond����
        public static long CompareHeaderTime(DateTime time1, DateTime time2)
        {
            time1 = new DateTime(time1.Year, time1.Month, time1.Day, time1.Hour, time1.Minute, time1.Second);
            time2 = new DateTime(time2.Year, time2.Month, time2.Day, time2.Hour, time2.Minute, time2.Second);

            return time1.Ticks - time2.Ticks;
        }

        public static int Date8toRfc1123(string strOrigin,
out string strTarget,
out string strError)
        {
            strError = "";
            strTarget = "";

            strOrigin = strOrigin.Replace("-", "");

            // ��ʽΪ 20060625�� ��Ҫת��Ϊrfc
            if (strOrigin.Length != 8)
            {
                strError = "Դ�����ַ��� '" + strOrigin + "' ��ʽ����ȷ��ӦΪ8�ַ�";
                return -1;
            }


            IFormatProvider culture = new CultureInfo("zh-CN", true);

            DateTime time;
            try
            {
                time = DateTime.ParseExact(strOrigin, "yyyyMMdd", culture);
            }
            catch
            {
                strError = "�����ַ��� '" + strOrigin + "' �ַ���ת��ΪDateTime����ʱ����";
                return -1;
            }

            time = time.ToUniversalTime();
            strTarget = DateTimeUtil.Rfc1123DateTimeString(time);


            return 0;
        }

        // ������һ�������ϵ�ͬһ��
        public static DateTime NextMonth(DateTime start)
        {
            int nDayDelta = 0;
            int nYear = start.Year;
            int nMonth = start.Month;
            int nDay = start.Day;

            if (nMonth == 12)
            {
                nYear++;
                nMonth = 1;
            }
            else
            {
                nMonth++;
            }

            while (true)
            {
                try
                {
                    start = new DateTime(nYear, nMonth, nDay - nDayDelta);
                }
                catch
                {
                    nDayDelta++;
                    continue;
                }

                return start;
            }
        }

        // ������һ�����
        // parameters:
        //      strYear 4�ַ������
        public static string NextYear(string strYear)
        {
            Debug.Assert(strYear.Length == 4, "");
            long v = Convert.ToInt64(strYear);
            return (v+1).ToString().PadLeft(4, '0');
        }


        // ������һ�������ϵ�ͬһ��
        public static DateTime NextYear(DateTime start)
        {
            int nDelta = 0;
            while (true)
            {
                try
                {
                    start = new DateTime(start.Year + 1, start.Month, start.Day - nDelta);
                }
                catch
                {
                    nDelta++;
                    continue;
                }

                return start;
            }
        }

        // Ϊ8�ַ��������ڸ�ʽ����'-'
        public static string AddHyphenToString8(string strDate,
            string strHyphen)
        {
            if (String.IsNullOrEmpty(strDate) == true)
                return strDate;

            if (strDate.Length == 4)
                return strDate;

            if (strDate.Length == 6)
                return strDate.Substring(0, 4)
                + strHyphen + strDate.Substring(4, 2);

            if (strDate.Length == 8)
                return strDate.Substring(0, 4)
                    + strHyphen + strDate.Substring(4, 2)
                    + strHyphen + strDate.Substring(6, 2);

            return strDate;
        }

        public static string DateTimeToString8(DateTime time)
        {
            return time.Year.ToString().PadLeft(4, '0')
                + time.Month.ToString().PadLeft(2, '0')
                + time.Day.ToString().PadLeft(2, '0');
        }

        public static long DateTimeToLong8(DateTime time)
        {
            return Convert.ToInt64(time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0'));
        }

        public static DateTime Long8ToDateTime(long lDay)
        {
            int nYear = Convert.ToInt32(lDay.ToString().PadLeft(8, '0').Substring(0, 4));
            int nMonth = Convert.ToInt32(lDay.ToString().PadLeft(8, '0').Substring(4, 2));
            int nDay = Convert.ToInt32(lDay.ToString().PadLeft(8, '0').Substring(6, 2));

            return new DateTime(nYear, nMonth, nDay);
        }

        // ���ܻ��׳��쳣
        public static DateTime Long8ToDateTime(string strDate8)
        {
            if (strDate8.Length != 8)
                throw new Exception("�����ַ�����ʽ����Ϊ8�ַ���");

            int nYear = Convert.ToInt32(strDate8.Substring(0, 4));
            int nMonth = Convert.ToInt32(strDate8.Substring(4, 2));
            int nDay = Convert.ToInt32(strDate8.Substring(6, 2));

            try
            {
                return new DateTime(nYear, nMonth, nDay);
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                throw new ArgumentException("�ַ��� '"+strDate8+"' ��һ�������ܳ��ֵ�����", ex);
            }
        }

#if NO
        // ��RFC1123ʱ���ַ���ת��Ϊ����һ��ʱ���ַ���
        // exception: �����׳��쳣
        public static string LocalTime(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "G");
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "ʱ���ַ��� '" + strRfc1123Time + "' ��ʽ���󣬲��ǺϷ���RFC1123��ʽ";
            }
        }
#endif

        // ��RFC1123ʱ���ַ���ת��Ϊ����һ��ʱ���ַ���
        // exception: �����׳��쳣
        public static string LocalTime(string strRfc1123Time,
            string strFormat = "G")
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, strFormat);
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "ʱ���ַ��� '" + strRfc1123Time + "' ��ʽ���󣬲��ǺϷ���RFC1123��ʽ";
            }
        }

        // ��RFC1123ʱ���ַ���ת��Ϊ����һ�������ַ���
        // exception: �����׳��쳣
        public static string LocalDate(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";

                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "d");  // "yyyy-MM-dd"
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "�����ַ��� '" + strRfc1123Time + "' ��ʽ���󣬲��ǺϷ���RFC1123��ʽ";
            }
        }

        // ��������RFC1123��ʱ��ֵ�ַ���
        // ע��timeӦ���ڵ���ǰת��Ϊgmtʱ��ֵ
        public static string Rfc1123DateTimeString(DateTime time)
        {
            // System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();

#if NO
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = en;
#endif
            string strTime = time.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",    // info.RFC1123Pattern,
                DateTimeFormatInfo.InvariantInfo);

#if NO
            Thread.CurrentThread.CurrentCulture = save;
#endif

            return strTime;
        }

        // parameters:
        //      time    localʱ�䣬����GMTʱ��
        public static string Rfc1123DateTimeStringEx(DateTime time)
        {
            // System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();

#if NO
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = en;
#endif
            string strTime = time.ToString("ddd, dd MMM yyyy HH':'mm':'ss ", 
                DateTimeFormatInfo.InvariantInfo);

            string strTimeZone = time.ToString("zzz").Replace(":", "");
            if (strTimeZone == "+0000"
                || strTimeZone == "-0000")
                strTimeZone = "GMT";

#if NO
            Thread.CurrentThread.CurrentCulture = save;
#endif

            return strTime + strTimeZone;
        }

        public static DateTime FromUTimeString(string strTime)
        {
            // �Զ����ʽ�ַ���Ϊ��yyyy'-'MM'-'dd HH':'mm':'ss'Z'���� 
            // �޷�����ʱ����������
            IFormatProvider culture = new CultureInfo("zh-CN", true);
            return DateTime.ParseExact(strTime,
                "u",
                culture);
        }

        static TimeSpan GetOffset(string strDigital)
        {
            if (strDigital.Length != 5)
                throw new Exception("strDigital����Ϊ5�ַ�");

            int hours = Convert.ToInt32(strDigital.Substring(1, 2));
            int minutes = Convert.ToInt32(strDigital.Substring(3, 2));
            TimeSpan offset = new TimeSpan(hours, minutes, 0);
            if (strDigital[0] == '-')
                offset = new TimeSpan(offset.Ticks * -1);

            return offset;
        }

        // ��RFC1123�ַ����е�timezone���ַ������
        // parameters:
        //      strMain [out]ȥ��timezone�Ժ����߲���// ����ȥ����߶�������Ĳ���
        //      strTimeZone [out]timezone����
        static int SplitRfc1123TimeZoneString(string strTimeParam,
            out string strMain,
            out string strTimeZone,
            out TimeSpan offset,
            out string strError)
        {
            strError = "";
            strMain = "";
            strTimeZone = "";
            offset = new TimeSpan(0);
            int nRet = 0;

            string strTime = strTimeParam.Trim();

            /*
            // ȥ����������Ĳ���
            int nRet = strTime.IndexOf(",");
            if (nRet != -1)
                strTime = strTime.Substring(nRet + 1).Trim();
             * */

            // һλ��ĸ
            if (strTime.Length > 2
                && strTime[strTime.Length - 2] == ' ')
            {
                strMain = strTime.Substring(0, strTime.Length - 2).Trim();
                strTimeZone = strTime.Substring(strTime.Length - 1);
                if (strTimeZone == "J")
                {
                    strError = "RFC1123�ַ��� '" + strTimeParam + "' ��ʽ���� ���һλTimeZone�ַ�������Ϊ'J'";
                    return -1;
                }

                if (strTimeZone == "Z")
                    return 0;

                int nHours = 0;

                if (strTimeZone[0] >= 'A' && strTimeZone[0] < 'J')
                    nHours = -(strTimeZone[0] - 'A' + 1);
                else if (strTimeZone[0] >= 'K' && strTimeZone[0] <= 'M')
                    nHours = -(strTimeZone[0] - 'B' + 1);
                else if (strTimeZone[0] >= 'N' && strTimeZone[0] <= 'Y')
                    nHours = strTimeZone[0] - 'N' + 1;

                offset = new TimeSpan(nHours, 0, 0);
                return 0;
            }

            // ( "+" / "-") 4DIGIT
            if (strTime.Length > 5
                && (strTime[strTime.Length - 5] == '+' || strTime[strTime.Length - 5] == '-'))
            {
                strMain = strTime.Substring(0, strTime.Length - 5).Trim();
                strTimeZone = strTime.Substring(strTime.Length - 5);

                try
                {
                    offset = GetOffset(strTimeZone);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                return 0;
            }

            string[] modes = {
                            "GMT",
                            "UT",
                            "EST",
                            "EDT",
                            "CST",
                            "CDT",
                            "MST",
                            "MDT",
                            "PST",
                            "PDT"};
            if (strTime.Length <= 3)
            {
                strError = "RFC1123�ַ��� '" + strTimeParam + "' ��ʽ���� �ַ�������";
                return -1;
            }

            string strPart = strTime.Substring(strTime.Length - 3);
            foreach (string mode in modes)
            {
                nRet = strPart.LastIndexOf(mode);
                if (nRet != -1)
                {
                    nRet = strTime.LastIndexOf(mode);
                    Debug.Assert(nRet != -1, "");

                    strMain = strTime.Substring(0, nRet).Trim();
                    strTimeZone = mode;

                    if (strTimeZone == "GMT" || strTimeZone == "UT")
                        return 0;

                    string strDigital = "";

                    switch (strTimeZone)
                    {
                        case "EST":
                            strDigital = "-0500";
                            break;
                        case "EDT":
                            strDigital = "-0400";
                            break;
                        case "CST":
                            strDigital = "-0600";
                            break;
                        case "CDT":
                            strDigital = "-0500";
                            break;
                        case "MST":
                            strDigital = "-0700";
                            break;
                        case "MDT":
                            strDigital = "-0600";
                            break;
                        case "PST":
                            strDigital = "-0800";
                            break;
                        case "PDT":
                            strDigital = "-0700";
                            break;
                        default:
                            strError = "error";
                            return -1;
                    }

                    try
                    {
                        offset = GetOffset(strDigital);
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }

                    return 0;
                }
            }

            strError = "RFC1123�ַ��� '" + strTimeParam + "' ��ʽ���� TimeZone���ֲ��Ϸ�";
            return -1;
        }

        public static bool TryParseRfc1123DateTimeString(string strTime, out DateTime time)
        {
            try
            {
                time = FromRfc1123DateTimeString(strTime);
                return true;
            }
            catch
            {
                time = new DateTime(0);
                return false;
            }
        }

        // ���ַ���ת��ΪDateTime����
        // ע�ⷵ�ص���GMTʱ��
        // ע������׳��쳣
        public static DateTime FromRfc1123DateTimeString(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                throw new Exception("ʱ���ַ���Ϊ��");

            string strError = "";
            string strMain = "";
            string strTimeZone = "";
            TimeSpan offset;
            // ��RFC1123�ַ����е�timezone���ַ������
            // parameters:
            //      strMain [out]ȥ��timezone�Ժ����߲���
            //      strTimeZone [out]timezone����
            int nRet = SplitRfc1123TimeZoneString(strTime,
            out strMain,
            out strTimeZone,
            out offset,
            out strError);
            if (nRet == -1)
                throw new Exception(strError);

            DateTime parsedBack;
            string[] formats = {
                "ddd, dd MMM yyyy HH':'mm':'ss",   // [ddd, ] 'GMT'
                "dd MMM yyyy HH':'mm':'ss",
                "ddd, dd MMM yyyy HH':'mm",
                "dd MMM yyyy HH':'mm",
                                };

            bool bRet = DateTime.TryParseExact(strMain,
                formats,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out parsedBack);
            if (bRet == false)
            {
                strError = "ʱ���ַ��� '" + strTime + "' ����RFC1123��ʽ";
                throw new Exception(strError);
            }

            return parsedBack - offset;
        }
#if NO
        // ���ַ���ת��ΪDateTime����
        // ע�ⷵ�ص���GMTʱ��
        // ע������׳��쳣
        public static DateTime FromRfc1123DateTimeString(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                throw new Exception("ʱ���ַ���Ϊ��");

            string strError = "";
            string strMain = "";
            string strTimeZone = "";
            TimeSpan offset;
            // ��RFC1123�ַ����е�timezone���ַ������
            // parameters:
            //      strMain [out]ȥ��timezone�Ժ����߲���
            //      strTimeZone [out]timezone����
            int nRet = SplitRfc1123TimeZoneString(strTime,
            out strMain,
            out strTimeZone,
            out offset,
            out strError);
            if (nRet == -1)
                throw new Exception(strError);

            System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;
            DateTime parsedBack = DateTime.ParseExact(strMain + " GMT",
                info.RFC1123Pattern,
                en.DateTimeFormat/*,
				DateTimeStyles.AdjustToUniversal*/
                                                  );
            Thread.CurrentThread.CurrentCulture = save;

            return parsedBack - offset;
        }

#endif

        // ��RFC1123ʱ���ַ���ת��Ϊ���ر�����̬�ַ���
        // ע������׳��쳣
        public static string Rfc1123DateTimeStringToLocal(string strTime)
        {
            if (String.IsNullOrEmpty(strTime) == true)
                return "";

            return FromRfc1123DateTimeString(strTime).ToLocalTime().ToString();
#if NO
            System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;
            DateTime parsedBack = DateTime.ParseExact(strTime,
                info.RFC1123Pattern,
                en.DateTimeFormat/*,
				DateTimeStyles.AdjustToUniversal*/
                                                  );
            Thread.CurrentThread.CurrentCulture = save;

            DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(parsedBack);


            return localTime.ToString();
#endif

        }

        // ��RFC1123ʱ���ַ���ת��Ϊ���ر�����̬�ַ���
        // ע������׳��쳣
        public static string Rfc1123DateTimeStringToLocal(string strTime,
            string strFormat)
        {
            if (String.IsNullOrEmpty(strTime) == true)
                return "";
#if NO
            System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;
            DateTime parsedBack = DateTime.ParseExact(strTime,
                info.RFC1123Pattern,
                en.DateTimeFormat
                                                  );
            Thread.CurrentThread.CurrentCulture = save;
#endif

            DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(
                FromRfc1123DateTimeString(strTime)
                );

            return localTime.ToString(strFormat);
        }

        // ��uʱ���ַ���ת��Ϊ���ر�����̬�ַ���
        public static string uDateTimeStringToLocal(string strTime)
        {
            if (String.IsNullOrEmpty(strTime) == true)
                return "";
#if NO
            System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;
#endif
            DateTime parsedBack = DateTime.ParseExact(strTime,
                "yyyy-MM-dd HH:mm:ssZ",
                DateTimeFormatInfo.InvariantInfo
                // en.DateTimeFormat
                );
#if NO
            Thread.CurrentThread.CurrentCulture = save;
#endif

            DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(parsedBack);
            return localTime.ToString("yyyy-MM-dd HH:mm:ss");
        }


        //

        public static string DateTimeString(DateTime time)
        {
            System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();

#if NO
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = en;
#endif

            string strTime = time.ToString(info.LongTimePattern,
                DateTimeFormatInfo.InvariantInfo);

#if NO
            Thread.CurrentThread.CurrentCulture = save;
#endif
            return strTime;
        }

        public static DateTime ToDateTime(string strTime)
        {
            System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();

#if NO
            CultureInfo en = new CultureInfo("en-US");

            CultureInfo save = Thread.CurrentThread.CurrentCulture;
#endif
            DateTime parsedBack = DateTime.ParseExact(strTime,
                info.LongTimePattern,
                DateTimeFormatInfo.InvariantInfo
                // en.DateTimeFormat
                                                  );
#if NO
            Thread.CurrentThread.CurrentCulture = save;
#endif
            // DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(parsedBack);
            //return localTime.ToString();
            return parsedBack;
        }

    }

}
