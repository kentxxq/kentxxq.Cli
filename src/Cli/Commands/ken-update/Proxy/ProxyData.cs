using System.Collections.Generic;

namespace Cli.Commands.ken_update;


public enum ProxyEnum
{
    Ghproxy,
    Fastgit,
    Cfworker
}


// public static class Proxy
// {
//     public static Dictionary<ProxyEnum, string> Data = new()
//     {
//         { ProxyEnum.Ghproxy, "https://ghproxy.com/" },
//         { ProxyEnum.Fastgit, "https://download.fastgit.org/" },
//         { ProxyEnum.Cfworker ,"https://github.abskoop.workers.dev/"}
//     };
// }