using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(DiffReporter))]
[assembly: FrontLoadedReporter(typeof(DefaultFrontLoaderReporter))]
