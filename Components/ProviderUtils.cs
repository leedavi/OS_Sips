using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DotNetNuke.Common.Utilities;

namespace OS_Sips
{
    public class ProviderUtils
    {
        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_Sipspayment", "OS_SipsPAYMENT", Utils.GetCurrentCulture());
            return info;
        }

        public static String GetBankRemotePost(OrderData orderData)
        {
            var rPost = new RemotePost();

            var objCtrl = new NBrightBuyController();
            var settings = objCtrl.GetPluginSinglePageData("OS_Sipspayment", "OS_SipsPAYMENT", Utils.GetCurrentCulture());

            var appliedtotal = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal");
            var alreadypaid = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");
            var orderTotal = Regex.Replace((appliedtotal - alreadypaid).ToString("0.00"), "[^0-9]", "");

            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/OS_Sips");

            var pathfile = PortalSettings.Current.HomeDirectoryMapPath.TrimEnd('\\') + "\\" + settings.GetXmlProperty("genxml/textbox/paramfolder") + "\\pathfile";
            var transid = Convert.ToDateTime(orderData.PurchaseInfo.ModifiedDate).Ticks.ToString();
            transid = transid.Substring(transid.Length - 6);

            var parms = "";
            parms = "pathfile=\"" + pathfile + "\" ";
            parms += "merchant_id=" + settings.GetXmlProperty("genxml/textbox/merchantid") + " ";
            parms += "merchant_country=" + settings.GetXmlProperty("genxml/textbox/merchantcountry") + " ";
            parms += "amount=" + orderTotal + " ";
            parms += "currency_code=" + settings.GetXmlProperty("genxml/textbox/currencycode") + " ";
            parms += "transaction_id=" + transid + " ";
            parms += "payment_means=" + settings.GetXmlProperty("genxml/textbox/paymentmeans") + " ";
            parms += "order_id=" + orderData.PurchaseInfo.ItemID.ToString("") + " ";

            var param = new string[3];
            param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
            param[1] = "status=1";
            var storeSettings = new StoreSettings(orderData.PortalId);
            var sUrlOk = Globals.NavigateURL(storeSettings.PaymentTabId, "", param);
            param[1] = "status=0";
            var sUrlKo = Globals.NavigateURL(storeSettings.PaymentTabId, "", param);
            parms += "normal_return_url=" + sUrlOk + " ";
            parms += "cancel_return_url=" + sUrlKo + " ";
            var baseUri = new Uri("http://" + PortalSettings.Current.PortalAlias.HTTPAlias);
            var absoluteUri = new Uri(baseUri, "/DesktopModules/NBright/OS_Sips/notify.ashx");
            parms += "automatic_response_url=" + absoluteUri + " ";
            parms += "language=" + orderData.Lang.Substring(0, 2) + " ";


            if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_SipsApiparams.html", parms);
            }

            var exepath = controlMapPath.TrimEnd('\\') + "\\sipsbin\\request.exe";
            var sipsdata = CallSipsExec(exepath, parms);

            var tableau = sipsdata.Split('!');

            string code = "";
            code = tableau[1];
            string errorMsg = tableau[2];

            if (code.Equals("") | code.Equals("-1")) return errorMsg;

            var htmlOutput = tableau[3];
            var aryResult = htmlOutput.Split('"');
            var sipsUrl = aryResult[1];
            var sipsData = aryResult[5];


            rPost.Url = sipsUrl;
            rPost.Add("DATA", sipsData); // must be uppercase.
            // just force CB payment. (CB must be in the list of payment_means)
            rPost.Add("CB.x", "5");
            rPost.Add("CB.y", "5");

            // save here (may chnage lang field)
            orderData.AddAuditMessage(transid, "sisptransid", "sipsapi", "True");
            orderData.Save();

            //Build the re-direct html 
            var rtnStr = rPost.GetPostHtml();
            if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_SipsApihtmlOutput.html", htmlOutput);
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_SipsApipost.html", rtnStr);
            }
            return rtnStr;
        }


        public static string CallSipsExec(string sipsExecPath, string Params)
        {
            string strData = "";
            string strErr = "";

            // Set start information.
            ProcessStartInfo start_info = new ProcessStartInfo(sipsExecPath);
            start_info.Arguments = Params;
            start_info.UseShellExecute = false;
            start_info.CreateNoWindow = true;
            start_info.RedirectStandardOutput = true;
            start_info.RedirectStandardError = true;

            // Make the process and set its start information.
            Process proc = new Process();
            proc.StartInfo = start_info;

            // Start the process.
            proc.Start();

            // Attach to stdout and stderr.
            StreamReader stdOut = proc.StandardOutput;
            StreamReader stdErr = proc.StandardError;

            strData = stdOut.ReadToEnd();
            strErr = stdErr.ReadToEnd();

            // Clean up.
            stdOut.Close();
            stdErr.Close();
            proc.Close();

            var objCtrl = new NBrightBuyController();
            var settings = objCtrl.GetPluginSinglePageData("OS_Sipspayment", "OS_SipsPAYMENT", Utils.GetCurrentCulture());
            if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_NBrightBuySipsData.html", strData + " Err:" + strErr + " Params:" + Params);
            }

            return strData;

        }

    }
}
