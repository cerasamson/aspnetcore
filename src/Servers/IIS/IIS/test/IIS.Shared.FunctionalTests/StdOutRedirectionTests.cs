// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Server.IIS.FunctionalTests.Utilities;
using Microsoft.AspNetCore.Server.IntegrationTesting.IIS;
using Microsoft.AspNetCore.Testing;

#if !IIS_FUNCTIONALS
using Microsoft.AspNetCore.Server.IIS.FunctionalTests;

#if IISEXPRESS_FUNCTIONALS
namespace Microsoft.AspNetCore.Server.IIS.IISExpress.FunctionalTests;
#elif NEWHANDLER_FUNCTIONALS
namespace Microsoft.AspNetCore.Server.IIS.NewHandler.FunctionalTests;
#elif NEWSHIM_FUNCTIONALS
namespace Microsoft.AspNetCore.Server.IIS.NewShim.FunctionalTests;
#endif

#else
namespace Microsoft.AspNetCore.Server.IIS.FunctionalTests;
#endif

[Collection(PublishedSitesCollection.Name)]
public class StdOutRedirectionTests : IISFunctionalTestBase
{
    public StdOutRedirectionTests(PublishedSitesFixture fixture) : base(fixture)
    {
    }

    [ConditionalFact]
    [RequiresNewShim]
    public async Task FrameworkNotFoundExceptionLogged_Pipe()
    {
        var deploymentParameters = Fixture.GetBaseDeploymentParameters(Fixture.InProcessTestSite);

        await RunTest(deploymentParameters, async deploymentResult =>
        {
            Helpers.ModifyFrameworkVersionInRuntimeConfig(deploymentResult);

            var response = await deploymentResult.HttpClient.GetAsync("/HelloWorld");
            Assert.False(response.IsSuccessStatusCode);

            StopServer();

            EventLogHelpers.VerifyEventLogEvent(deploymentResult,
                @"Framework: 'Microsoft.NETCore.App', version '2.9.9' \(x64\)", Logger);
            EventLogHelpers.VerifyEventLogEvent(deploymentResult,
                "To install missing framework, download:", Logger);
        });
    }

    [ConditionalFact]
    [RequiresNewShim]
    [QuarantinedTest("https://github.com/dotnet/aspnetcore/issues/43087")]
    public async Task FrameworkNotFoundExceptionLogged_File()
    {
        var deploymentParameters =
            Fixture.GetBaseDeploymentParameters(Fixture.InProcessTestSite);

        deploymentParameters.EnableLogging(LogFolderPath);

        await RunTest(deploymentParameters, async deploymentResult =>
        {
            Helpers.ModifyFrameworkVersionInRuntimeConfig(deploymentResult);

            var response = await deploymentResult.HttpClient.GetAsync("/HelloWorld");
            Assert.False(response.IsSuccessStatusCode);

            StopServer();

            var contents = Helpers.ReadAllTextFromFile(Helpers.GetExpectedLogName(deploymentResult, LogFolderPath), Logger);
            var missingFrameworkString = "To install missing framework, download:";
            EventLogHelpers.VerifyEventLogEvent(deploymentResult,
                @"Framework: 'Microsoft.NETCore.App', version '2.9.9' \(x64\)", Logger);
            EventLogHelpers.VerifyEventLogEvent(deploymentResult,
                missingFrameworkString, Logger);
            Assert.Contains(@"Framework: 'Microsoft.NETCore.App', version '2.9.9' (x64)", contents);
            Assert.Contains(missingFrameworkString, contents);
        });
    }

    [ConditionalFact]
    [RequiresIIS(IISCapability.PoolEnvironmentVariables)]
    [SkipIfDebug]
    public async Task EnableCoreHostTraceLogging_TwoLogFilesCreated()
    {
        var deploymentParameters =
            Fixture.GetBaseDeploymentParameters(Fixture.InProcessTestSite);
        deploymentParameters.TransformArguments((a, _) => $"{a} CheckLargeStdOutWrites");

        deploymentParameters.EnvironmentVariables["COREHOST_TRACE"] = "1";

        deploymentParameters.EnableLogging(LogFolderPath);

        await RunTest(deploymentParameters, async deploymentResult =>
        {
            var response = await deploymentResult.HttpClient.GetAsync("/HelloWorld");
            Assert.False(response.IsSuccessStatusCode);

            StopServer();

            var fileInDirectory = Directory.GetFiles(LogFolderPath).Single();
            var contents = Helpers.ReadAllTextFromFile(fileInDirectory, Logger);
            EventLogHelpers.VerifyEventLogEvent(deploymentResult, "Invoked hostfxr", Logger);
            Assert.Contains("Invoked hostfxr", contents);
        });
    }

    [ConditionalTheory]
    [QuarantinedTest("https://github.com/dotnet/aspnetcore/issues/42913")]
    [RequiresIIS(IISCapability.PoolEnvironmentVariables)]
    [SkipIfDebug]
    [InlineData("CheckLargeStdErrWrites")]
    [InlineData("CheckLargeStdOutWrites")]
    [InlineData("CheckOversizedStdErrWrites")]
    [InlineData("CheckOversizedStdOutWrites")]
    public async Task EnableCoreHostTraceLogging_PipeCaptureNativeLogs(string path)
    {
        var deploymentParameters = Fixture.GetBaseDeploymentParameters(Fixture.InProcessTestSite);
        deploymentParameters.EnvironmentVariables["COREHOST_TRACE"] = "1";
        deploymentParameters.TransformArguments((a, _) => $"{a} {path}");

        await RunTest(deploymentParameters, async deploymentResult =>
        {
            var response = await deploymentResult.HttpClient.GetAsync("/HelloWorld");

            Assert.False(response.IsSuccessStatusCode);

            StopServer();

            EventLogHelpers.VerifyEventLogEvent(deploymentResult, "Invoked hostfxr", Logger);
        });
    }

    [ConditionalTheory]
    [QuarantinedTest("https://github.com/dotnet/aspnetcore/issues/42820")]
    [RequiresIIS(IISCapability.PoolEnvironmentVariables)]
    [SkipIfDebug]
    [InlineData("CheckLargeStdErrWrites")]
    [InlineData("CheckLargeStdOutWrites")]
    [InlineData("CheckOversizedStdErrWrites")]
    [InlineData("CheckOversizedStdOutWrites")]
    public async Task EnableCoreHostTraceLogging_FileCaptureNativeLogs(string path)
    {
        var deploymentParameters =
            Fixture.GetBaseDeploymentParameters(Fixture.InProcessTestSite);
        deploymentParameters.EnvironmentVariables["COREHOST_TRACE"] = "1";
        deploymentParameters.TransformArguments((a, _) => $"{a} {path}");

        deploymentParameters.EnableLogging(LogFolderPath);

        await RunTest(deploymentParameters, async deploymentResult =>
        {
            var response = await deploymentResult.HttpClient.GetAsync("/HelloWorld");
            Assert.False(response.IsSuccessStatusCode);

            StopServer();

            var fileInDirectory = Directory.GetFiles(LogFolderPath).First();
            var contents = Helpers.ReadAllTextFromFile(fileInDirectory, Logger);

            EventLogHelpers.VerifyEventLogEvent(deploymentResult, "Invoked hostfxr", Logger);
            Assert.Contains("Invoked hostfxr", contents);
        });
    }
}
