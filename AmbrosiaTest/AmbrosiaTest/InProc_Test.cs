﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms; // need this to handle threading issue on sleeps
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Configuration;


namespace AmbrosiaTest
{
    /// <summary>
    /// Summary description for InProc_Test
    /// </summary>
    [TestClass]
    public class InProc_Test
    {
        //************* Init Code *****************
        // NOTE: Need this bit of code at the top of every "[TestClass]" (per .cs test file) to get context \ details of the current test running
        // NOTE: Make sure all names be "Azure Safe". No capital letters and no underscore.
        [TestInitialize()]
        public void Initialize()
        {
            Utilities MyUtils = new Utilities();
            MyUtils.TestInitialize();
        }
        //************* Init Code *****************


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        //** Basic end to end test for the InProc TCP feature where Client is InProc and Server is Two Proc
        [TestMethod]
        public void AMB_InProc_TCP_ClientOnly_Test()
        {
            //NOTE - the Cleanup has this hard coded so if this changes, update Cleanup section too
            string testName = "inproctcpclientonly";
            string clientJobName = testName + "clientjob";
            string serverName = testName + "server";
            string ambrosiaLogDir = ConfigurationManager.AppSettings["AmbrosiaLogDirectory"] + "\\";
            string byteSize = "1073741824"; 

            Utilities MyUtils = new Utilities();

            //AMB1 - Job
            string logOutputFileName_AMB1 = testName + "_AMB1.log";
            AMB_Settings AMB1 = new AMB_Settings
            {
                AMB_ServiceName = clientJobName,
                AMB_PortAppReceives = "1000",
                AMB_PortAMBSends = "1001",
                AMB_ServiceLogPath = ambrosiaLogDir,
                AMB_CreateService = "A",
                AMB_PauseAtStart = "N",
                AMB_PersistLogs = "Y",
                AMB_NewLogTriggerSize = "1000",
                AMB_ActiveActive = "N",
                AMB_Version = "0"
            };
            MyUtils.CallAMB(AMB1, logOutputFileName_AMB1, AMB_ModeConsts.RegisterInstance);

            //AMB2
            string logOutputFileName_AMB2 = testName + "_AMB2.log";
            AMB_Settings AMB2 = new AMB_Settings
            {
                AMB_ServiceName = serverName,
                AMB_PortAppReceives = "2000",
                AMB_PortAMBSends = "2001",
                AMB_ServiceLogPath = ambrosiaLogDir,
                AMB_CreateService = "A",
                AMB_PauseAtStart = "N",
                AMB_PersistLogs = "Y",
                AMB_NewLogTriggerSize = "1000",
                AMB_ActiveActive = "N",
                AMB_Version = "0"
            };
            MyUtils.CallAMB(AMB2, logOutputFileName_AMB2, AMB_ModeConsts.RegisterInstance);

            //ImmCoord2
            string logOutputFileName_ImmCoord2 = testName + "_ImmCoord2.log";
            int ImmCoordProcessID2 = MyUtils.StartImmCoord(serverName, 2500, logOutputFileName_ImmCoord2);

            //Client Job Call
            string logOutputFileName_ClientJob = testName + "_ClientJob.log";
            int clientJobProcessID = MyUtils.StartPerfClientJob("1001", "1000", clientJobName, serverName, "1024", "1", logOutputFileName_ClientJob, MyUtils.deployModeInProcManual, "1500");

            // Give it a few seconds to start
            Thread.Sleep(2000);

            //Server Call
            string logOutputFileName_Server = testName + "_Server.log";
            int serverProcessID = MyUtils.StartPerfServer("2001", "2000", clientJobName, serverName, logOutputFileName_Server, 1, false);

            //Delay until client is done - also check Server just to make sure
            bool pass = MyUtils.WaitForProcessToFinish(logOutputFileName_ClientJob, byteSize, 5, false, testName, true); // number of bytes processed
            pass = MyUtils.WaitForProcessToFinish(logOutputFileName_Server, byteSize, 5, false, testName, true);

            // Stop things so file is freed up and can be opened in verify
            MyUtils.KillProcess(clientJobProcessID);
            MyUtils.KillProcess(serverProcessID);
            MyUtils.KillProcess(ImmCoordProcessID2);

            // Verify Client
            MyUtils.VerifyTestOutputFileToCmpFile(logOutputFileName_ClientJob);

            // Verify Server
            MyUtils.VerifyTestOutputFileToCmpFile(logOutputFileName_Server);

            // Verify integrity of Ambrosia logs by replaying
            // Unable to verify when client files in different location than server log - TO DO: modify method to do this
  //          MyUtils.VerifyAmbrosiaLogFile(testName, Convert.ToInt64(byteSize), true, true, AMB1.AMB_Version);
        }

        //** Simple end to end where Client is InProc Pipe and Server is two proc
        [TestMethod]
        public void AMB_InProc_Pipe_ClientOnly_Test()
        {
            //NOTE - the Cleanup has this hard coded so if this changes, update Cleanup section too
            string testName = "inprocpipeclientonly";
            string clientJobName = testName + "clientjob";
            string serverName = testName + "server";
            string ambrosiaLogDir = ConfigurationManager.AppSettings["AmbrosiaLogDirectory"] + "\\";
            string byteSize = "1073741824";

            Utilities MyUtils = new Utilities();

            //AMB1 - Job
            string logOutputFileName_AMB1 = testName + "_AMB1.log";
            AMB_Settings AMB1 = new AMB_Settings
            {
                AMB_ServiceName = clientJobName,
                AMB_PortAppReceives = "1000",
                AMB_PortAMBSends = "1001",
                AMB_ServiceLogPath = ambrosiaLogDir,
                AMB_CreateService = "A",
                AMB_PauseAtStart = "N",
                AMB_PersistLogs = "Y",
                AMB_NewLogTriggerSize = "1000",
                AMB_ActiveActive = "N",
                AMB_Version = "0"
            };
            MyUtils.CallAMB(AMB1, logOutputFileName_AMB1, AMB_ModeConsts.RegisterInstance);

            //AMB2
            string logOutputFileName_AMB2 = testName + "_AMB2.log";
            AMB_Settings AMB2 = new AMB_Settings
            {
                AMB_ServiceName = serverName,
                AMB_PortAppReceives = "2000",
                AMB_PortAMBSends = "2001",
                AMB_ServiceLogPath = ambrosiaLogDir,
                AMB_CreateService = "A",
                AMB_PauseAtStart = "N",
                AMB_PersistLogs = "Y",
                AMB_NewLogTriggerSize = "1000",
                AMB_ActiveActive = "N",
                AMB_Version = "0"
            };
            MyUtils.CallAMB(AMB2, logOutputFileName_AMB2, AMB_ModeConsts.RegisterInstance);

            //ImmCoord2
            string logOutputFileName_ImmCoord2 = testName + "_ImmCoord2.log";
            int ImmCoordProcessID2 = MyUtils.StartImmCoord(serverName, 2500, logOutputFileName_ImmCoord2);

            //Client Job Call
            string logOutputFileName_ClientJob = testName + "_ClientJob.log";
            int clientJobProcessID = MyUtils.StartPerfClientJob("1001", "1000", clientJobName, serverName, "1024", "1", logOutputFileName_ClientJob, MyUtils.deployModeInProc, "1500");

            // Give it a few seconds to start
            Thread.Sleep(2000);

            //Server Call
            string logOutputFileName_Server = testName + "_Server.log";
            int serverProcessID = MyUtils.StartPerfServer("2001", "2000", clientJobName, serverName, logOutputFileName_Server, 1, false);

            //Delay until client is done - also check Server just to make sure
            bool pass = MyUtils.WaitForProcessToFinish(logOutputFileName_ClientJob, byteSize, 5, false, testName, true); // number of bytes processed
            pass = MyUtils.WaitForProcessToFinish(logOutputFileName_Server, byteSize, 5, false, testName, true);

            // Stop things so file is freed up and can be opened in verify
            MyUtils.KillProcess(clientJobProcessID);
            MyUtils.KillProcess(serverProcessID);
            MyUtils.KillProcess(ImmCoordProcessID2);

            // Verify Client
            MyUtils.VerifyTestOutputFileToCmpFile(logOutputFileName_ClientJob);

            // Verify Server
            MyUtils.VerifyTestOutputFileToCmpFile(logOutputFileName_Server);

            // Verify integrity of Ambrosia logs by replaying 
            // To verify Server in one location and client in another would take bigger code change
            // Not that crucial to do ... but TO DO: make it so verify log in two different places.
            //MyUtils.VerifyAmbrosiaLogFile(testName, Convert.ToInt64(byteSize), true, true, AMB1.AMB_Version);
        }


        [TestCleanup()]
        public void Cleanup()
        {

            // Kill all ImmortalCoordinators, Job and Server exes
            Utilities MyUtils = new Utilities();
            MyUtils.InProcTestCleanup();
        }


    }
}
