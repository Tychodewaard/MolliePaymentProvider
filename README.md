# DnnCMollie
Payment Gateway for Mollie

## To install
First nstall the zip package as you would install any package. It's in the _installation folder.

Second, go to NBB Backoffice and navigate to Admin, plugins.
Add a new plugin, and enter the following values:
| Label           | Value                                           |
|-----------------|-------------------------------------------------|
| Plugin Ref      | molliepayment                                   |
| Default Name    | Mollie Payment                                  |
| Group           | Admin                                           |
| Path to Control | /DesktopModules/NBright/DnnCMollie/Payment.ascx |
| Security Roles  | Admin                                           |

and for Plugin Provider settings:
| Label               | Value                                 |
|---------------------|---------------------------------------|
| Provider type       | Payments                              |
| Assembly            | DnnC.MolliePaymentProvider            |
| Namespace and class | DnnC.Mollie.DnnCMolliePaymentProvider |
| Active              | Check to activate                     |

Third, by now, you should have an Admin-->Mollie Payment option in the backoffice menu. There you will need to set your Mollie API key and wether or not to use test mode.

After that, you should be good to go.

