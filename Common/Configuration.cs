using System;
using System.Collections.Generic;
using System.Linq;

namespace Crypto
{
    public class Configuration
    {
        public static bool IsDocker()
        {
            string result = Common.Shell("ls /.dockerenv");
            return result.StartsWith("/.dockerenv");
        }

        public static bool IsLinux()
        {
            return
                System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Linux
                );
        }
    }
}
