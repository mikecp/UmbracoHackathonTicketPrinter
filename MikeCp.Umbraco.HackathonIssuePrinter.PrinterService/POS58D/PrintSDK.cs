using System;
using System.Runtime.InteropServices;

namespace MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;

public enum AlignType
{
    ALIGN_LEFT = 0,
    ALIGN_CENTER,
    ALIGN_RIGHT
}

public enum CutType
{
    CT_FULL_CUT = 65,
    CT_HALF_CUT = 66
}

class PrintSDK
{
    #region 打印流程控制
    /// <summary>
    /// 获取SDK版本号
    /// </summary>
    /// <param name="version">传出，调用者分配缓存SDK版本，请参考示例</param>
    /// <returns>false获取失败; true获取成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_GetSDKVersion(byte[] version);

    /// <summary>
    /// 获取支持打印机名称
    /// </summary>
    /// <param name="lpPrinters">传出，调用者分配缓存，请参考示例</param>
    /// <param name="len">缓存大小</param>
    /// <returns>false获取失败; true获取成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_GetSupportPrinters(byte[] lpPrinters, Int32 len);

    /// <summary>
    /// 连接打印机
    /// </summary>
    /// <param name="prtName">打印机名称,参考"CON_GetSupportPrinters"</param>
    /// <param name="port">端口名称</param>
    /// <param name="timeout">连接超时时间，单位毫秒</param>
    /// <returns>==0,连接失败; !=连接成功，为下一次调用的句柄</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_ConnectDevices(string prtName, String port, int timeout);

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_CloseDevices(int objCode);

    /// <summary>
    /// 查询打印机状态
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>
    /// 0 打印机状态正常
    /// 1 打印机无纸
    /// 2 打印机纸将尽(只在某些机型下有效如:RG-K532 且需要另配纸将近传感器)
    /// 3 打印机未连接
    /// 4 打印机未验证
    /// 5 打印机纸仓盖被打开
    /// 6 打印机切刀错误
    /// </returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_QueryStatus(int objCode);

    /// <summary>
    /// 查询上一单打印任务是否完成
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="timeout">超时时间单位毫秒，应大于从发送到打印完成总体用时时间。如打印一单需要2s，那这里值应为4000~5000</param>
    /// <returns>
    /// 0 端口操作失败，在端口收发数据错误
    /// 1 超时未完成打印
    /// 2 打印成功完成
    /// </returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_QueryPrintStatus(int objCode, int timeout);

    /// <summary>
    /// 页打印开始，普通热敏打印机
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="graphicMode">图形模式选项: true图形模式; false文本模式</param>
    /// <param name="width">图形模式的宽度，为打印机可打印宽度。58毫米打印机宽度为384，80毫米打印机宽度为576; 文本模式输入0即可</param>
    /// <param name="height">图形模式最大打印高度; 文本模式输入0即可</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_PageStart(int objCode, bool graphicMode, int width, int height);

    /// <summary>
    /// 页结束，生成相应的可打印指令
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_PageEnd(int objCode);

    /// <summary>
    /// 发送打印内容
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>
    /// 0发送失败，端口IO操作失败
    /// 1错误，打印机反馈错误
    /// 2成功，打印任务发送完成
    /// </returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_PageSend(int objCode);

    /// <summary>
    /// 打印文件
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="szPath">文件路径，支持图形文件、文本文件</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_PrintFile(int objCode, string szPath);

    /// <summary>
    /// 打印图形buffer
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="width">图形宽度</param>
    /// <param name="height">图形高度</param>
    /// <param name="buffer">图形缓存</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_PrintBMPBuffer(int objCode, int width, int height, byte[] buffer);

    /// <summary>
    /// 查询打印机固件版本
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="szFirmware">版本缓存内容</param>
    /// <param name="len">缓存区数据大小</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_QueryPrinterFirmware(int objCode, byte[] szFirmware, int len);

    /// <summary>
    /// 查询打印机序列号
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="szSerialNo">序列号缓存</param>
    /// <param name="len">缓存大小</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_QueryPrinterSerialNo(int objCode, byte[] szSerialNo, int len);

    /// <summary>
    /// 日志记录功能打开，用于调试场合
    /// </summary>
    /// <param name="path">日志保存路径</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_StartRecord(string path);

    /// <summary>
    /// 结束日志记录
    /// </summary>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_EndRecord();

    /// <summary>
    /// 让打印机重启，调用成功后需重连端口
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_Restart(int objCode);

    /// <summary>
    /// 下载Flash位图，需黑白位图，单张位图不能超过64k
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="iIndex">图片索引[0,9]</param>
    /// <param name="pszPath">图片路径</param>
    /// <returns>0端口未连接或文件打开失败;1图片尺寸大(小于64k);2不是黑白位图;3传输失败;4下载完成</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_DownloadBmp2Flash(int objCode, int iIndex, string pszPath);

    /// <summary>
    /// 打印Flash缓存位图
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="iIndex">图片索引</param>
    /// <returns>1操作成功; 0操作失败</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_PrintFlashBmp(int objCode, int iIndex);

    /// <summary>
    /// 获取连接的USB打印机
    /// </summary>
    /// <param name="iType">固定为0</param>
    /// <param name="Port">接口对象缓存</param>
    /// <param name="iLen">缓存大小</param>
    /// <returns>实际使用空间大小</returns>
    [DllImport("POS58D\\PortIO.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 PT_GetPorts(int iType, byte[] Port, int iLen);

    /// <summary>
    /// 获取图片缓存数据(CPCL指令封装)
    /// </summary>
    /// <param name="szPath">图片路径</param>
    /// <param name="posX">图片打印起始x轴</param>
    /// <param name="posY">图片打印起始y轴</param>
    /// <param name="size">输出：转化后的缓存大小</param>
    /// 返回值: 指令数据
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr CON_GetBMPBuffer(string szPath, int posX, int posY, out int size);

    #endregion

    #region 指令控制部分

    /// <summary>
    /// 初始化打印机
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlReset(int objCode);

    /// <summary>
    /// 黑标检测，必须跟在"CON_PageStart"后调用，支持文本、图形模式
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlBlackMark(int objCode);

    /// <summary>
    /// 文本模式下设置打印字体
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="bFont">改变字体 true:英文9x17 中文12x24; false:英文12x24 中文24x24</param>
    /// <param name="bThick">加粗模式 ture:加粗; false正常</param>
    /// <param name="bExWidth">一倍宽度</param>
    /// <param name="bExHeight">一倍高度</param>
    /// <param name="bUnderLine">下划线</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlFormatString(int objCode, bool bFont, bool bThick, bool bExWidth, bool bExHeight, bool bUnderLine);

    /// <summary>
    /// 打印文本
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="szText">要打印的文本</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_PrintText(int objCode, string szText);

    /// <summary>
    /// 一维条码打印效果
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="bcType">条码类型</param>
    /// <param name="iWidth">条码宽度[2,6]</param>
    /// <param name="iHeight">条码高度，像素点</param>
    /// <param name="hri">条码字符打印位置，0不打印1上方2下方3双方</param>
    /// <param name="strData">条码内容</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_Print1DBarcode(int objCode, int bcType, int iWidth, int iHeight, int hri, string strData);

    /// <summary>
    /// 打印二维码
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="type2D">二维码类型</param>
    /// <param name="strPrint">二维码内容</param>
    /// <param name="version">版本号，值越大二维码越大，默认为0表示自动[0,40]</param>
    /// <param name="ecc">纠错等级[1,4]</param>
    /// <param name="size">放大倍数[1,6]</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_Print2DBarcode(int objCode, int type2D, string strPrint, int version, int ecc, int size);

    /// <summary>
    /// 打印机切纸
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="cutWay">切刀方式65全切,66半切</param>
    /// <param name="postion">切纸完走纸距离</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlCutPaper(int objCode, CutType cutWay, int postion);

    /// <summary>
    /// 对齐方式
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="alignType">对齐方式</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlAlignType(int objCode, AlignType alignType);

    /// <summary>
    /// 打印机走纸行数
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="feed">多走纸点行数</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlFeedLines(int objCode, int feed);

    /// <summary>
    /// 设置字体放大倍数
    /// </summary>
    /// <param name="m_objID">端口连接成功后的句柄</param>
    /// <param name="width">宽度放大倍数[1,4]</param>
    /// <param name="height">高度放大倍数[1,4]</param>
    /// <returns></returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlCharSize(int m_objID, int width, int height);

    /// <summary>
    /// 设置打印位置和有效宽度
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="iLeftMargin">左起始位置点数</param>
    /// <param name="iWidth">有效打印宽度点数</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ASCII_CtrlPrintPosition(int objCode, int iLeftMargin, int iWidth);

    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CON_DirectIO(int iObjcode, byte[] btIn, out int inPut, byte[] btOut, out int outPut);

    #endregion

    #region 图形打印部分
    /// <summary>
    /// 绘图方式打印文本
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">x轴起始位置</param>
    /// <param name="y">y轴起始位置</param>
    /// <param name="text">文本内容</param>
    /// <param name="fntWidth">字体宽度</param>
    /// <param name="fntHeight">字体高度</param>
    /// <param name="fntName">字体名称</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 DRAW_PrintText(int objCode, int x, int y, string text, int fntWidth, int fntHeight, string fntName);

    /// <summary>
    /// 绘图方式打印二维码
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">x轴起始位置</param>
    /// <param name="y">y轴起始位置</param>
    /// <param name="nLevel">纠错等级[1,4]</param>
    /// <param name="nVersion">版本号[1,40]</param>
    /// <param name="szText">二维码内容</param>
    /// <param name="scale">放大、缩小倍数</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 DRAW_QRPrint(int objCode, int x, int y, int nLevel, int nVersion, string szText, float scale);

    /// <summary>
    /// 绘图方式打印Code128码
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">x轴起始位置</param>
    /// <param name="y">y轴起始位置</param>
    /// <param name="width">条码宽度[1,4]</param>
    /// <param name="height">条码高度</param>
    /// <param name="szText">条码内容</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 DRAW_Code128Print(int objCode, int x, int y, int width, int height, string szText);

    /// <summary>
    /// 设置文本字体，配合DRAW_PrintText2使用
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="width">字体宽度</param>
    /// <param name="height">字体高度</param>
    /// <param name="bold">是否加粗</param>
    /// <param name="italic">斜体</param>
    /// <param name="underline">下划线</param>
    /// <param name="fntName">字体名称</param>
    /// <returns></returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 DRAW_SetFont(int objCode, int width, int height, bool bold, bool italic, bool underline, string fntName);

    /// <summary>
    /// 绘图方式打印文本
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">x轴起始位置</param>
    /// <param name="y">y轴起始位置</param>
    /// <param name="text">文本内容</param>
    /// <returns></returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 DRAW_PrintText2(int objCode, int x, int y, string text);

    /// <summary>
    /// 绘图方式打印图片
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">x轴起始位置</param>
    /// <param name="y">y轴起始位置</param>
    /// <param name="pszPath">图形文件路径</param>
    /// <returns></returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 DRAW_PrintBmp(int objCode, int x, int y, string pszPath);

    #endregion

    #region CPCL标签打印，支持MLP80A,DTP112A

    /// <summary>
    /// CPCL页开始
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="width">标签宽度，像素点单位</param>
    /// <param name="height">标签高度，像素点单位</param>
    /// <param name="padLeft">左起始偏移量</param>
    /// <param name="preFeed">打印前退纸</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_PageStart(int objCode, int width, int height, int padLeft, int preFeed);

    /// <summary>
    /// CPCL打印文本
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">x轴起始位置</param>
    /// <param name="y">y轴起始位置</param>
    /// <param name="width">宽度放大倍数</param>
    /// <param name="height">高度放大倍数</param>
    /// <param name="rotate">旋转 90</param>
    /// <param name="font">字体模式</param>
    /// <param name="lpString">打印字符串</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_PrintText(int objCode, int x, int y, int width, int height, int rotate, int font, string lpString);

    /// <summary>
    /// CPCL页结束
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="tm">传输模式</param>
    /// <param name="postFeed">打印完多走纸行数</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_PageEnd(int objCode, int tm, int postFeed);

    /// <summary>
    /// 画线
    /// </summary>
    /// <param name="iObjCode">端口连接成功后的句柄</param>
    /// <param name="x0">x轴起始位置</param>
    /// <param name="y0">y轴起始位置</param>
    /// <param name="x1">x轴结束位置</param>
    /// <param name="y1">y轴结束位置</param>
    /// <param name="width">线条宽度</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_Line(int iObjCode, int x0, int y0, int x1, int y1, int width);

    /// <summary>
    /// 画矩形
    /// </summary>
    /// <param name="iObjCode">端口连接成功后的句柄</param>
    /// <param name="x0">x轴起始位置</param>
    /// <param name="y0">y轴起始位置</param>
    /// <param name="x1">x轴结束位置</param>
    /// <param name="y1">y轴结束位置</param>
    /// <param name="width"></param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_Rectangle(int iObjCode, int x0, int y0, int x1, int y1, int width);

    /// <summary>
    /// 打印字符串
    /// </summary>
    /// <param name="iObjCode">端口连接成功后的句柄</param>
    /// <param name="bold">加粗</param>
    /// <param name="inverse">反色</param>
    /// <param name="underline">下划线</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_FormatString(int iObjCode, int bold, int inverse, int underline);

    /// <summary>
    /// 设置字符间距
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="space">间距，像素点</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_FontSpace(int objCode, int space);

    /// <summary>
    /// 一位条码打印
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">起始位置x轴</param>
    /// <param name="y">起始位置y轴</param>
    /// <param name="width">条码宽度[0,4]</param>
    /// <param name="ratio">宽窄条比率[0,4]</param>
    /// <param name="height">条码高度</param>
    /// <param name="strType">条码类型
    /// UPCA,UPCE,EAN13,EAN8,39,CODABAR,93,128
    /// </param>
    /// <param name="strPrint">条码内容</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_Print1DBarcode(int objCode, int x, int y, int width, int ratio, int height, string strType, string strPrint);

    /// <summary>
    /// 打印二维码
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="type2D">二维码类型，固定为2</param>
    /// <param name="x">起始位置x轴</param>
    /// <param name="y">起始位置y轴</param>
    /// <param name="version">版本号[0,40]</param>
    /// <param name="ecc"></param>
    /// <param name="size"></param>
    /// <param name="strPrint"></param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 CPCL_Print2DBarcode(int objCode, int type2D, int x, int y, int version, int ecc, int size, string strPrint);

    #endregion

    #region ESC/P标签打印,支持LP561

    /// <summary>
    /// 651页模式打开
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="direct">打印方向[0,3]顺时针旋转90度的倍数</param>
    /// <param name="width">标签宽度</param>
    /// <param name="height">标签高度</param>
    /// <returns>0调用失败; 1调用成功</returns>
    //标签打印ESCP
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_PageStart(int objCode, int direct, int width, int height);

    /// <summary>
    /// 651页模式关闭
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_PageEnd(int objCode);

    /// <summary>
    /// 打印文本
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">起始位置x轴</param>
    /// <param name="y">起始位置y轴</param>
    /// <param name="underline">下划线</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="bold">加粗</param>
    /// <param name="font">字体模式</param>
    /// <param name="lpString">文本字符串</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_PrintText(int objCode, int x, int y, int underline, int width, int height, int bold, int font, string lpString);

    /// <summary>
    /// 画线
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x0">起始位置x轴</param>
    /// <param name="y0">起始位置y轴</param>
    /// <param name="x1">结束位置x轴</param>
    /// <param name="y1">结束位置y轴</param>
    /// <param name="width">线条宽度</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_Line(int objCode, int x0, int y0, int x1, int y1, int width);

    /// <summary>
    /// 画矩形
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x0">起始位置x轴</param>
    /// <param name="y0">起始位置y轴</param>
    /// <param name="x1">结束位置x轴</param>
    /// <param name="y1">结束位置y轴</param>
    /// <param name="width">线条宽度</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_Rectangle(int objCode, int x0, int y0, int x1, int y1, int width);

    /// <summary>
    /// 一位条码打印
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">起始位置x轴</param>
    /// <param name="y">起始位置y轴</param>
    /// <param name="height">条码高度</param>
    /// <param name="hri">条码字符打印位置0不打印 1上方 2下方</param>
    /// <param name="strPrint">条码内容</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_Print1DBarcode(int objCode, int x, int y, int height, int hri, string strPrint);

    /// <summary>
    /// 打印二维条码
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <param name="x">起始位置x轴</param>
    /// <param name="y">起始位置y轴</param>
    /// <param name="ecc">条码纠错等级[48,49,50,51]</param>
    /// <param name="size">条码大小[1,8]</param>
    /// <param name="strPrint">条码内容</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_PrintQRBarcode(int objCode, int x, int y, int ecc, int size, string strPrint);

    /// <summary>
    /// 画线结束
    /// </summary>
    /// <param name="objCode">端口连接成功后的句柄</param>
    /// <returns>0调用失败; 1调用成功</returns>
    [DllImport("POS58D\\PrintSDK.dll", CharSet = CharSet.Unicode)]
    public static extern Int32 ESC561_PrintLineEnd(int objCode);
    #endregion

}
