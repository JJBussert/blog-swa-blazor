using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace PuppeteerSharp
{
    public static class IPageExtensions
    {
        private static int _screenshotCounter = 1;

        public static async Task AttachScreenshotAsync(this IPage page, string attachmentComment, ITestOutputHelper output)
        {
            var stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1).GetMethod();
            var testClassName = callingMethod.ReflectedType.Name;
            var testMethodName = callingMethod.Name;
            var screenshotFileName = $"{testClassName}_{testMethodName}_{_screenshotCounter}.png";

            // Take a screenshot of the page
            var screenshotPath = Path.Combine(Path.GetTempPath(), screenshotFileName);
            await page.ScreenshotAsync(screenshotPath);

            // Attach the screenshot file to the test output
            using (var screenshotStream = new FileStream(screenshotPath, FileMode.Open))
            {
                var attachmentFilePath = screenshotPath;

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI")))
                {
                    // This code runs when in Azure DevOps.
                    // Use the 'VSTest@2' task to publish the screenshot as an attachment.
                    output.WriteLine($"##vso[task.uploadattachment attachmenttype=GeneralComment;comment={attachmentComment};]{attachmentFilePath}");
                    output.WriteLine($"##vso[task.addattachment attachmenttype=VSTestAttachment;name={screenshotFileName};]{attachmentFilePath}");
                }
                else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
                {
                    // This code runs when in GitHub Actions.
                    // Use the 'actions/upload-artifact' action to upload the screenshot as an artifact.
                    var artifactPath = Path.Combine("screenshots", screenshotFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
                    File.Copy(screenshotPath, artifactPath, overwrite: true);
                    output.WriteLine($"::warning::File attachment not supported in this test environment. Uploading artifact instead.");
                    output.WriteLine($"::set-output name=artifact_name::Screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
                    output.WriteLine($"::set-output name=artifact_path::{artifactPath}");
                }
                else
                {
                    // This code runs when not in Azure DevOps or GitHub Actions.
                    // Attach a clickable link to the screenshot file path to the test output.
                    var attachmentUri = new Uri(Path.GetFullPath(attachmentFilePath));
                    var attachmentLink = $"<a href=\"{attachmentUri}\">{screenshotFileName}</a>";
                    output.WriteLine($"Attached file: {attachmentLink}");
                }
            }

            // Increment the screenshot counter
            _screenshotCounter++;
        }
    }

}
