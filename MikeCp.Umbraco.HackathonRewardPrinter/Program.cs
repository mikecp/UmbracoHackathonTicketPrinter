// See https://aka.ms/new-console-template for more information

using MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;
using Microsoft.Extensions.Configuration;
using MikeCp.Umbraco.HackathonIssuePrinter.PrinterService;

var config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json", false, true)
    .Build();

var printerConfigSection = config.GetSection("Printer");
var printerConfiguration = new PrinterConfiguration()
{
    USB = printerConfigSection["USB"],
    IP = printerConfigSection["IP"]
};

var allowedVouchers = int.Parse(config["AllowedVouchers"] ?? "30");

Console.Title = "CG25 Hackathon Reward Printer";
Console.ForegroundColor = ConsoleColor.DarkBlue;
Console.WriteLine("******************************************************");
Console.WriteLine("* Welcome to the CG25 Hackathon hunt reward printer! *");
Console.WriteLine("******************************************************");
Console.ForegroundColor = ConsoleColor.DarkYellow;
Console.Write("\r\nPlease enter the number of the next voucher to print (default 1): ");

if (!int.TryParse(Console.ReadLine(), out var voucherNumber))
{
    voucherNumber = 1;
}

Console.ResetColor();

using var log = new StreamWriter("./log.txt", true);  // Log to retrieve last printed voucher number in case of problem ;-)
using var printer = new POS58DPrinterService(printerConfiguration);
log.AutoFlush = true;
log.WriteLine("Starting CG25 Hackathon reward printer\r\n");
while (voucherNumber <= allowedVouchers)
{
    Console.Write("\r\nEnter next winner's name: ");
    var winnerName = Console.ReadLine();
    try
    {
        if (string.IsNullOrWhiteSpace(winnerName))
        {
            throw new ArgumentNullException(nameof(winnerName));
        }

        var msg = $"Printing voucher {voucherNumber} of {allowedVouchers} for {winnerName}";
        Console.WriteLine(msg);
        printer.PrintVoucher(winnerName, voucherNumber);
        voucherNumber++;
        log.WriteLine(msg);
    }
    catch (Exception e)
    {
        var errMsg = $"\r\nWOOPS!! Something went wrong when printing voucher {voucherNumber} for {winnerName}\r\n{e.Message}\r\n";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errMsg);
        log.WriteLine(errMsg);
        Console.ResetColor();
    }
}

Console.ForegroundColor = ConsoleColor.DarkYellow;
Console.WriteLine();
Console.WriteLine("**************************************");
Console.WriteLine("* SORRY, no more vouchers available! *");
Console.WriteLine("**************************************");
Console.ReadLine();
