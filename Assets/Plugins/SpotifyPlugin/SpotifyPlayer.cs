
using System.Diagnostics;
using System.Text;
using System;
using System.Runtime.InteropServices;

namespace Net.Junian.UniPlugins
{
    public class SpotifyPlayer
    {

        const string Osascript = "osascript";
        const string SpotifyAppName = "Spotify";
        const string spotifyUriHeader = "spotify:track:";
        const string spotifyTrackLinkHeader = "https://open.spotify.com/track/";

        private static SpotifyPlayer _instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static SpotifyPlayer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SpotifyPlayer();
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpotifyAppleScriptApi"/> class.
        /// </summary>
        private SpotifyPlayer()
        {
        }

        /// <summary>
        /// Play the specified trackUri.
        /// </summary>
        /// <param name="trackUri">Track URI.</param>
        public void PlayTrack(string trackUri)
        {
            var spotifyTrackUri = "";
            if (trackUri.StartsWith(spotifyUriHeader) ||
              trackUri.StartsWith(spotifyTrackLinkHeader))
            {
                spotifyTrackUri = trackUri;
            }
            else
            {
                spotifyTrackUri = spotifyUriHeader + trackUri;
            }

            ExecuteScript(string.Format("play track \"{0}\"", spotifyTrackUri));
        }

        /// <summary>
        /// Play the specified anySpotifyUri.
        /// </summary>
        /// <param name="anySpotifyUri">Any spotify URI.</param>
        public void Play(string anySpotifyUri)
        {
            ExecuteScript(string.Format("play track \"{0}\"", anySpotifyUri));
        }

        /// <summary>
        /// Plaies the loop.
        /// </summary>
        /// <param name="anySpotifyUri">Any spotify URI.</param>
        public void PlayLoop(string anySpotifyUri)
        {
            var command =
                "tell application \"Spotify\"\n" +
                "  set shuffling to false\n" +
                "  set repeating to true\n" +
                "  play track \"{0}\"\n" +
                "end tell\n";
            ExecuteChainScript(string.Format(command, anySpotifyUri));
        }

        /// <summary>
        /// Plaies the once.
        /// </summary>
        /// <param name="anySpotifyUri">Any spotify URI.</param>
        public void PlayOnce(string trackUri)
        {
            var command =
                "tell application \"Spotify\"\n" +
                "  pause\n" +
                "  set shuffling to false\n" +
                "  set repeating to false\n" +
                "  play track \"{0}\"\n" +
                "end tell\n";

            var spotifyTrackUri = "";
            if (trackUri.StartsWith(spotifyUriHeader) ||
              trackUri.StartsWith(spotifyTrackLinkHeader))
            {
                spotifyTrackUri = trackUri;
            }
            else
            {
                spotifyTrackUri = spotifyUriHeader + trackUri;
            }

            ExecuteChainScript(string.Format(command, spotifyTrackUri));
        }

        /// <summary>
        /// Plaies the pause.
        /// </summary>
        public void PlayPause()
        {
            ExecuteScript("playpause");
        }

        /// <summary>
        /// Play this instance.
        /// </summary>
        public void Play()
        {
            ExecuteScript("play");
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public void Pause()
        {
            ExecuteScript("pause");
        }

        /// <summary>
        /// Sets the shuffling.
        /// </summary>
        /// <param name="isShuffling">If set to <c>true</c> is shuffling.</param>
        public void SetShuffling(bool isShuffling)
        {
            ExecuteScript(
                string.Format("set shuffling to {0}",
                    isShuffling.ToString()));
        }

        /// <summary>
        /// Sets the repeat.
        /// </summary>
        /// <param name="isRepeat">If set to <c>true</c> is repeat.</param>
        public void SetRepeating(bool isRepeat)
        {
            ExecuteScript(
                string.Format("set repeating to {0}",
                    isRepeat.ToString()));
        }

        /// <summary>
        /// Builds the osa script parameter.
        /// </summary>
        /// <returns>The osa script parameter.</returns>
        /// <param name="commandToTell">Command to tell.</param>
        private string BuildOsaScriptParameter(string commandToTell)
        {
            return string.Format("-e 'tell application \"{0}\" to {1}'", SpotifyAppName, commandToTell);
        }

        private string BuildOsaCommandsChain(string commandChain)
        {
            return string.Format("-e '{0}'", commandChain);
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <returns>The string value.</returns>
        /// <param name="command">Command.</param>
        public string GetStringValue(string command)
        {
            try
            {
                return ExecuteSpotifyAppleScript(command);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the player position.
        /// </summary>
        /// <value>The player position.</value>
        public float PlayerPosition
        {
            get
            {
                var positionStr = GetStringValue("player position");
                var str = -1f;
                float.TryParse(positionStr, out str);
                return str;
            }
        }

        /// <summary>
        /// Gets the state of the player.
        /// </summary>
        /// <value>The state of the player.</value>
        public string PlayerState
        {
            get
            {
                return GetStringValue("player state as text");
            }
        }

        /// <summary>
        /// Gets the duration of current track.
        /// </summary>
        /// <value>The duration of current track.</value>
        public float DurationOfCurrentTrack
        {
            get
            {
                var str = float.Parse(GetStringValue("duration of current track"));
                return str;
            }
        }

        public string SpotifyUrlOfCurrentTrack
        {
            get
            {
                var str = (GetStringValue("spotify url of current track"));
                return str;
            }
        }

        public void SetVolume(int volume)
        {
            ExecuteScript(
                string.Format("set sound volume to {0}",
                    volume.ToString()));
        }

        public void ExecuteScript(string command)
        {
            try
            {
                ExecuteSpotifyAppleScript(command);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        public void ExecuteChainScript(string command)
        {
            try
            {
                ExecuteAppleScript(command);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        [DllImport("SpotifyPlugin")]
        private static extern IntPtr _ExecuteSpotifyAppleScript(string command);

        public string ExecuteSpotifyAppleScript(string command)
        {
            try
            {
                var result = Marshal.PtrToStringAuto(SpotifyPlayer._ExecuteSpotifyAppleScript(command));
                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
            return string.Empty;
        }

#if UNITY_STANDALONE

        [DllImport("SpotifyPlugin")]
		private static extern IntPtr _ExecuteAppleScript(string command);

#endif

        public string ExecuteAppleScript(string command)
        {

#if UNITY_STANDALONE

			try
			{
				var result = Marshal.PtrToStringAuto(SpotifyPlayer._ExecuteAppleScript(command));
				return result;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex);
			}

#endif

            return string.Empty;
        }

#if UNITY_IOS

        [DllImport("__Internal")]
        private static extern void _Init(string clientID, string callbackUrl);
#endif

        public void Init(string clientID, string callbackUrl)
        {
#if UNITY_IOS
            try
            {
                SpotifyPlayer._Init(clientID, callbackUrl);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
#endif
        }

    }
}