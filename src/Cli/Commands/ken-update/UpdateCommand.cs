using System.CommandLine;

namespace Cli.Commands.ken_update;

public static class UpdateCommand
{
    public static Command GetCommand()
    {
        var command = new Command("update", "update ken command");
        command.SetHandler(Run);
        return command;
    }

    private static void Run()
    {
        // 检查当前版本
        // 检查github上面的版本
        // 下载对应最新的cli
        // 移动当前的版本，将新版本cli放到现有的位置
    }
}