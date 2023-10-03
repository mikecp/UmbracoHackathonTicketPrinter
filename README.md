# UmbracoHackathonTicketPrinter

The initial scope of this project was to build a gimmick for the Umbraco Codegarden 23 Hackathon: we wanted to print receipts tickets containing information about issues in the Umbraco repository that would get labeled with a specific "hackathon-up-for-grabs" label.  

You can find all details about the genesis of this project and the choices we made in the October 23 edition of the online [Skrift](https://skrift.io/) magazine. You also get explainations on how you can integrate it without too much work  in your own hackathons, events or even at work!  
Hence the going public of this repository together with the publication of the article 😊

TLDR: Here are some quick hints for using and configuring the solution to your needs.

## Finding the USB port

In Windows you can run `Get-PrinterPort` in powershell to have a look at your ports in use. You should see a printer configured on `USB001` (number may vary) when you have the printer plugged in to your local PC.

## Using with Webhooks

If you don't use the GitHub personal access token technique then you don't need to fill one in in the `appsettings.json` file. Instead you run the app and open it up to the web using the brilliant software `ngrok`.

The `ngrok` app has a recently new option to give you a permanent URL for free (a Cloud edge). It allows you to configure the GitHub webhook validation with the password you set up when you create the webhook on GitHub.

![image](https://github.com/mikecp/UmbracoHackathonTicketPrinter/assets/304656/e90fad02-5cb7-44b6-810d-28c80c2d41ed)

Don't forget to tap "Save" in the top right.

The instructions tell you to then run a command like this once:

`ngrok config add-authtoken {the-auth-token-given-by-the-instructions}`

And then you can always start the tunnel with it's identifyer: 

`ngrok tunnel --label edge={identifier} http://localhost:5120`

Please note that your API should be running locally on http, becaue https is not available for free ngrok accounts (the public forwarding URL is always in https though)

In order to set up the webhook in GitHub, go to the Webhook settings on your repository (`https://github.com/{your-user-name}/{your-repository-name}/settings/hooks`) and choose the following:

- Payload URL: `https://{your-prefix}.ngrok-free.app/printGitHubLabeledIssueTicket
- Content type: `application/json`
- Secret: `{the password you just set in ngrok}`
- SSL verification Enabled
- Let me select individual events ==> Choose "Issues"

Then tap "Add webhook" to finish up.

## Changing the output

In `~/MikeCp.Umbraco.HackathonIssuePrinter.PrinterService/POS58D/POS58DPrinterService.cs` you are able to tweak the text and images you want to print. 

### Bitmaps

If you want to add images, they can be dropped into `~/MikeCp.Umbraco.HackathonIssuePrinter.PrinterService/Assets`. 

You might need to play with the image files a little bit to get them to output nicely. In Microsoft Paint you can do a "Save As" and select 16 bit color output, that seems to give good results.

## Labels to listen to

In `appsettings.json` you will find a list of `LabelsToProcess`, if any of these labels get added on an issue then the print will happen, otherwise actual printing will be skipped.
