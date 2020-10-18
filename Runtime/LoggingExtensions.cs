using System;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Logging
{
    /// <summary>
    /// This copies the python-style logging categories of DEBUG, INFO, WARNING, ERROR and CRITICAL
    /// and adds Android's VERBOSE (aka SPAM) and FATAL (alias for CRITICAL)
    /// By default VERBOSE and DEBUG level logs are stripped from non-development builds.
    /// Info level logging is for logs that aren't important but should show up in normal builds, like version number, etc.
    /// Critical level logs are situations that are so severe the app will crash or should be closed.
    /// </summary>
    public static class LoggingExtensions
    {
        public enum LOG_LEVEL
        {
            VERBOSE = 0,
            SPAM = 0,
            DEBUG = 1,
            INFO = 2,
            WARNING = 3,
            ERROR = 4,
            CRITICAL = 5,
            FATAL = 5,
            SILENT = 6
        }

        private const string DEBUG = nameof(DEBUG);
        private const string DEBUG_DRAW = nameof(DEBUG_DRAW);
        private const string DEVELOPMENT_BUILD = nameof(DEVELOPMENT_BUILD);

        private const string UNITY_ASSERTIONS = nameof(UNITY_ASSERTIONS);
        private const string UNITY_EDITOR = nameof(UNITY_EDITOR);

        private const string LOG_LEVEL_VERBOSE = nameof(LOG_LEVEL_VERBOSE);
        private const string LOG_LEVEL_DEBUG = nameof(LOG_LEVEL_DEBUG);
        private const string LOG_LEVEL_INFO = nameof(LOG_LEVEL_INFO);
        private const string LOG_LEVEL_WARN = nameof(LOG_LEVEL_WARN);
        private const string LOG_LEVEL_ERROR = nameof(LOG_LEVEL_ERROR);
        private const string LOG_LEVEL_CRITICAL = nameof(LOG_LEVEL_CRITICAL);

        private const string CRITICAL_ERROR_DECORATOR = "!!! {0} !!!";

        private static readonly Color INFO_COLOR = Color.cyan;
        private static readonly Color SPAM_COLOR = Color.white;
        private static readonly Color CRITICAL_COLOR = new Color32(255, 69, 0, 255);

        public static bool Colorize { get; set; } = true;
        public static LOG_LEVEL LogLevel { get; set; } = LOG_LEVEL.VERBOSE;

        #region Compiler Flags
        // Most of this is just to log what compiler flags are set for the current build.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void CompilationFlags()
        {
            string FLAG_FORMAT = "{0} compiler flag set.\n";

            var stringBuilder = new System.Text.StringBuilder();

#if UNITY_EDITOR
            stringBuilder.AppendFormat(FLAG_FORMAT, UNITY_EDITOR);
#endif

#if UNITY_STANDALONE
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_STANDALONE");
#endif

#if UNITY_IOS
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_IOS");
#endif

#if UNITY_ANDROID
            stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_ANDROID");
#endif

#if UNITY_PS4
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_PS4");
#endif

#if UNITY_XBOXONE
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_XBOXONE");
#endif

#if UNITY_WEBGL
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_WEBGL");
#endif

#if UNITY_ADS
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_ADS");
#endif

#if UNITY_ANALYTICS
		    stringBuilder.AppendFormat(FLAG_FORMAT, "UNITY_ANALYTICS");
#endif

#if UNITY_ASSERTIONS
            stringBuilder.AppendFormat(FLAG_FORMAT, UNITY_ASSERTIONS);
#endif

#if DEVELOPMENT_BUILD
		    stringBuilder.AppendFormat(FLAG_FORMAT, DEVELOPMENT_BUILD);
#endif

#if DEBUG
            stringBuilder.AppendFormat(FLAG_FORMAT, DEBUG);
#endif

#if CSHARP_7_3_OR_NEWER
            stringBuilder.AppendFormat(FLAG_FORMAT, "CSHARP_7_3_OR_NEWER");
#endif

#if ENABLE_MONO
            stringBuilder.AppendFormat(FLAG_FORMAT, "ENABLE_MONO");
#endif

#if ENABLE_IL2CPP
	        stringBuilder.AppendFormat(FLAG_FORMAT, "ENABLE_IL2CPP");
#endif

#if NET_2_0
		    stringBuilder.AppendFormat(FLAG_FORMAT, "NET_2_0");
#endif

#if NET_2_0_SUBSET
		    stringBuilder.AppendFormat(FLAG_FORMAT, "NET_2_0_SUBSET");
#endif

#if NET_LEGACY
		    stringBuilder.AppendFormat(FLAG_FORMAT, "NET_LEGACY");
#endif

#if NET_4_6
            stringBuilder.AppendFormat(FLAG_FORMAT, "NET_4_6");
#endif

#if NET_STANDARD_2_0
            stringBuilder.AppendFormat(FLAG_FORMAT, "NET_STANDARD_2_0");
#endif
            LoggingExtensions.Log(stringBuilder.ToString());
        }
        #endregion

        #region Decoration methods
        private static string PrefixScopeInfo(string message, int skipFrames = 2)
        {
#if UNITY_EDITOR || DEBUG || DEVELOPMENT_BUILD
            // StackFrame is expensive so don't even attempt in release builds
            var stackFrame = new StackFrame(skipFrames, false);
            var method = stackFrame.GetMethod();

            var methodName = method.Name;
            var className = method.DeclaringType.Name;

            message = $"[{className}.{methodName}] {message}";
#endif
            return message;
        }

        private static string ColorizeString(string message, Color color)
        {
#if UNITY_EDITOR
            message = Colorize ? $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>" : message;
#endif
            return message;
        }
        #endregion

        #region Mimic UnityEngine.Debug
        /// <summary>
        ///   <para>Reports whether the development console is visible. The development console cannot be made to appear using:</para>
        /// </summary>
        public static bool developerConsoleVisible => Debug.developerConsoleVisible;

        /// <summary>
        ///   <para>In the Build Settings dialog there is a check box called "Development Build".</para>
        /// </summary>
        public static bool isDebugBuild => Debug.isDebugBuild;

        /// <summary>
        ///   <para>Get default debug logger.</para>
        /// </summary>
        public static ILogger unityLogger => Debug.unityLogger;

        /// <summary>
        ///   <para>Draws a line between specified start and end points.</para>
        /// </summary>
        /// <param name="start">Point in world space where the line should start.</param>
        /// <param name="end">Point in world space where the line should end.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="duration">How long the line should be visible for.</param>
        /// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(DEBUG_DRAW)]
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f, bool depthTest = true)
        {
            Debug.DrawLine(start, end, color, duration, depthTest);
        }

        /// <summary>
        ///   <para>Draws a line from start to start + dir in world coordinates.</para>
        /// </summary>
        /// <param name="start">Point in world space where the ray should start.</param>
        /// <param name="dir">Direction and length of the ray.</param>
        /// <param name="color">Color of the drawn line.</param>
        /// <param name="duration">How long the line will be visible for (in seconds).</param>
        /// <param name="depthTest">Should the line be obscured by other objects closer to the camera?</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(DEBUG_DRAW)]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0.0f, bool depthTest = true)
        {
            Debug.DrawLine(start, start + dir, color, duration, depthTest);
        }

        /// <summary>
        ///   <para>Pauses the editor.</para>
        /// </summary>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD)]
        public static void Break()
        {
            Debug.Break();
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD)]
        public static void DebugBreak()
        {
            Debug.DebugBreak();
        }

        /// <summary>
        ///   <para>Clears errors from the developer console.</para>
        /// </summary>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD)]
        public static void ClearDeveloperConsole()
        {
            Debug.ClearDeveloperConsole();
        }

        /// <summary>
        ///   <para>Log a message to the Unity Console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG)]
        public static void Log(object message, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.DEBUG)
            {
                return;
            }

            string output = PrefixScopeInfo(message.ToString());

            Debug.unityLogger.Log(LogType.Log, (object)output, context);
        }

        /// <summary>
        ///   <para>Logs a formatted message to the Unity Console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG)]
        public static void LogFormat(string format, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.DEBUG)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Log, output);
        }

        /// <summary>
        ///   <para>A variant of Debug.Log that logs a warning message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN)]
        public static void LogWarning(object message, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(message.ToString());

            Debug.unityLogger.Log(LogType.Warning, (object)output, context);
        }

        /// <summary>
        ///   <para>Logs a formatted warning message to the Unity Console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN)]
        public static void LogWarningFormat(string format, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Warning, output);
        }

        /// <summary>
        ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR)]
        public static void LogError(object message, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.ERROR)
            {
                return;
            }

            string output = PrefixScopeInfo(message.ToString());

            Debug.unityLogger.Log(LogType.Error, (object)output, context);
        }

        /// <summary>
        ///   <para>Logs a formatted error message to the Unity console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR)]
        public static void LogErrorFormat(string format, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.ERROR)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Error, output);
        }

        /// <summary>
        ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
        /// </summary>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="exception">Runtime Exception.</param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogException(Exception exception, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.CRITICAL)
            {
                return;
            }

#if UNITY_EDITOR || DEBUG || DEVELOPMENT_BUILD
            string output = PrefixScopeInfo($"An exception occured: {exception.GetType().Name}");
            Debug.unityLogger.Log(LogType.Error, (object)output, context);
#endif

            Debug.unityLogger.LogException(exception, context);
        }

        /// <summary>
        ///   <para>Assert a condition and logs an error message to the Unity console on failure.</para>
        /// </summary>
        /// <param name="condition">Condition you expect to be true.</param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void Assert(bool condition, Object context = null)
        {
            if (condition || LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo("Assertion failed");

            Debug.unityLogger.Log(LogType.Assert, (object)output, context);
        }

        /// <summary>
        ///   <para>Assert a condition and logs an error message to the Unity console on failure.</para>
        /// </summary>
        /// <param name="condition">Condition you expect to be true.</param>
        /// <param name="context">Object to which the message applies.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void Assert(bool condition, object message, Object context = null)
        {
            if (condition || LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(message.ToString());

            Debug.unityLogger.Log(LogType.Assert, (object)output, context);
        }

        /// <summary>
        ///   <para>Assert a condition and logs a formatted error message to the Unity console on failure.</para>
        /// </summary>
        /// <param name="condition">Condition you expect to be true.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void AssertFormat(bool condition, string format, params object[] args)
        {
            if (condition || LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Assert, output);
        }

        /// <summary>
        ///   <para>Assert a condition and logs a formatted error message to the Unity console on failure.</para>
        /// </summary>
        /// <param name="condition">Condition you expect to be true.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void AssertFormat(bool condition, Object context, string format, params object[] args)
        {
            if (condition || LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Assert, (object)output, context);
        }

        /// <summary>
        ///   <para>A variant of Debug.Log that logs an assertion message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void LogAssertion(object message, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(message.ToString());

            Debug.unityLogger.Log(LogType.Assert, (object)output, context);
        }

        /// <summary>
        ///   <para>Logs a formatted assertion message to the Unity console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void LogAssertionFormat(string format, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Assert, output);
        }

        /// <summary>
        ///   <para>Logs a formatted assertion message to the Unity console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional(UNITY_ASSERTIONS)]
        public static void LogAssertionFormat(this Object context, string format, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(string.Format(format, args));

            Debug.unityLogger.Log(LogType.Assert, (object)output, context);
        }

        #endregion

        #region Verbose
        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE)]
        public static void LogVerbose(object pMessage, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.VERBOSE)
            {
                return;
            }

            string message = PrefixScopeInfo(pMessage.ToString());

            message = ColorizeString(message, SPAM_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE)]
        public static void LogVerbose(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.VERBOSE)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            message = ColorizeString(message, SPAM_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE)]
        public static void LogVerboseFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.VERBOSE)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            message = ColorizeString(message, INFO_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE)]
        public static void LogSpam(object pMessage, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.SPAM)
            {
                return;
            }

            string message = PrefixScopeInfo(pMessage.ToString());

            message = ColorizeString(message, SPAM_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE)]
        public static void LogSpam(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.SPAM)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            message = ColorizeString(message, SPAM_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE)]
        public static void LogSpamFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.SPAM)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            message = ColorizeString(message, INFO_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }
        #endregion

        #region Log
        /// <summary>
        /// Used for spammy debug logs that should be stripped from release builds
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG)]
        public static void Log(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.DEBUG)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        /// Used for spammy debug logs that should be stripped from release builds
        /// </summary>
        /// <param name="context"></param>
        /// <param name="color"></param>
        /// <param name="message"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG)]
        public static void Log(this UnityEngine.Object context, Color color, string message)
        {
            if (LogLevel > LOG_LEVEL.DEBUG)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            message = ColorizeString(message, color);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        /// Used for spammy debug logs that should be stripped from release builds
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG)]
        public static void LogFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.DEBUG)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        /// Used for spammy debug logs that should be stripped from release builds
        /// </summary>
        /// <param name="context"></param>
        /// <param name="color"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG)]
        public static void LogFormat(this UnityEngine.Object context, Color color, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.DEBUG)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            message = ColorizeString(message, color);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }
        #endregion

        #region Info
        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO)]
        public static void LogInfo(object pMessage, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.INFO)
            {
                return;
            }

            string message = PrefixScopeInfo(pMessage.ToString());

            message = ColorizeString(message, INFO_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO)]
        public static void LogInfo(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.INFO)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            message = ColorizeString(message, INFO_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO)]
        public static void LogInfoFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.INFO)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            message = ColorizeString(message, INFO_COLOR);

            Debug.unityLogger.Log(LogType.Log, (object)message, context);
        }
        #endregion

        #region Warning
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN)]
        public static void LogWarning(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            Debug.unityLogger.Log(LogType.Warning, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN)]
        public static void LogWarningFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            Debug.unityLogger.Log(LogType.Warning, (object)message, context);
        }
        #endregion

        #region Error
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR)]
        public static void LogError(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.ERROR)
            {
                return;
            }

            message = PrefixScopeInfo(message);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR)]
        public static void LogErrorFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.ERROR)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(message, args));

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }
        #endregion

        #region Exception/Critical
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogException(this UnityEngine.Object context, System.Exception exception)
        {
            if (LogLevel > LOG_LEVEL.CRITICAL)
            {
                return;
            }

            Debug.unityLogger.LogException(exception, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogCritical(object message, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.CRITICAL)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(CRITICAL_ERROR_DECORATOR, message));

            message = ColorizeString((string)message, CRITICAL_COLOR);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogCritical(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.CRITICAL)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(CRITICAL_ERROR_DECORATOR, message));

            message = ColorizeString(message, CRITICAL_COLOR);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogCriticalFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.CRITICAL)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(CRITICAL_ERROR_DECORATOR, string.Format(message, args)));

            message = ColorizeString(message, CRITICAL_COLOR);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogFatal(object message, Object context = null)
        {
            if (LogLevel > LOG_LEVEL.FATAL)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(CRITICAL_ERROR_DECORATOR, message));

            message = ColorizeString((string)message, CRITICAL_COLOR);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogFatal(this UnityEngine.Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.FATAL)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(CRITICAL_ERROR_DECORATOR, message));

            message = ColorizeString(message, CRITICAL_COLOR);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }

        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(DEVELOPMENT_BUILD), Conditional(LOG_LEVEL_VERBOSE), Conditional(LOG_LEVEL_DEBUG), Conditional(LOG_LEVEL_INFO), Conditional(LOG_LEVEL_WARN), Conditional(LOG_LEVEL_ERROR), Conditional(LOG_LEVEL_CRITICAL)]
        public static void LogFatalFormat(this UnityEngine.Object context, string message, params object[] args)
        {
            if (LogLevel > LOG_LEVEL.FATAL)
            {
                return;
            }

            message = PrefixScopeInfo(string.Format(CRITICAL_ERROR_DECORATOR, string.Format(message, args)));

            message = ColorizeString(message, CRITICAL_COLOR);

            Debug.unityLogger.Log(LogType.Error, (object)message, context);
        }
        #endregion

        #region Assertion
        [Conditional(UNITY_ASSERTIONS)]
        public static void LogAssertion(this Object context, string message)
        {
            if (LogLevel > LOG_LEVEL.WARNING)
            {
                return;
            }

            string output = PrefixScopeInfo(message.ToString());

            Debug.unityLogger.Log(LogType.Assert, (object)output, context);
        }
        #endregion
    }
}
