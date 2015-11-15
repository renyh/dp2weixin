using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace DigitalPlatform.Text
{
    public class PriceUtil
    {
        // ����۸�˻�
        // ��PrintOrderForm��ת�ƹ���
        public static int MultiPrice(string strPrice,
            int nCopy,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            int nRet = PriceUtil.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "���� '" + strValue + "' ��ʽ����ȷ";
                return -1;
            }

            value *= (decimal)nCopy;

            strResult = strPrefix + value.ToString() + strPostfix;
            return 0;
        }

        // �ܹ�����˺Ż��߳�����
        public static string GetPurePrice(string strText)
        {
            string strError = "";

            string strLeft = "";
            string strRight = "";
            string strOperator = "";
            // �ȴ���˳���
            // return:
            //      -1  ����
            //      0   û�з��ֳ˺š�����
            //      1   ���ֳ˺Ż��߳���
            int nRet = ParseMultipcation(strText,
                out strLeft,
                out strRight,
                out strOperator,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            if (nRet == 0)
                return OldGetPurePrice(strText);

            Debug.Assert(nRet == 1, "");
            string strMultiper = "";
            string strPrice = "";


            if (StringUtil.IsDouble(strLeft) == false
                && StringUtil.IsDouble(strRight) == false)
            {
                strError = "����ַ�����ʽ���� '" + strText + "'���˺Ż���ŵ����߱���������һ���Ǵ�����";
                throw new Exception(strError);
            }

            if (StringUtil.IsDouble(strLeft) == false)
            {
                strPrice = strLeft;
                strMultiper = strRight;
            }
            else if (StringUtil.IsDouble(strRight) == false)
            {
                strPrice = strRight;
                strMultiper = strLeft;
                if (strOperator == "/")
                {
                    strError = "����ַ��� '" + strText + "' ��ʽ���󡣳��ŵ��ұ߲����Ǵ�����";
                    throw new Exception(strError);
                }
            }
            else
            {
                // Ĭ������Ǽ۸��ұ��Ǳ���
                strPrice = strLeft;
                strMultiper = strRight;
            }

            string strValue = OldGetPurePrice(strPrice);

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "��������ַ��� '" + strPrice + "' ��, ���ֲ��� '" + strValue + "' ��ʽ����ȷ";
                throw new Exception(strError);
            }

            if (String.IsNullOrEmpty(strOperator) == false)
            {
                double multiper = 0;
                try
                {
                    multiper = Convert.ToDouble(strMultiper);
                }
                catch
                {
                    strError = "���� '" + strMultiper + "' ��ʽ����ȷ";
                    throw new Exception(strError);
                }

                if (strOperator == "*")
                {
                    value = (decimal)((double)value * multiper);
                }
                else
                {
                    Debug.Assert(strOperator == "/", "");

                    if (multiper == 0)
                    {
                        strError = "����ַ�����ʽ���� '" + strText + "'�����������У���������Ϊ0";
                        throw new Exception(strError);
                    }

                    value = (decimal)((double)value / multiper);
                }

                return value.ToString();
            }

            return strValue;
        }

        // �Ӹ��ӵ��ַ����У���������۸����ֲ��֣�����С���㣩��
        // 2006/11/15 �ܴ�������ǰ��������
        public static string OldGetPurePrice(string strPrice)
        {
            if (String.IsNullOrEmpty(strPrice) == true)
                return strPrice;

            string strResult = "";
            int nSegment = 0;   // 0 �����ֶ� 1���ֶ� 2 �����ֶ�
            int nPointCount = 0;

            bool bNegative = false; // �Ƿ�Ϊ����

            for (int i = 0; i < strPrice.Length; i++)
            {
                char ch = strPrice[i];

                if ((ch <= '9' && ch >= '0')
                    || ch == '.')
                {

                    if (ch == '.')
                    {
                        if (nPointCount == 1)
                            break;  // �Ѿ����ֹ�һ��С������

                        nPointCount++;
                    }

                    if (nSegment == 0)
                    {
                        nSegment = 1;
                    }
                }
                else
                {
                    if (nSegment == 0)
                    {
                        if (ch == '-')
                            bNegative = true;
                    }

                    if (nSegment == 1)
                    {
                        nSegment = 2;
                        break;
                    }
                }

                if (nSegment == 1)
                    strResult += ch;
            }

            // �����һ������С����
            if (strResult.Length > 0
                && strResult[0] == '.')
            {
                strResult = "0" + strResult;
            }

            // 2008/11/15
            if (bNegative == true)
                return "-" + strResult;

            return strResult;
        }


        // ���ܼ۸�
        // ���ҵ�λ��ͬ�ģ��������
        // ��������Ҫ������ʾ�������Զ����������� -- �Ѵ����ַ��������������
        // return:
        //      ���ܺ�ļ۸��ַ���
        public static string TotalPrice(List<string> prices)
        {
            string strResult = "";
            string strError = "";

            int nRet = TotalPrice(prices,
                out strResult,
                out strError);
            if (nRet == -1)
                return strError;

            return strResult;
        }
        
        // ���ܼ۸�
        // ���ҵ�λ��ͬ�ģ��������
        // ��������������һ���汾���Ƿ���List<string>��
        // return:
        //      -1  error
        //      0   succeed
        public static int TotalPrice(List<string> prices,
            out string strTotalPrice,
            out string strError)
        {
            strError = "";
            strTotalPrice = "";
            List<string> results = null;
            int nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(results != null, "");
            if (results.Count == 0)
                return 0;

            strTotalPrice = JoinPriceString(results);
            return 0;
        }

        // �����ɼ۸��ַ����������
        public static string JoinPriceString(List<string> prices)
        {
            string strResult = "";
            for (int i = 0; i < prices.Count; i++)
            {
                string strPrice = prices[i].Trim();
                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;
                if (strPrice[0] == '+' || strPrice[0] == '-')
                    strResult += strPrice;
                else
                {
                    // ��һ���۸�ǰ�治�ü�+��
                    if (String.IsNullOrEmpty(strResult) == false)
                        strResult += "+";

                    strResult += strPrice;
                }
            }

            return strResult;
        }

        // ���������۸��ַ���
        public static string JoinPriceString(string strPrice1,
            string strPrice2)
        {
            if (string.IsNullOrEmpty(strPrice1) == true
                && string.IsNullOrEmpty(strPrice2) == true)
                return "";

            if (string.IsNullOrEmpty(strPrice1) == true)
                return strPrice2;
            if (string.IsNullOrEmpty(strPrice2) == true)
                return strPrice1;

            strPrice1 = strPrice1.Trim();
            strPrice2 = strPrice2.Trim();

            if (String.IsNullOrEmpty(strPrice1) == true
                && String.IsNullOrEmpty(strPrice2) == true)
                return "";

            if (String.IsNullOrEmpty(strPrice1) == true)
                return strPrice2;

            if (String.IsNullOrEmpty(strPrice2) == true)
                return strPrice1;

            if (strPrice2[0] == '+'
                || strPrice2[0] == '-')
                return strPrice1 + strPrice2;

            return strPrice1 + "+" + strPrice2;
        }

        // ������"-123.4+10.55-20.3"�ļ۸��ַ�����ת������
        // parameters:
        //      bSum    �Ƿ�Ҫ˳�����? true��ʾҪ����
        public static int NegativePrices(string strPrices,
            bool bSum,
            out string strResultPrice,
            out string strError)
        {
            strError = "";
            strResultPrice = "";

            strPrices = strPrices.Trim();

            if (String.IsNullOrEmpty(strPrices) == true)
                return 0;
            
            List<string> prices = null;
            // ������"-123.4+10.55-20.3"�ļ۸��ַ����и�Ϊ�����ļ۸��ַ����������Դ���������
            // return:
            //      -1  error
            //      0   succeed
            int nRet = SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                return -1;

            // ֱ��ÿ����ת
            if (bSum == false)
            {
                for (int i = 0; i < prices.Count; i++)
                {
                    string strOnePrice = prices[i];
                    if (String.IsNullOrEmpty(strOnePrice) == true)
                        continue;
                    if (strOnePrice[0] == '+')
                        strResultPrice += "-" + strOnePrice.Substring(1);
                    else if (strOnePrice[0] == '-')
                        strResultPrice += "+" + strOnePrice.Substring(1);
                    else
                        strResultPrice += "-" + strOnePrice;    // ȱʡΪ����
                }

                return 0;
            }

            List<string> results = new List<string>();

            // ���ܼ۸�
            // ���ҵ�λ��ͬ�ģ��������
            // return:
            //      -1  error
            //      0   succeed
            nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            for (int i = 0; i < results.Count; i++)
            {
                string strOnePrice = results[i];
                if (String.IsNullOrEmpty(strOnePrice) == true)
                    continue;
                if (strOnePrice[0] == '+')
                    strResultPrice += "-" + strOnePrice.Substring(1);
                else if (strOnePrice[0] == '-')
                    strResultPrice += "+" + strOnePrice.Substring(1);
                else
                    strResultPrice += "-" + strOnePrice;    // ȱʡΪ����
            }

            return 0;
        }

        // �Ƚ������۸��ַ���
        // return:
        //      -3  ���ֲ�ͬ���޷�ֱ�ӱȽ� strError����˵��
        //      -2  error strError����˵��
        //      -1  strPrice1С��strPrice2
        //      0   ����
        //      1   strPrice1����strPrice2
        public static int Compare(string strPrice1,
            string strPrice2,
            out string strError)
        {
            strError = "";

            string strPrefix1 = "";
            string strValue1 = "";
            string strPostfix1 = "";
            int nRet = ParsePriceUnit(strPrice1,
                out strPrefix1,
                out strValue1,
                out strPostfix1,
                out strError);
            if (nRet == -1)
            {
                strError = "����ַ���1 '" + strPrice1 + "' ��������: " + strError;
                return -2;
            }

            decimal value1 = 0;
            try
            {
                value1 = Convert.ToDecimal(strValue1);
            }
            catch
            {
                strError = "���� '" + strValue1 + "' ��ʽ����ȷ";
                return -2;
            }

            if (strPrefix1 == "" && strPostfix1 == "")
                strPrefix1 = "CNY";

            string strPrefix2 = "";
            string strValue2 = "";
            string strPostfix2 = "";
            nRet = ParsePriceUnit(strPrice2,
                out strPrefix2,
                out strValue2,
                out strPostfix2,
                out strError);
            if (nRet == -1)
            {
                strError = "����ַ���2 '" + strPrice2 + "' ��������: " + strError;
                return -2;
            }

            if (strPrefix2 == "" && strPostfix2 == "")
                strPrefix2 = "CNY";

            if (strPrefix1 != strPrefix2
                || strPostfix1 != strPostfix2)
            {
                strError = "���ֲ�ͬ(һ����'" + strPrice1 + "'��һ����'" + strPrice2 + "')���޷����н��Ƚ�";
                return -3;
            }

            decimal value2 = 0;
            try
            {
                value2 = Convert.ToDecimal(strValue2);
            }
            catch
            {
                strError = "���� '" + strValue2 + "' ��ʽ����ȷ";
                return -2;
            }

            if (value1 < value2)
                return -1;

            if (value1 == value2)
                return 0;

            Debug.Assert(value1 > value2, "");

            return 1;
        }

        // �������ɸ��۸��ַ����Ƿ񶼱�ʾ��0?
        // return:
        //      -1  ����
        //      0   ��Ϊ0
        //      1   Ϊ0
        public static int IsZero(List<string> prices,
            out string strError)
        {
            strError = "";

            List<CurrencyItem> items = new List<CurrencyItem>();

            // �任ΪPriceItem
            for (int i = 0; i < prices.Count; i++)
            {
                string strPrefix = "";
                string strValue = "";
                string strPostfix = "";
                int nRet = ParsePriceUnit(prices[i],
                    out strPrefix,
                    out strValue,
                    out strPostfix,
                    out strError);
                if (nRet == -1)
                    return -1;
                decimal value = 0;
                try
                {
                    value = Convert.ToDecimal(strValue);
                }
                catch
                {
                    strError = "���� '" + strValue + "' ��ʽ����ȷ";
                    return -1;
                }

                CurrencyItem item = new CurrencyItem();
                item.Prefix = strPrefix;
                item.Postfix = strPostfix;
                item.Value = value;

                items.Add(item);
            }

            // ����
            for (int i = 0; i < items.Count; i++)
            {
                CurrencyItem item = items[i];

                if (item.Value != 0)
                    return 0;   // �м�����˲�Ϊ0��
            }

            return 1;   // ȫ��Ϊ0
        }

        // ������"-123.4+10.55-20.3"�ļ۸��ַ����鲢����
        public static int SumPrices(string strPrices,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            List<string> prices = null;
            // ������"-123.4+10.55-20.3"�ļ۸��ַ����и�Ϊ�����ļ۸��ַ����������Դ���������
            // return:
            //      -1  error
            //      0   succeed
            int nRet = SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                return -1;

            // ���ܼ۸�
            // ���ҵ�λ��ͬ�ģ��������
            // return:
            //      -1  error
            //      0   succeed
            nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 2012/3/7
        // ������"-123.4+10.55-20.3"�ļ۸��ַ����鲢����
        public static int SumPrices(string strPrices,
            out string strSumPrices,
            out string strError)
        {
            strError = "";
            strSumPrices = "";


            List<string> prices = null;
            // ������"-123.4+10.55-20.3"�ļ۸��ַ����и�Ϊ�����ļ۸��ַ����������Դ���������
            // return:
            //      -1  error
            //      0   succeed
            int nRet = SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> results = new List<string>();

            // ���ܼ۸�
            // ���ҵ�λ��ͬ�ģ��������
            // return:
            //      -1  error
            //      0   succeed
            nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            strSumPrices = JoinPriceString(results);
            return 0;
        }

        // ������"-123.4+10.55-20.3"�ļ۸��ַ����и�Ϊ�����ļ۸��ַ����������Դ���������
        // return:
        //      -1  error
        //      0   succeed
        public static int SplitPrices(string strPrices,
            out List<string> prices,
            out string strError)
        {
            strError = "";
            prices = new List<string>();

            strPrices = strPrices.Replace("+", ",+").Replace("-",",-");
            string[] parts = strPrices.Split(new char[] {','});
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                prices.Add(strPart);
            }

            return 0;
        }

        // 2012/3/7
        // У�����ַ�����ʽ��ȷ��
        // return:
        //      -1  �д�
        //      0   û�д�
        public static int VerifyPriceFormat(
            List<string> valid_formats,
            string strString,
            out string strError)
        {
            strError = "";

            // û�и�ʽ���壬�Ͳ���У��
            if (valid_formats.Count == 0)
                return 0;

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";

            int nRet = ParsePriceUnit(strString,
            out strPrefix,
            out strValue,
            out strPostfix,
            out strError);
            if (nRet == -1)
                return -1;

            foreach (string fmt in valid_formats)
            {
                string[] parts = fmt.Split(new char[] { '|' });
                string strPrefixFormat = "";
                string strValueFormat = "";
                string strPostfixFormat = "";
                if (parts.Length > 0)
                    strPrefixFormat = parts[0];
                if (parts.Length > 1)
                    strValueFormat = parts[1];
                if (parts.Length > 2)
                    strPostfixFormat = parts[2];

                if (string.IsNullOrEmpty(strPrefixFormat) == false
                    && strPrefix != strPrefixFormat)
                    continue;

                // ��ʱ��У��value����

                if (string.IsNullOrEmpty(strPostfixFormat) == false
    && strPostfix != strPostfixFormat)
                    continue;

                return 0;
            }

            strError = "����ַ��� '"+strString+"' �ĸ�ʽ�����϶��� '" + StringUtil.MakePathList(valid_formats) + "' ��Ҫ��";
            return -1;
        }

        // �����۸����
        // ����ǰ�����+ -��
        // return:
        //      -1  ����
        //      0   �ɹ�
        public static int ParsePriceUnit(string strString,
            out string strPrefix,
            out string strValue,
            out string strPostfix,
            out string strError)
        {
            strPrefix = "";
            strValue = "";
            strPostfix = "";
            strError = "";

            strString = strString.Trim();
            // ȥ������ 2012/9/1
            strString = strString.Replace(",", "");
            strString = strString.Replace("��", "");

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "����ַ���Ϊ��";
                return -1;
            }

            bool bNegative = false; // �Ƿ�Ϊ����
            if (strString[0] == '+')
            {
                bNegative = false;
                strString = strString.Substring(1).Trim();
            }
            else if (strString[0] == '-')
            {
                bNegative = true;
                strString = strString.Substring(1).Trim();
            }

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "����ַ���(��������������)Ϊ��";
                return -1;
            }

            bool bInPrefix = true;

            for (int i = 0; i < strString.Length; i++)
            {
                if ((strString[i] >= '0' && strString[i] <= '9')
                    || strString[i] == '.')
                {
                    bInPrefix = false;
                    strValue += strString[i];
                }
                else
                {
                    if (bInPrefix == true)
                        strPrefix += strString[i];
                    else
                    {
                        strPostfix = strString.Substring(i).Trim();
                        break;
                    }
                }
            }

            strPrefix = strPrefix.Trim();   // 2012/3/7

            if (string.IsNullOrEmpty(strValue) == true)
            {
                strError = "����ַ��� '" + strString + "' ȱ�����ֲ���";
                return -1;
            }

            // 2012/1/5
            if (strPrefix.IndexOfAny(new char[] { '+', '-' }) != -1
                || strPostfix.IndexOfAny(new char[] { '+', '-' }) != -1)
            {
                strError = "���� + �� - ֻӦ�����ڵ�������ַ����ĵ�һ���ַ�λ��";
                return -1;
            }

            // 2008/11/11
            if (bNegative == true)
                strValue = "-" + strValue;

            return 0;
        }

        // return:
        //      -1  ����
        //      0   û�з��ֳ˺š����š�ע���ʱ strLeft �� strRight ���صĶ��ǿ�
        //      1   ���ֳ˺Ż��߳���
        static int ParseMultipcation(string strText,
            out string strLeft,
            out string strRight,
            out string strOperator,
            out string strError)
        {
            strLeft = "";
            strRight = "";
            strOperator = "";
            strError = "";

            int nRet = strText.IndexOfAny(new char[] {'/','*'});
            if (nRet == -1)
                return 0;

            strLeft = strText.Substring(0, nRet).Trim();
            strRight = strText.Substring(nRet + 1).Trim();
            strOperator = strText.Substring(nRet, 1);
            return 1;
        }

        // ������������ַ��������� CNY10.00 �� -CNY100.00/7
        public static int ParseSinglePrice(string strText,
            out CurrencyItem item,
            out string strError)
        {
            strError = "";
            item = new CurrencyItem();

            if (string.IsNullOrEmpty(strText) == true)
                return 0;

            strText = strText.Trim();

            if (String.IsNullOrEmpty(strText) == true)
                return 0;

            string strLeft = "";
            string strRight = "";
            string strOperator = "";
            // �ȴ���˳���
            // return:
            //      -1  ����
            //      0   û�з��ֳ˺š�����
            //      1   ���ֳ˺Ż��߳���
            int nRet = ParseMultipcation(strText,
                out strLeft,
                out strRight,
                out strOperator,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                Debug.Assert(strOperator.Length == 1, "");

                if (String.IsNullOrEmpty(strLeft) == true
                    || String.IsNullOrEmpty(strRight) == true)
                {
                    strError = "����ַ�����ʽ���� '" + strText + "'���˺Ż���ŵ����߱��붼������";
                    return -1;
                }
            }

            string strPrice = "";
            string strMultiper = "";

            if (nRet == 0)
            {
                Debug.Assert(String.IsNullOrEmpty(strLeft) == true, "");
                Debug.Assert(String.IsNullOrEmpty(strRight) == true, "");
                Debug.Assert(String.IsNullOrEmpty(strOperator) == true, "");

                strPrice = strText.Trim();
            }
            else
            {
                Debug.Assert(nRet == 1, "");

                if (StringUtil.IsDouble(strLeft) == false
                    && StringUtil.IsDouble(strRight) == false)
                {
                    strError = "����ַ�����ʽ���� '" + strText + "'���˺Ż���ŵ����߱���������һ���Ǵ�����";
                    return -1;
                }


                if (StringUtil.IsDouble(strLeft) == false)
                {
                    strPrice = strLeft;
                    strMultiper = strRight;
                }
                else if (StringUtil.IsDouble(strRight) == false)
                {
                    strPrice = strRight;
                    strMultiper = strLeft;
                    if (strOperator == "/")
                    {
                        strError = "����ַ�����ʽ���� '" + strText + "'�����ŵ��ұ߲����Ǵ�����";
                        return -1;
                    }
                }
                else
                {
                    // Ĭ������Ǽ۸��ұ��Ǳ���
                    strPrice = strLeft;
                    strMultiper = strRight;
                }
            }

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            nRet = ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strValue) == true)
            {
                strError = "����ַ��� '" + strPrice + "' ��û�а������ֲ���";
                return -1;
            }

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "����ַ��� '" + strPrice + "' ��, ���ֲ��� '" + strValue + "' ��ʽ����ȷ";
                return -1;
            }

            if (String.IsNullOrEmpty(strOperator) == false)
            {
                double multiper = 0;
                try
                {
                    multiper = Convert.ToDouble(strMultiper);
                }
                catch
                {
                    strError = "���� '" + strMultiper + "' ��ʽ����ȷ";
                    return -1;
                }

                if (strOperator == "*")
                {
                    value = (decimal)((double)value * multiper);
                }
                else
                {
                    Debug.Assert(strOperator == "/", "");

                    if (multiper == 0)
                    {
                        strError = "����ַ�����ʽ���� '" + strText + "'�����������У���������Ϊ0";
                        return -1;
                    }

                    value = (decimal)((double)value / multiper);
                }
            }

            item.Prefix = strPrefix.ToUpper();
            item.Postfix = strPostfix.ToUpper();
            item.Value = value;

            // ȱʡ����Ϊ�����
            if (item.Prefix == "" && item.Postfix == "")
                item.Prefix = "CNY";

            return 0;
        }

        // ���ܼ۸�
        // ���ҵ�λ��ͬ�ģ��������
        // parameters:
        //      prices  ���ɵ�һ�۸��ַ������ɵ����顣��δ���й�����
        // return:
        //      -1  error
        //      0   succeed
        public static int TotalPrice(List<string> prices,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            List<CurrencyItem> items = new List<CurrencyItem>();

            // �任ΪPriceItem
            for (int i = 0; i < prices.Count; i++)
            {
                string strText = prices[i].Trim();

                if (String.IsNullOrEmpty(strText) == true)
                    continue;

#if NO
                string strLeft = "";
                string strRight = "";
                string strOperator = "";
                // �ȴ���˳���
                // return:
                //      -1  ����
                //      0   û�з��ֳ˺š�����
                //      1   ���ֳ˺Ż��߳���
                int nRet = ParseMultipcation(strText,
                    out strLeft,
                    out strRight,
                    out strOperator,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    Debug.Assert(strOperator.Length == 1, "");

                    if (String.IsNullOrEmpty(strLeft) == true
                        || String.IsNullOrEmpty(strRight) == true)
                    {
                        strError = "����ַ�����ʽ���� '" + strText + "'���˺Ż���ŵ����߱��붼������";
                        return -1;
                    }
                }


                string strPrice = "";
                string strMultiper = "";

                if (nRet == 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strLeft) == true, "");
                    Debug.Assert(String.IsNullOrEmpty(strRight) == true, "");
                    Debug.Assert(String.IsNullOrEmpty(strOperator) == true, "");

                    strPrice = strText.Trim();
                }
                else
                {
                    Debug.Assert(nRet == 1, "");

                    if (StringUtil.IsDouble(strLeft) == false
                        && StringUtil.IsDouble(strRight) == false)
                    {
                        strError = "����ַ�����ʽ���� '" + strText + "'���˺Ż���ŵ����߱���������һ���Ǵ�����";
                        return -1;
                    }


                    if (StringUtil.IsDouble(strLeft) == false)
                    {
                        strPrice = strLeft;
                        strMultiper = strRight;
                    }
                    else if (StringUtil.IsDouble(strRight) == false)
                    {
                        strPrice = strRight;
                        strMultiper = strLeft;
                        if (strOperator == "/")
                        {
                            strError = "����ַ�����ʽ���� '" + strText + "'�����ŵ��ұ߲����Ǵ�����";
                            return -1;
                        }
                    }
                    else
                    {
                        // Ĭ������Ǽ۸��ұ��Ǳ���
                        strPrice = strLeft;
                        strMultiper = strRight;
                    }
                }

                string strPrefix = "";
                string strValue = "";
                string strPostfix = "";
                nRet = ParsePriceUnit(strPrice,
                    out strPrefix,
                    out strValue,
                    out strPostfix,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 2012/1/5
                if (string.IsNullOrEmpty(strValue) == true)
                {
                    strError = "��������ַ��� '" + strPrice + "' ��û�а������ֲ���";
                    return -1;
                }

                decimal value = 0;
                try
                {
                    value = Convert.ToDecimal(strValue);
                }
                catch
                {
                    strError = "��������ַ��� '" + strPrice + "' ��, ���ֲ��� '" + strValue + "' ��ʽ����ȷ";
                    return -1;
                }

                if (String.IsNullOrEmpty(strOperator) == false)
                {
                    double multiper = 0;
                    try
                    {
                        multiper = Convert.ToDouble(strMultiper);
                    }
                    catch
                    {
                        strError = "���� '" + strMultiper + "' ��ʽ����ȷ";
                        return -1;
                    }

                    if (strOperator == "*")
                    {
                        value = (decimal)((double)value * multiper);
                    }
                    else
                    {
                        Debug.Assert(strOperator == "/", "");

                        if (multiper == 0)
                        {
                            strError = "����ַ�����ʽ���� '" + strText + "'�����������У���������Ϊ0";
                            return -1;
                        }

                        value = (decimal)((double)value / multiper);
                    }
                }

                PriceItem item = new PriceItem();
                item.Prefix = strPrefix.ToUpper();
                item.Postfix = strPostfix.ToUpper();
                item.Value = value;

                // ȱʡ����Ϊ�����
                if (item.Prefix == "" && item.Postfix == "")
                    item.Prefix = "CNY";
#endif
                CurrencyItem item = null;
                int nRet = ParseSinglePrice(strText,
            out item,
            out strError);
                if (nRet == -1)
                    return -1;

                items.Add(item);
            }

            // ����
            for (int i = 0; i < items.Count; i++)
            {
                CurrencyItem item = items[i];

                for (int j = i + 1; j < items.Count; j++)
                {
                    CurrencyItem current_item = items[j];
                    if (current_item.Prefix == item.Prefix
                        && current_item.Postfix == item.Postfix)
                    {
                        item.Value += current_item.Value;
                        items.RemoveAt(j);
                        j--;
                    }

                    /*
                else
                    break;
                     * */
                    // ������һ��BUG��û�����򣬲���֪�����滹��û���ظ��������أ�����break��2009/10/10 changed
                }
            }

            // ���
            for (int i = 0; i < items.Count; i++)
            {
                CurrencyItem item = items[i];
                decimal value = item.Value;

                // ����Ҫ������ǰ��
                if (value < 0)
                    results.Add("-" + item.Prefix + (-value).ToString() + item.Postfix);
                else
                    results.Add(item.Prefix + value.ToString() + item.Postfix);
            }

            return 0;
        }

    }

    /// <summary>
    /// �������
    /// </summary>
    public class CurrencyItem
    {
        /// <summary>
        /// ǰ׺�ַ���
        /// </summary>
        public string Prefix = "";
        /// <summary>
        /// ��׺�ַ���
        /// </summary>
        public string Postfix = "";
        /// <summary>
        /// ��ֵ
        /// </summary>
        public decimal Value = 0;
    }
}
