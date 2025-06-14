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

Console.WriteLine("Welcome to the CG25 Hackathon hunt reward printer!");
Console.Write("\r\nPlease enter the number of the next voucher to print (default 1): ");

if (!int.TryParse(Console.ReadLine(), out var voucherNumber))
{
    voucherNumber = 1;
}

using var printer = new POS58DPrinterService(printerConfiguration);
while (voucherNumber <= allowedVouchers)
{
    Console.Write("\r\nEnter next winner's name: ");
    var winnerName = Console.ReadLine();
    try
    {
        Console.WriteLine($"Printing voucher {voucherNumber} for {winnerName}");
        printer.PrintVoucher(winnerName, voucherNumber);
        voucherNumber++;
    }
    catch (Exception e)
    {
        Console.WriteLine($"\r\nWOOPS!! Something went wrong when printing voucher {voucherNumber} for {winnerName}\r\n{e.Message}\r\n\r\n");
    }
}

Console.WriteLine("\r\nSorry, no more vouchers available!");
Console.ReadLine();
