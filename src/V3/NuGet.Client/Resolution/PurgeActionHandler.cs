﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Client.Resolution
{
    public class PurgeActionHandler : IActionHandler
    {
        public Task Execute(PackageAction action, ExecutionContext context, IExecutionLogger logger)
        {
            // Preconditions:
            Debug.Assert(!context.PackageManager.LocalRepository.IsReferenced(
                action.PackageName.Id,
                CoreConverters.SafeToSemVer(action.PackageName.Version)),
                "Expected the purge operation would only be executed AFTER the package was no longer referenced!");

            // Get the package out of the project manager
            var package = context.PackageManager.LocalRepository.FindPackage(
                action.PackageName.Id,
                CoreConverters.SafeToSemVer(action.PackageName.Version));
            Debug.Assert(package != null);

            return Task.Factory.StartNew(() =>
            {
                // Purge the package from the local repository
                context.PackageManager.Logger = new ShimLogger(logger);
                context.PackageManager.Execute(new PackageOperation(
                    package,
                    NuGet.PackageAction.Uninstall));
            });
        }

        public Task Rollback(PackageAction action, ExecutionContext context, IExecutionLogger logger)
        {
            // Just run the download action to undo a purge
            return new DownloadActionHandler().Execute(action, context, logger);
        }
    }
}