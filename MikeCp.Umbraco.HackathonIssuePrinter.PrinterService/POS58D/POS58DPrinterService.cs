using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;

public sealed class POS58DPrinterService : IPrinterService, IDisposable
{
    private int m_objID;

    public POS58DPrinterService()
    {
        m_objID = 0;
        OpenConnection();
    }

    public void Print(IssueDocument issue)
    {
        int preFeed = 0;
        int postFeed = 0;
        bool bSucc = false;
        string strErr = "";
        int tick = Environment.TickCount;
        if (preFeed != 0)
        {
            PrintSDK.CON_PageStart(m_objID, false, 0, 0);
            PrintSDK.ASCII_CtrlFeedLines(m_objID, preFeed);
            PrintSDK.CON_PageEnd(m_objID);
            PrintSDK.CON_PageSend(m_objID);
            if (preFeed < 0)
                Thread.Sleep(500);
        }

        PrintSDK.CON_PageStart(m_objID, false, 0, 0);
        PrintSDK.ASCII_CtrlReset(m_objID);

        // Global alignment is centered
        PrintSDK.ASCII_CtrlAlignType(m_objID, AlignType.ALIGN_CENTER);
        
        // Print Issue Id
        PrintSDK.ASCII_CtrlCharSize(m_objID,3, 3);
        PrintSDK.ASCII_PrintText(m_objID, $"#{issue.Id}\r\n\r\n");
       
        // Print Issue Title
        PrintSDK.ASCII_CtrlCharSize(m_objID, 2, 2);
        PrintSDK.ASCII_PrintText(m_objID, $"{issue.Title}\r\n\r\n");
        
        // Print Issue details
        PrintSDK.ASCII_CtrlCharSize(m_objID, 1, 1);
        PrintSDK.ASCII_PrintText(m_objID, "Issue created by ");
        
        PrintSDK.ASCII_CtrlFormatString(m_objID, false, true, false, false, false);
        PrintSDK.ASCII_PrintText(m_objID, issue.Author);
        PrintSDK.ASCII_CtrlFormatString(m_objID, false, false, false, false, false);


        PrintSDK.ASCII_PrintText(m_objID, $" for repository ");

        PrintSDK.ASCII_CtrlFormatString(m_objID, false, true, false, false, false);
        PrintSDK.ASCII_PrintText(m_objID, issue.Source);
        PrintSDK.ASCII_CtrlFormatString(m_objID, false, false, false, false, false);

        PrintSDK.ASCII_PrintText(m_objID, $" was marked as up for grabs\r\n\r\n");
        PrintSDK.ASCII_PrintText(m_objID, "Scan to start working on it:\r\n");

        // Print QR code with link to issue
        PrintSDK.ASCII_Print2DBarcode(m_objID, 2, issue.Link, 4, 1, 6);

        // End of doc
        PrintSDK.ASCII_CtrlCutPaper(m_objID, CutType.CT_HALF_CUT, 0);
        PrintSDK.CON_PageEnd(m_objID);
        
        // Handle actual printing (quasi copy-paste from SDK demo sample)
        bSucc = false;
        switch (PrintSDK.CON_PageSend(m_objID))
        {
            case 0:
                strErr = "CON_PageSend failure(IO failure),spend:{0:G}(ms)\r\n";
                break;
            case 1:
                strErr = "CON_PageSend failure(unverify),spend:{0:G}(ms)\r\n";
                break;
            case 2:
                {
                    switch (PrintSDK.CON_QueryPrintStatus(m_objID, 8000))
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
        string strMess = String.Format(strErr, System.Environment.TickCount - tick);

        if (!bSucc)
        {
            switch (PrintSDK.CON_QueryStatus(m_objID))
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
        // We need to load the liqst of USB Poerts prior to connecting to it...
        byte[] btUSBList = new byte[256];
        Int32 len1 = PrintSDK.PT_GetPorts(0, btUSBList, 256);
    
        Int32 iRet = 0;

        var portName = "USB1";
        var printerType = "RG-P58D";

        iRet = PrintSDK.CON_ConnectDevices(printerType, portName, 1000);

        if (iRet != 0)
        {
            m_objID = iRet;
            byte[] btVersion = new byte[256];
            Int32 len = PrintSDK.CON_QueryPrinterFirmware(m_objID, btVersion, 256);
            string str = Encoding.Unicode.GetString(btVersion, 0, (len) * 2);
            byte[] btSerial = new byte[256];
            len = PrintSDK.CON_QueryPrinterSerialNo(m_objID, btSerial, 256);
            str = Encoding.Unicode.GetString(btSerial, 0, (len) * 2);
        }
        else
        {
            throw new IOException("Port open failure");
        }
   }

    private void CloseConnection()
    {
        if (m_objID != 0)
        {
            PrintSDK.CON_CloseDevices(m_objID);
            m_objID = 0;
        }
    }

    public void Dispose()
    {
        CloseConnection();
    }
}
