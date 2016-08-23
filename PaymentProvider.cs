using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;


using DnnC.Mollie.Api;
using System.IO;
using System.Globalization;


namespace DnnC.Mollie
{
    public class DnnCMolliePaymentProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.PaymentsInterface
    {
        public override string Paymentskey { get; set; }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            var info = ProviderUtils.GetProviderSettings("DnnCMolliepayment");
            var templ = ProviderUtils.GetTemplateData(info.GetXmlProperty("genxml/textbox/checkouttemplate"), info);

            return templ;
        }

        public override string RedirectForPayment(OrderData orderData)
        {

            var appliedtotal = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedsubtotal");
            var alreadypaid = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");

            var info = ProviderUtils.GetProviderSettings("DnnCMolliepayment");

            var cartDesc = info.GetXmlProperty("genxml/textbox/cartdescription");
            var testMode = info.GetXmlPropertyBool("genxml/checkbox/testmode");
            var testApiKey = info.GetXmlProperty("genxml/textbox/testapikey");
            var liveApiKey = info.GetXmlProperty("genxml/textbox/liveapikey");
            var notifyUrl = Utils.ToAbsoluteUrl("/DesktopModules/NBright/DnnCMollie/notify.ashx");
            var returnUrl = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "");
            var ItemId = orderData.PurchaseInfo.ItemID.ToString("");


            //File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_DnnCMolliepost.html", rtnStr);
            //var chkIdeal = 

            var apiKey = testApiKey;

            var errText = apiKey + "||<br/>" + cartDesc + "||<br/>" + testMode + "||<br/>" + testApiKey + "||<br/>" + liveApiKey + "||<br/>WebHook : " + notifyUrl + "||<br/> ReturnUrl : " + returnUrl + "||<br/>ItemId : " + ItemId + "||<br/>";
            File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_DnnCErrors.html", errText);

            if (!testMode)
            {
                apiKey = liveApiKey;
            }

            MollieClient mollieClient = new MollieClient();
            mollieClient.setApiKey(apiKey);

            Payment payment = new Payment
            {
                amount = decimal.Parse((appliedtotal - alreadypaid).ToString("0.00", CultureInfo.InvariantCulture)), //99.99M,
                description = cartDesc,
                redirectUrl = returnUrl,
                method = Method.mistercash,
                metadata = "OrderId = " + ItemId,
                //webhookUrl = notifyUrl,

            };
            PaymentStatus paymentStatus = mollieClient.StartPayment(payment);


            //File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_DnnCMolliepost1.html", paymentStatus.links.paymentUrl);
            //
            //ReturnUrl = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
            //"orderid=" + oInfo.PurchaseInfo.ItemID.ToString("");





            orderData.OrderStatus = "020";
            orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "");
            orderData.PurchaseInfo.SetXmlProperty("genxml/posturl", paymentStatus.links.paymentUrl);
            orderData.PurchaseInfo.Lang = Utils.GetCurrentCulture();
            orderData.SavePurchaseData();
            try
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write(ProviderUtils.GetBankRemotePost(orderData));
            }
            catch (Exception ex)
            {
                // rollback transaction
                orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "<div>ERROR: Invalid payment data </div><div>" + ex + "</div>");
                orderData.PaymentFail();
                var param = new string[3];
                param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                param[1] = "status=0";
                return Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
            }

            try
            {
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                // this try/catch to avoid sending error 'ThreadAbortException'  
            }

            return "";
        }

        public override string ProcessPaymentReturn(HttpContext context)
        {
            var orderid = Utils.RequestQueryStringParam(context, "orderid");
            if (Utils.IsNumeric(orderid))
            {
                var status = Utils.RequestQueryStringParam(context, "status");
                if (status == "0")
                {
                    var orderData = new OrderData(Convert.ToInt32(orderid));
                    var rtnerr = orderData.PurchaseInfo.GetXmlProperty("genxml/paymenterror");
                    if (rtnerr == "") rtnerr = "fail"; // to return this so a fail is activated.

                    // check we have a waiting for bank status (IPN may have altered status already)
                    if (orderData.OrderStatus == "020") orderData.PaymentFail();

                    return rtnerr;
                }
            }
            return "";
        }

    }
}
