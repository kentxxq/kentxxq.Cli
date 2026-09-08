using System.CommandLine;
using Cli.Commands;
using Cli.Utils;

Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

try
{
    return await AllCommands.BuildCommandLine().Parse(args).InvokeAsync(
        new InvocationConfiguration { EnableDefaultExceptionHandler = false });
}
catch (OperationCanceledException)
{
    MyAnsiConsole.MarkupWarningLine("操作已取消。");
    return 130;
}
catch (Exception exception)
{
    MyAnsiConsole.MarkupErrorLine($"操作失败：{exception.GetType().Name}。请检查参数、网络和访问权限。");
    return 1;
}
