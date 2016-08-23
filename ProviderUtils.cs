using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;


using DnnC.Mollie.Api;

namespace DnnC.Mollie
{
    public class ProviderUtils
    {


        public static String GetTemplateMollieData(String templatename, NBrightInfo pluginInfo)
        {
            var templ = GetTemplateMollieData(templatename, pluginInfo);

            #region "Get Mollie options from API"

            var info = ProviderUtils.GetProviderSettings("DnnCMolliepayment");

            var testMode = info.GetXmlPropertyBool("genxml/checkbox/testmode");
            var testApiKey = info.GetXmlProperty("genxml/textbox/testapikey");
            var liveApiKey = info.GetXmlProperty("genxml/textbox/liveapikey");


            // Check to see if the test api keys is filled in, stops the error with the settings in the backoffice
            if (testApiKey != "")
            {

                var apiKey = testApiKey;

                if (!testMode)
                {
                    apiKey = liveApiKey;
                }

                MollieClient mollieClient = new MollieClient();
                mollieClient.setApiKey(apiKey);

                var strPayOptions = "";
                PaymentMethods methods = mollieClient.GetPaymentMethods();
                Issuers issuers = mollieClient.GetIssuers();

                foreach (PaymentMethod method in methods.data)
                {
                    strPayOptions += "<tr>";
                    strPayOptions += "<td><input type='radio' id='" + method.id + "' value='" + method.id + "' name='group1' class='rdoBanks' /></td>";
                    strPayOptions += "<td><img src='" + method.image.normal + "' /></td>";
                    strPayOptions += "<td><strong>" + method.description + "</strong></td>";

                    strPayOptions += "<td>";
                    if (method.id == "ideal")
                    {

                        strPayOptions += "<select id='mollieidealgatewaybankselectordropdown' name='mollieidealgatewaybankselectordropdown'>";
                        foreach (Issuer issuer in issuers.data)
                        {
                            strPayOptions += string.Format("<option value=\"{0}\">{1}</option>", issuer.id, issuer.name);
                        }
                        strPayOptions += "</select>";

                    }
                    strPayOptions += "</td>";


                    strPayOptions += "</tr>";
                    strPayOptions += "<tr><td colspan='4'><hr/></td></tr>";
                }
                templ = templ.Replace("[PAYMENTMETHODS]", strPayOptions);
            }
            #endregion

            return templ;
        }

        public static String GetTemplateData(String templatename, NBrightInfo pluginInfo)
        {
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/DnnCMollie");
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\config", "");
            var templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());
            templ = Utils.ReplaceSettingTokens(templ, pluginInfo.ToDictionary());
            templ = Utils.ReplaceUrlTokens(templ);

            return templ;
        }

        public static NBrightInfo GetProviderSettings(String ctrlkey)
        {
            var info = (NBrightInfo)Utils.GetCache("DnnCMolliePaymentProvider" + PortalSettings.Current.PortalId.ToString(""));
            if (info == null)
            {
                var modCtrl = new NBrightBuyController();

                info = modCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "DnnCMolliePAYMENT", ctrlkey);

                if (info == null)
                {
                    info = new NBrightInfo(true);
                    info.GUIDKey = ctrlkey;
                    info.TypeCode = "DnnCMolliePAYMENT";
                    info.ModuleId = -1;
                    info.PortalId = PortalSettings.Current.PortalId;
                }

                Utils.SetCache("DnnCMolliePaymentProvider" + PortalSettings.Current.PortalId.ToString(""), info);
            }

            return info;
        }

        public static String GetBankRemotePost(OrderData orderData)
        {


            var rPost = new RemotePost();

            var settings = ProviderUtils.GetProviderSettings("DnnCMolliepayment");

            var payData = new PayData(orderData);

            rPost.Url = orderData.PurchaseInfo.GetXmlProperty("genxml/posturl");
            //File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_DnnCMolliepost2.html", rPost.Url);
            //rPost.Add("param", "param");


            //Build the re-direct html 
            var rtnStr = rPost.GetPostHtml("/DesktopModules/NBright/DnnCMollie/Themes/config/img/cic.jpg");
            if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_DnnCMolliepost.html", rtnStr);
            }
            return rtnStr;
        }


        public static string getStatusCode(OrderData oInfo, HttpRequest request)
        {

            var result = "00";

            var payData = new PayData(oInfo);

            // do code to calculate staus code.


            return result;
        }

    }
}
