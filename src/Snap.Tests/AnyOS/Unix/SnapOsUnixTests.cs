﻿#if PLATFORM_UNIX
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Snap.AnyOS;
using Snap.AnyOS.Unix;
using Snap.Core;
using Snap.Core.IO;
using Snap.Reflection;
using Snap.Shared.Tests;
using Snap.Shared.Tests.Extensions;
using Xunit;

namespace Snap.Tests.AnyOS.Unix
{
    public class SnapOsUnixTests : IClassFixture<BaseFixture>
    {
        readonly BaseFixture _baseFixture;
        readonly ISnapFilesystem _snapFilesystem;
        readonly ISnapOs _snapOs;
        readonly SnapOsUnix _snapOsUnix;

        public SnapOsUnixTests(BaseFixture baseFixture)
        {
            _baseFixture = baseFixture;
            _snapFilesystem = new SnapFilesystem();
            _snapOsUnix = new SnapOsUnix(_snapFilesystem, new SnapOsProcessManager(), new SnapOsSpecialFoldersUnix());
            _snapOs = new SnapOs(_snapOsUnix);
        }
       
        [Fact]
        public void TestDistroType()
        {
            // If this test case fails then please submit a PR that returns the correct unix distro type :)
            Assert.Equal(SnapOsDistroType.Ubuntu, _snapOs.DistroType);
        }

        [Fact]
        public async Task TestParseLsbRelease()
        {
            var lsbRelease = @"
No LSB modules are available.
Distributor ID:	Ubuntu
Description:	Ubuntu 18.10
Release:	18.10
Codename:	cosmic";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var (exitCode, maybeLsbRelease) = await _snapOs.OsProcess.RunAsync("lsb_release", "-a", CancellationToken.None);
                if (exitCode == 0 && !string.IsNullOrWhiteSpace(maybeLsbRelease))
                {
                    lsbRelease = maybeLsbRelease;
                }
            } 

            var (distributorId, description, release, codeName) = _snapOsUnix.ParseLsbRelease(lsbRelease);
            Assert.Equal("Ubuntu", distributorId);
            Assert.Equal("Ubuntu 18.10", description);
            Assert.Equal("18.10", release);
            Assert.Equal("cosmic", codeName);
        }
        
#if PLATFORM_UNIX
        [Fact]
        public async Task TestSnapOsUnixNativeMethods_Chmod()
        {
            await _snapFilesystem.FileWriteUtf8StringAsync("yolo", "test.txt", CancellationToken.None);

            Assert.Equal(0,NativeMethodsUnix.chmod("test.txt", 755));            
        }

        [Fact]
        public async Task TestGetProcessesAsync()
        {
            var processes = await _snapOs.GetProcessesAsync(CancellationToken.None);
            Assert.NotEmpty(processes);
        }
#endif

        [Fact]
        public void TestSpecialFolders()
        {
            Assert.NotEmpty(_snapOs.SpecialFolders.ApplicationData);
            Assert.NotEmpty(_snapOs.SpecialFolders.LocalApplicationData);
            Assert.NotEmpty(_snapOs.SpecialFolders.DesktopDirectory);
            Assert.NotEmpty(_snapOs.SpecialFolders.StartupDirectory);
            Assert.NotEmpty(_snapOs.SpecialFolders.StartMenu);
            Assert.NotEmpty(_snapOs.SpecialFolders.InstallerCacheDirectory);
        }

    }
          
}
#endif
