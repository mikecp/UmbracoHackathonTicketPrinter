// See https://aka.ms/new-console-template for more information
using MikeCp.Umbraco.HackathonIssuePrinter.PrinterService.POS58D;

Console.WriteLine("Hello, World!");

using (var printer = new POS58DPrinterService())
{
    printer.Print(new
                    ("12348",
                    "Test frolm console",
                    string.Empty,
                    "Umbraco-CMS",
                    "mikecp",
                    "https://github.com/umbraco/Umbraco-CMS/pull/9888"
                    ));
}