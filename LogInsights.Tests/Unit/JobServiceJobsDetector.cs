using LogInsights.Datastore;
using LogInsights.LogReader;
using NUnit.Framework;
using System;

namespace LogInsights.Tests.Unit
{
    public class JobServiceJobsDetector
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var logEntry = new LogEntry()
            {
                Logger = "Jobservice", 
                NumberOfLines = 1,
                Message = "VI.JobService.JobComponents.ScriptComponent - 4615371a-c92b-473a-8421-488a065cf9fe: Successful", 
                Spid = "42",
                TimeStamp = DateTime.UtcNow
            };

            var datastore = new DataStore();
            var detector = new Detectors.JobServiceJobsDetector();
            detector.datastore = datastore;
            detector.InitializeDetector();
            detector.isEnabled = true;

            detector.ProcessMessage(logEntry);

            detector.FinalizeDetector();

            
            var jobServiceActivities = datastore.GetOrAdd<JobServiceActivity>();
            Assert.That(jobServiceActivities.HasData, "Data was found");
            Assert.AreEqual(1, jobServiceActivities.JobServiceJobs.Count, "Count job was found");
            var job = jobServiceActivities.JobServiceJobs[0];
            Assert.AreEqual("VI.JobService.JobComponents.ScriptComponent", job.componentname, nameof(job.componentname));
            Assert.True(job.hasMissingStartOrFinishMessages, nameof(job.hasMissingStartOrFinishMessages));
            Assert.AreEqual("Success", job.finaleStatus, nameof(job.finaleStatus));
            Assert.AreEqual("4615371a-c92b-473a-8421-488a065cf9fe", job.uidJob, nameof(job.uidJob));
            Assert.AreEqual("", job.taskname, nameof(job.taskname));
            Assert.AreEqual("", job.queuename, nameof(job.queuename));
            //Assert.Pass();
        }

        #region test bodys

        [TestCaseSource(nameof(TestSingleMessageSource))]
        public void TestSingleMessage(string messageText, bool hasData, int jobCount, string uidJob, string jobComponent, string jobTask, string queue, bool isIncomplete, string finalState)
        {
            // prepare log entry to process
            var logEntry = new LogEntry()
            {
                Logger = "Jobservice", 
                NumberOfLines = 1,
                Message = messageText, 
                Spid = "42",
                TimeStamp = DateTime.UtcNow
            };

            // setup detector
            var datastore = new DataStore();
            var detector = new Detectors.JobServiceJobsDetector();
            detector.datastore = datastore;
            detector.InitializeDetector();
            detector.isEnabled = true;

            // run processing 
            detector.ProcessMessage(logEntry);

            detector.FinalizeDetector();

            // fetch results and check
            var jobServiceActivities = datastore.GetOrAdd<JobServiceActivity>();
            Assert.AreEqual(hasData, jobServiceActivities.HasData, nameof(jobServiceActivities.HasData));
            Assert.AreEqual(jobCount, jobServiceActivities.JobServiceJobs.Count, nameof(jobServiceActivities.JobServiceJobs.Count));
            if(jobCount<1)
                return;
            var job = jobServiceActivities.JobServiceJobs[0];
            Assert.AreEqual(uidJob, job.uidJob, nameof(job.uidJob));
            Assert.AreEqual(jobComponent, job.componentname, nameof(job.componentname));
            Assert.AreEqual(jobTask, job.taskname, nameof(job.taskname));
            Assert.AreEqual(queue, job.queuename, nameof(job.queuename));
            Assert.AreEqual(isIncomplete, job.hasMissingStartOrFinishMessages, nameof(job.hasMissingStartOrFinishMessages));
            Assert.AreEqual(finalState, job.finaleStatus, nameof(job.finaleStatus));
            
        }

        #endregion

        #region test case data

        // set of single log messages test cases
        public static System.Collections.IEnumerable TestSingleMessageSource
        {
            get
            {
                // success message nlog style
                yield return new TestCaseData(
                    "VI.JobService.JobComponents.ScriptComponent - 4615371a-c92b-473a-8421-488a065cf9fe: Successful",
                    true, 1, "4615371a-c92b-473a-8421-488a065cf9fe", "VI.JobService.JobComponents.ScriptComponent", "", "", true, "Success");
                
                // start message with process step parameters jobservice.log style
                yield return new TestCaseData(
                    @"\SI0VM2487 - Process step parameter 301801F8-ED8A-4395-B8DC-02F4BBD365AERA:
[Job]
	ComponentAssembly=JobService
	ComponentClass=VI.JobService.JobComponents.JobCheckComponent
	Task=CheckJob
	Executiontype=INTERNAL
[Parameters]
	Queue=\SI0VM2487
	uid_job=f18af915-6fe0-44e5-9249-2d2aa74dba4d
	uid_self=301801F8-ED8A-4395-B8DC-02F4BBD365AERA",
                    true, 1, "301801F8-ED8A-4395-B8DC-02F4BBD365AERA", "VI.JobService.JobComponents.JobCheckComponent", "CheckJob", @"\SI0VM2487", true, "Unknown");
                
                // success message jobservice.log style
                yield return new TestCaseData(
                    @"\SI0VM2487 - VI.JobService.JobComponents.ScriptComponent - cb628d8a-127f-47dc-b696-c1213e873293: Successful",
                    true, 1, "cb628d8a-127f-47dc-b696-c1213e873293", "VI.JobService.JobComponents.ScriptComponent", "", @"\SI0VM2487", true, "Success");

                // out parameter message jobservice.log style
                yield return new TestCaseData(
                    @"\SI0VM2487 - Process step output parameter cb628d8a-127f-47dc-b696-c1213e873293:
	Value=False",
                    true, 1, "cb628d8a-127f-47dc-b696-c1213e873293", "", "", @"\SI0VM2487", true, "Success");
                
                // messages of no interest jobservice.log style
                yield return new TestCaseData(
                    @"Requesting process steps for queue \SI0VM2487.",
                    false, 0, "", "", "", "", false, "Unknown");
                yield return new TestCaseData(
                    @"Last process step request succeeded.",
                    false, 0, "", "", "", "", false, "Unknown");



            }
            
        }

        #endregion


    }
}
