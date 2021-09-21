**One Identity open source projects are supported through [One Identity GitHub issues](https://github.com/OneIdentity/IdentityManager.LogInsights/issues) and the [One Identity Community](https://www.oneidentity.com/community/). This includes all scripts, plugins, SDKs, modules, code snippets or other solutions. For assistance with any One Identity GitHub project, please raise a new Issue on the [One Identity GitHub project](https://github.com/OneIdentity/IdentityManager.LogInsights/issues) page. You may also visit the [One Identity Community](https://www.oneidentity.com/community/) to ask questions.  Requests for assistance made through official One Identity Support will be referred back to GitHub and the One Identity Community forums where those requests can benefit all users.**

Log insights for One Identity Manager logs
==========================================

![LogfileMetaAnalyser screen shot](./LogfileMetaAnalyser.png)

Can read text files (a single one, a multi select or a file system folder) of following types:
- NLog with default OneIM config (e.g. StdioProcessor.log)
- Jobservice.log produced by OneIM Jobservice service


What does it do?
----------------

- it will read the provided log files and try to detect their type
- all lines are send into a buffer to unite all lines that belong to a message (which need to start with a time stamp)
- those messages are passed into several parsers, each of them will subscribe to a specific topic
	- detecting time ranges of all logfiles to provide a time line for all of them
	- detecting time gaps between messages, which references to possible stuck situations
	- detecting Jobservice process steps, each of them have a request and response which should be tied together, detect errors and warning results for process steps
	- detecting OneIM synchronization AdHoc and FullSync processes, including start and finish event, type and target system name to provide an overall activity
	- detecting OneIM synchronization information about involved Connectors and their activity
	- detecting OneIM synchronization activity (e.g. loading data from a specific target system)
	- detecting OneIM synchronization journal reports
	- detecting database SQL commands and their duration, detecting SQL transactions and long running queries
- finally all gathered information are presented in an UI
- there is a possibility to filter the log files for certain messages, based on gathered IDs, message types and activity flows and threads


Components:
-----------

- user interface as client application
- log file text reader
- parser for log messages
- detectors which analyze the messages
- storing information into an internal data store structure
- each detector has its own result presenting UI component to present its own results based on a specific topic
- message export functionality

