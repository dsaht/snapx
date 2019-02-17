﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using snapx.Options;
using Snap.Core;
using Snap.Core.Models;
using Snap.Extensions;
using Snap.Logging;
using Snap.NuGet;

namespace snapx
{
    internal partial class Program
    {
        static async Task<int> CommandPromoteAsync([NotNull] PromoteNupkgOptions opts, [NotNull] ISnapFilesystem filesystem,
            [NotNull] ISnapAppReader appReader, [NotNull] INuGetPackageSources nuGetPackageSources, [NotNull] INugetService nugetService,
            [NotNull] ILog logger, [NotNull] string workingDirectory, CancellationToken cancellationToken)
        {
            if (opts == null) throw new ArgumentNullException(nameof(opts));
            if (filesystem == null) throw new ArgumentNullException(nameof(filesystem));
            if (appReader == null) throw new ArgumentNullException(nameof(appReader));
            if (nuGetPackageSources == null) throw new ArgumentNullException(nameof(nuGetPackageSources));
            if (nugetService == null) throw new ArgumentNullException(nameof(nugetService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (workingDirectory == null) throw new ArgumentNullException(nameof(workingDirectory));
            if (opts == null) throw new ArgumentNullException(nameof(opts));
            if (nugetService == null) throw new ArgumentNullException(nameof(nugetService));

            var (snapApps, snapApp, error, snapsManifestAbsoluteFilename) = BuildSnapAppFromDirectory(filesystem, appReader,
                nuGetPackageSources, opts.AppId, opts.Rid, workingDirectory);
            if (error)
            {
                logger.Error($"Snap with id {opts.AppId} was not found in manifest: {snapsManifestAbsoluteFilename}");
                return -1;
            }

            var remainingChannels = snapApp.Channels.Skip(1).Select(x => x.Name).ToList();
            if (!remainingChannels.Any())
            {
                logger.Error($"Unable to promote snap with id: {opts.AppId} because it only has one channel: {snapApps.Channels.First().Name}");
                return -1;
            }

            if (opts.Channel != null)
            {
                remainingChannels = remainingChannels.Where(x => string.Equals(x, opts.Channel, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!remainingChannels.Any())
                {
                    logger.Info($"Unable to promote snap with id: {opts.AppId}. Channel name was not found: {opts.Channel}");
                    return -1;
                }
            }

            logger.Info($"Promoting to channels: {string.Join(", ", remainingChannels)}.");

            var promotableSnapApps = new List<(SnapApp snapApp, SnapChannel channel, string upstreamPackageId)>();

            foreach (var channelName in remainingChannels)
            {
                var promoteableSnapApp = new SnapApp(snapApp);
                promoteableSnapApp.Channels.ForEach(x => x.Current = false);
                var promoteableSnapAppChannel = promoteableSnapApp.Channels.Single(x => x.Name == channelName);
                promoteableSnapAppChannel.Current = true;
                promotableSnapApps.Add((promoteableSnapApp, promoteableSnapAppChannel, promoteableSnapApp.BuildNugetUpstreamPackageId()));
            }

            foreach (var (_, _, upstreamPackageId) in promotableSnapApps)
            {
                logger.Info($"Retrieving most version for nupkg: {upstreamPackageId}.");
                var mostRecentMedatadata = await SnapUtility.Retry(async() =>
                    await nugetService.FindByMostRecentPackageIdAsync(
                        upstreamPackageId, nuGetPackageSources, cancellationToken));

                if (mostRecentMedatadata != null)
                {
                    logger.Error($"Nupkg is already published: {upstreamPackageId}");
                    return -1;
                }

                logger.Info($"Upstream version: {mostRecentMedatadata.Identity.Version}.");
                logger.Info("Downloading nupkg...");
            }

            return -1;
        }

    }
}
