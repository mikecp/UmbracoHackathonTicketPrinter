namespace MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;

public sealed class POS58DPrinterService : IPrinterService, IDisposable
{
    private int _printerId;
    private PrinterConfiguration _config;

    public string PrinterType => "RG-P58D";

    public POS58DPrinterService(PrinterConfiguration config)
    {
        _config = config;
        _printerId = 0;
        OpenConnection();
    }

    public void Print(IssueDocument issue)
    {
        var tick = Environment.TickCount;
 
        PrintSDK.CON_PageStart(_printerId, false, 0, 0);
        PrintSDK.ASCII_CtrlReset(_printerId);

        // Global alignment is centered
        PrintSDK.ASCII_CtrlAlignType(_printerId, AlignType.ALIGN_CENTER);
        
        // Print Issue Id
        PrintSDK.ASCII_CtrlCharSize(_printerId,3, 3);
        PrintSDK.ASCII_PrintText(_printerId, $"#{issue.Id}\r\n\r\n");
       
        // Print Issue Title
        PrintSDK.ASCII_CtrlCharSize(_printerId, 2, 2);
        PrintSDK.ASCII_PrintText(_printerId, $"{issue.Title}\r\n\r\n");
        
        // Print Issue details
        PrintSDK.ASCII_CtrlCharSize(_printerId, 1, 1);
        PrintSDK.ASCII_PrintText(_printerId, "Issue created by ");
        
        PrintSDK.ASCII_CtrlFormatString(_printerId, false, true, false, false, false);
        PrintSDK.ASCII_PrintText(_printerId, issue.Author);
        PrintSDK.ASCII_CtrlFormatString(_printerId, false, false, false, false, false);

        PrintSDK.ASCII_PrintText(_printerId, $" for repository ");

        PrintSDK.ASCII_CtrlFormatString(_printerId, false, true, false, false, false);
        PrintSDK.ASCII_PrintText(_printerId, issue.Source);
        PrintSDK.ASCII_CtrlFormatString(_printerId, false, false, false, false, false);

        PrintSDK.ASCII_PrintText(_printerId, $" was marked as up for grabs\r\n\r\n");
        PrintSDK.ASCII_PrintText(_printerId, "Scan to start working on it:\r\n\r\n");

        // Print QR code with link to issue
        PrintSDK.ASCII_Print2DBarcode(_printerId, 2, issue.Link, 4, 1, 6);

        // Print Footer
        PrintSDK.ASCII_CtrlAlignType(_printerId, AlignType.ALIGN_LEFT);
        PrintSDK.CON_PrintFile(_printerId, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\umbraco.bmp"));
        PrintSDK.ASCII_CtrlFormatString(_printerId, false, true, false, false, false);
        PrintSDK.ASCII_PrintText(_printerId, "CG23    ");
        PrintSDK.ASCII_CtrlFormatString(_printerId, false, false, false, false, false);
        PrintSDK.ASCII_PrintText(_printerId, string.Format("{0:G}", DateTime.Now) + "\r\n\r\n");
        PrintSDK.ASCII_CtrlAlignType(_printerId, AlignType.ALIGN_CENTER);
        PrintSDK.CON_PrintFile(_printerId, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets\\our_heart.bmp"));

        // End of doc
        PrintSDK.ASCII_CtrlCutPaper(_printerId, CutType.CT_HALF_CUT, 0);
        PrintSDK.CON_PageEnd(_printerId);

        // Handle actual printing (quasi copy-paste from SDK demo sample)
        var bSucc = false;
        var strErr = "";
        switch (PrintSDK.CON_PageSend(_printerId))
        {
            case 0:
                strErr = "CON_PageSend failure(IO failure),spend:{0:G}(ms)\r\n";
                break;
            case 1:
                strErr = "CON_PageSend failure(unverify),spend:{0:G}(ms)\r\n";
                break;
            case 2:
                {
                    switch (PrintSDK.CON_QueryPrintStatus(_printerId, 8000))
                    {
                        case 0:
                            strErr = "CON_QueryPrintStatus failure(IO error),spend:{0:G}(ms)\r\n";
                            break;
                        case 1:
                            strErr = "CON_QueryPrintStatus failure(overtime),spend:{0:G}(ms)\r\n";
                            break;
                        case 2:
                            bSucc = true;
                            break;
                    }
                }
                break;
        }
        var strMess = String.Format(strErr, Environment.TickCount - tick);

        if (!bSucc)
        {
            switch (PrintSDK.CON_QueryStatus(_printerId))
            {
                case 0:
                    bSucc = true;
                    break;
                case 1:
                    strMess += "\r\nAfter a mistake CON_QueryStatus:out of paper";
                    break;
                case 2:
                    strMess += "\r\nAfter a mistake CON_QueryStatus:paper will done";
                    break;
                case 3:
                    strMess += "\r\nAfter a mistake CON_QueryStatus:printer not connect";
                    break;
            }
        }
        
        if (!bSucc)
        {
            throw new IOException(strMess);
        }
    }

    private void OpenConnection()
    {
        // We need to load the list of USB Poerts prior to connecting to it...
        var btUSBList = new byte[256];
        PrintSDK.PT_GetPorts(0, btUSBList, 256);
    
        var portName = _config.IP ?? _config.USB ?? string.Empty;

        var iRet = PrintSDK.CON_ConnectDevices(PrinterType, portName, 1000);

        if (iRet != 0)
        {
            _printerId = iRet;
        }
        else
        {
            throw new IOException("Port open failure");
        }
   }

    private void CloseConnection()
    {
        if (_printerId != 0)
        {
            PrintSDK.CON_CloseDevices(_printerId);
            _printerId = 0;
        }
    }

    public void Dispose()
    {
        CloseConnection();
    }
}
