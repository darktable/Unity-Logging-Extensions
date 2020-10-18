using Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Debug = Logging.LoggingExtensions;

namespace Tests
{
    public class LoggingExtensionTests
    {
        private static GameObject gameObject;

        [SetUp]
        public void SetUp()
        {
            Debug.Colorize = false;
            Debug.LogLevel = Debug.LOG_LEVEL.VERBOSE;

            gameObject = new GameObject("LoggingTest");
        }

        [TearDown]
        public void TearDown()
        {
            Debug.Colorize = true;
            Debug.LogLevel = Debug.LOG_LEVEL.VERBOSE;

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void LogLevelSpamTest()
        {
            Debug.LogLevel = Debug.LOG_LEVEL.VERBOSE;

            Debug.LogVerbose("Verbose");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelSpamTest] Verbose");

            gameObject.LogVerbose("Object.Verbose");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelSpamTest] Object.Verbose");

            Debug.LogSpam("Spam");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelSpamTest] Spam");

            gameObject.LogSpam("Object.Spam");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelSpamTest] Object.Spam");

            Debug.LogLevel = Debug.LOG_LEVEL.DEBUG;

            Debug.LogVerbose("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.Log("Log");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelSpamTest] Log");
        }

        [Test]
        public void LogLevelDebugTest()
        {
            Debug.LogLevel = Debug.LOG_LEVEL.DEBUG;

            Debug.LogVerbose("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.Log("Log");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelDebugTest] Log");

            gameObject.Log("Object.Log");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelDebugTest] Object.Log");

            Debug.LogLevel = Debug.LOG_LEVEL.INFO;

            Debug.Log("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogInfo("Info");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelDebugTest] Info");
        }

        [Test]
        public void LogLevelInfoTest()
        {
            Debug.LogLevel = Debug.LOG_LEVEL.INFO;

            Debug.Log("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogInfo("Info");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelInfoTest] Info");

            gameObject.LogInfo("Object.Info");
            LogAssert.Expect(LogType.Log, "[LoggingExtensionTests.LogLevelInfoTest] Object.Info");

            Debug.LogLevel = Debug.LOG_LEVEL.WARNING;

            Debug.LogInfo("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogWarning("Warning");
            LogAssert.Expect(LogType.Warning, "[LoggingExtensionTests.LogLevelInfoTest] Warning");
        }

        [Test]
        public void LogLevelWarningTest()
        {
            Debug.LogLevel = Debug.LOG_LEVEL.WARNING;

            Debug.Log("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogWarning("Warning");
            LogAssert.Expect(LogType.Warning, "[LoggingExtensionTests.LogLevelWarningTest] Warning");

            gameObject.LogWarning("Object.Warning");
            LogAssert.Expect(LogType.Warning, "[LoggingExtensionTests.LogLevelWarningTest] Object.Warning");

            Debug.LogLevel = Debug.LOG_LEVEL.ERROR;

            Debug.LogWarning("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogError("Error");
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelWarningTest] Error");
        }

        [Test]
        public void LogLevelErrorTest()
        {
            Debug.LogLevel = Debug.LOG_LEVEL.ERROR;

            Debug.LogWarning("Nothing");
            LogAssert.NoUnexpectedReceived();
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*IgnoreFailingMessages.*"));

            Debug.LogError("Error");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelErrorTest] Error");

            gameObject.LogError("Object.Error");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelErrorTest] Object.Error");

            Debug.LogLevel = Debug.LOG_LEVEL.CRITICAL;

            Debug.LogError("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogCritical("Critical");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelErrorTest] !!! Critical !!!");
        }

        [Test]
        public void LogLevelCriticalTest()
        {
            Debug.LogLevel = Debug.LOG_LEVEL.CRITICAL;

            Debug.LogError("Nothing");
            LogAssert.NoUnexpectedReceived();
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*IgnoreFailingMessages.*"));

            Debug.LogCritical("Critical");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelCriticalTest] !!! Critical !!!");

            gameObject.LogCritical("Object.Critical");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelCriticalTest] !!! Object.Critical !!!");

            Debug.LogFatal("Fatal");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelCriticalTest] !!! Fatal !!!");

            gameObject.LogFatal("Object.Fatal");
            LogAssert.Expect(LogType.Error, "[LoggingExtensionTests.LogLevelCriticalTest] !!! Object.Fatal !!!");

            Debug.LogLevel = Debug.LOG_LEVEL.SILENT;

            Debug.LogCritical("Nothing");
            LogAssert.NoUnexpectedReceived();
            Debug.LogCritical("Critical");
            LogAssert.NoUnexpectedReceived();
        }
    }
}
