# YouTube Video Visitor

## Overview
This project automates the viewing of YouTube videos using Selenium with Tor for private browsing. It opens multiple tabs, navigates to randomly selected videos from a list, and simulates user interaction to play the videos.

## Requirements
- .NET Core SDK
- Mozilla Firefox with Tor Browser installed at the specified location (`C:\Users\AppData\Local\Tor Browser\Browser\firefox.exe`)
- Selenium WebDriver for Firefox (`OpenQA.Selenium`)

## Setup
1. **Clone Repository:**
   ```bash
   git clone https://github.com/your-username/your-repository.git
   cd your-repository
   ```

2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure URLs:**
   - Prepare a text file with YouTube video URLs, separated by commas (`url1,url2,...`). Provide the file path when prompted during execution.

## Usage
1. **Run the Application:**
   ```bash
   dotnet run
   ```

2. **Execution Flow:**
   - You will be prompted to enter the path of the text file containing YouTube video URLs.
   - The program iterates multiple times (`totalIterations`) and opens a specified number of tabs (`tabsPerIteration`) in each iteration.
   - Each tab navigates to randomly selected videos, handles Tor connection, and interacts with YouTube consent popups before playing the video.
   - Tabs remain open for a random duration before closing and waiting for the next iteration.

3. **Customization:**
   - Adjust `totalIterations`, `tabsPerIteration`, and timings (`tabDuration`, `iterationInterval`) in the code to suit your needs.

## Notes
- Ensure Mozilla Firefox with Tor Browser is correctly installed and located as specified in the code (`C:\Users\AppData\Local\Tor Browser\Browser\firefox.exe`).
- The application handles Tor connection settings and YouTube consent popups automatically.
- Debugging: Utilize `Debugger.Break()` for breakpoint debugging if errors occur during video playback.

## License
- This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details.
