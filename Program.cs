using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to My YouTube Video Visitor!");

            // Prompt user to input the path of the text file containing URLs
            Console.Write("Please enter the path of the text file containing URLs: ");
            string filePath = Console.ReadLine();

            // Read URLs from the text file
            string[] urls = ReadURLsFromFile(filePath);
            if (urls.Length == 0)
            {
                Console.WriteLine("No valid URLs found in the file. Exiting...");
                return;
            }

            // Number of iterations
            int totalIterations = 100;

            // Number of tabs to open in each iteration
            int tabsPerIteration = 5;

            for (int i = 1; i <= totalIterations; i++)
            {
                // Randomly select 5 URLs for this iteration
                string[] selectedUrls = urls.OrderBy(x => Guid.NewGuid()).Take(tabsPerIteration).ToArray();

                // Set up Tor browser options for private browsing mode
                var options = new FirefoxOptions();
                options.BrowserExecutableLocation = @"C:\Users\nusha\AppData\Local\Tor Browser\Browser\firefox.exe";
                options.Profile = new FirefoxProfileManager().GetProfile("default");
                options.Profile.SetPreference("browser.privatebrowsing.autostart", true);
                options.AddArgument("--disable-popup-blocking");

                // Launch Tor Browser
                Console.WriteLine("Launching Tor Browser...");
                var driver = new FirefoxDriver(options);

                // Handle the initial Tor connection window
                HandleTorConnectionWindow(driver);

                // Wait for Tor Browser to connect to the Tor network
                Console.WriteLine("Waiting for Tor Browser to connect to the Tor network...");
                await Task.Delay(30000); // Adjust delay as needed

                Console.WriteLine($"\nIteration {i}/{totalIterations}");

                foreach (string videoUrl in selectedUrls)
                {
                    try
                    {
                        // Validate if the URL is non-empty
                        if (string.IsNullOrWhiteSpace(videoUrl))
                        {
                            Console.WriteLine("Skipping empty URL.");
                            continue;
                        }

                        // Open a new tab
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
                        driver.SwitchTo().Window(driver.WindowHandles.Last());

                        Console.WriteLine($"Opening YouTube video: {videoUrl}");

                        bool success = false;
                        for (int attempt = 0; attempt < 3; attempt++)
                        {
                            try
                            {
                                driver.Navigate().GoToUrl(videoUrl);
                                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
                                wait.Until(d => d.FindElements(By.CssSelector("video")).Any());
                                success = true;
                                break;
                            }
                            catch (WebDriverTimeoutException)
                            {
                                Console.WriteLine($"Timeout navigating to {videoUrl}, retrying... (Attempt {attempt + 1}/3)");
                            }
                        }

                        if (!success)
                        {
                            throw new Exception("Failed to navigate to the video URL after 3 attempts.");
                        }

                        HandleYouTubeConsentPopup(driver);
                        PlayVideo(driver);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error opening video: {ex.Message}");
                        if (driver.WindowHandles.Count > 1)
                        {
                            Debugger.Break(); // Breakpoint here for debugging
                            driver.Close();
                            driver.SwitchTo().Window(driver.WindowHandles.First());
                        }
                    }
                }

                // Calculate a random duration to keep the tabs open
                int tabDuration = new Random().Next(900000, 1200000); // Random duration between 15 and 20 minutes
                Console.WriteLine($"Tabs will remain open for {tabDuration / 60000} minutes.");

                // Wait for the random duration before closing the tabs
                await Task.Delay(tabDuration);

                // Close all tabs and quit the browser
                driver.Quit();

                // Wait for a random interval before the next iteration
                int iterationInterval = new Random().Next(120000, 180000); // Random interval between 2 and 3 minutes
                Console.WriteLine($"Waiting for {iterationInterval / 1000} seconds before the next iteration...");
                await Task.Delay(iterationInterval);
            }

            Console.WriteLine("\nAll iterations completed.");
            Console.ReadKey(); // Keeps the console window open until a key is pressed
        }

        static string[] ReadURLsFromFile(string filePath)
        {
            try
            {
                // Read all lines from the file and split them by comma to get individual URLs
                string[] urls = File.ReadAllText(filePath).Split(',').Select(url => url.Trim()).ToArray();
                return urls;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading URLs from file: {ex.Message}");
                return new string[0];
            }
        }

        static void HandleTorConnectionWindow(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                // Wait for the "Always connect automatically" checkbox and click it
                var alwaysConnectCheckbox = wait.Until(d => d.FindElement(By.CssSelector("input[type='checkbox']")));
                alwaysConnectCheckbox.Click();

                // Wait for the "Connect" button and click it
                var connectButton = wait.Until(d => d.FindElement(By.CssSelector("button#connectButton"))); // Adjust the selector as needed
                connectButton.Click();

                Console.WriteLine("Successfully clicked 'Always connect automatically' and 'Connect' button.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling Tor connection window: {ex.Message}");
            }
        }

        static void HandleYouTubeConsentPopup(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var consentPopup = wait.Until(d => d.FindElements(By.CssSelector("form[action='https://consent.youtube.com/save']")));

                if (consentPopup.Any())
                {
                    var rejectAllButton = driver.FindElement(By.XPath("//button[contains(text(),'Reject all')]"));
                    rejectAllButton.Click();
                    Console.WriteLine("Clicked 'Reject all' on YouTube consent popup.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("YouTube consent popup not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling YouTube consent popup: {ex.Message}");
            }
        }

        static void PlayVideo(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;

                // Wait for the video element to be present
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30)); // Extended wait time
                wait.Until(d => d.FindElements(By.CssSelector("video")).Any());

                // Close potential popups or overlays
                ClosePopups(driver);

                // Set a random start time
                int videoLength = GetVideoLength(driver);
                if (videoLength > 0)
                {
                    int randomStartTime = new Random().Next(Math.Min(900, videoLength), Math.Min(1800, videoLength));
                    js.ExecuteScript($"document.querySelector('video').currentTime = {randomStartTime};");
                }

                // Simulate pressing 'k' to play the video
                Console.WriteLine("Simulating 'k' key press to play the video...");
                Actions action = new Actions(driver);
                action.SendKeys("k").Perform();

                // Ensure the video is playing
                wait.Until(d => (bool)js.ExecuteScript("return !document.querySelector('video').paused;"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring video is playing: {ex.Message}");
            }
        }

        static int GetVideoLength(IWebDriver driver)
        {
            try
            {
                var length = Convert.ToDouble(((IJavaScriptExecutor)driver).ExecuteScript("return document.querySelector('video').duration;"));
                return (int)length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting video length: {ex.Message}");
                return 0;
            }
        }

        static void ClosePopups(IWebDriver driver)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;

                // Close "Sign in" popup if present
                var signInPopup = driver.FindElements(By.CssSelector("ytd-popup-container paper-dialog"));
                if (signInPopup.Any())
                {
                    js.ExecuteScript("document.querySelector('ytd-popup-container paper-dialog').remove();");
                }

                // Close "Privacy reminder" popup if present
                var privacyReminder = driver.FindElements(By.CssSelector("ytd-consent-bump-v2-lightbox"));
                if (privacyReminder.Any())
                {
                    js.ExecuteScript("document.querySelector('ytd-consent-bump-v2-lightbox').remove();");
                }

                // Close other overlays or modals as needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing popups: {ex.Message}");
            }
        }
    }
}
